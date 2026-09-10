import { ApiError, apiFetch } from "@/lib/api";
import { getApiBaseUrl } from "@/lib/config";
import { formatAppDate } from "@/lib/datetime";
import {
  getNewsNeighborsFrom,
  newsArticles,
  type NewsArticle,
} from "@/lib/news";
import type { FileMetadataDto } from "@/lib/student-profile-api";

export type NewsArticleDto = {
  id: string;
  slug: string;
  title: string;
  excerpt: string;
  category: string;
  author: string;
  ticker: string;
  body: string[];
  imageFileIds: string[];
  publishedAt: string;
  featured: boolean;
  published: boolean;
  createdAt: string;
  updatedAt: string;
};

export type PaginatedNews = {
  items: NewsArticleDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
};

export type NewsWritePayload = {
  slug?: string;
  title: string;
  excerpt: string;
  category: string;
  author: string;
  ticker: string;
  body: string[];
  imageFileIds?: string[];
  publishedAt: string;
  featured: boolean;
  published?: boolean;
};

export const NEWS_CATEGORIES = [
  "Announcement",
  "Cycle",
  "Deadline",
  "Event",
  "Faculty",
  "Milestone",
  "Workshop",
] as const;

export const MAX_NEWS_IMAGES = 12;
/** Matches `FileStorage:MaxImageSizeBytes` (2 MB). */
export const MAX_NEWS_IMAGE_BYTES = 2 * 1024 * 1024;
/** Cap for all photos on one article (12 × 2 MB). */
export const MAX_NEWS_IMAGES_TOTAL_BYTES = MAX_NEWS_IMAGES * MAX_NEWS_IMAGE_BYTES;

export function formatNewsDate(iso: string): string {
  return formatAppDate(iso);
}

export function newsImageUrl(fileId: string | null | undefined): string | undefined {
  if (!fileId) return undefined;
  return `${getApiBaseUrl()}/api/files/${fileId}`;
}

export function toNewsArticle(dto: NewsArticleDto): NewsArticle {
  return {
    slug: dto.slug,
    title: dto.title,
    excerpt: dto.excerpt,
    category: dto.category,
    date: formatNewsDate(dto.publishedAt),
    dateISO: dto.publishedAt.slice(0, 10),
    author: dto.author,
    featured: dto.featured,
    ticker: dto.ticker,
    body: dto.body,
    images: (dto.imageFileIds ?? [])
      .map((id) => newsImageUrl(id))
      .filter((url): url is string => Boolean(url)),
  };
}

export async function listNews(params: {
  search?: string;
  publishedOnly?: boolean;
  pageNumber?: number;
  pageSize?: number;
} = {}): Promise<PaginatedNews> {
  const query = new URLSearchParams();
  if (params.search?.trim()) query.set("search", params.search.trim());
  if (params.publishedOnly) query.set("publishedOnly", "true");
  query.set("pageNumber", String(params.pageNumber ?? 1));
  query.set("pageSize", String(params.pageSize ?? 100));
  return apiFetch<PaginatedNews>(`/api/news?${query.toString()}`);
}

export async function getNewsById(id: string): Promise<NewsArticleDto> {
  return apiFetch<NewsArticleDto>(`/api/news/${id}`);
}

export async function getNewsBySlug(slug: string): Promise<NewsArticleDto> {
  return apiFetch<NewsArticleDto>(`/api/news/slug/${encodeURIComponent(slug)}`);
}

export async function createNews(
  payload: NewsWritePayload,
): Promise<NewsArticleDto> {
  return apiFetch<NewsArticleDto>("/api/news", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export async function updateNews(
  id: string,
  payload: NewsWritePayload,
): Promise<NewsArticleDto> {
  return apiFetch<NewsArticleDto>(`/api/news/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export async function deleteNews(id: string): Promise<void> {
  await apiFetch<null>(`/api/news/${id}`, { method: "DELETE" });
}

export async function uploadNewsImage(
  newsId: string,
  file: File,
): Promise<FileMetadataDto> {
  const body = new FormData();
  body.append("file", file);
  body.append("entityType", "NewsArticle");
  body.append("entityId", newsId);
  body.append("fileCategory", "NewsImage");
  return apiFetch<FileMetadataDto>("/api/files", {
    method: "POST",
    body,
  });
}

export async function loadPublicNews(): Promise<NewsArticle[]> {
  try {
    const page = await listNews({ pageNumber: 1, pageSize: 200, publishedOnly: true });
    return page.items.map(toNewsArticle);
  } catch (err) {
    if (err instanceof ApiError && err.status === 404) {
      return [];
    }
    return newsArticles;
  }
}

export async function loadPublicNewsArticle(slug: string): Promise<{
  article: NewsArticle;
  previous: NewsArticle | null;
  next: NewsArticle | null;
} | null> {
  const articles = await loadPublicNews();
  const article = articles.find((item) => item.slug === slug);
  if (!article) return null;
  return { article, ...getNewsNeighborsFrom(articles, slug) };
}
