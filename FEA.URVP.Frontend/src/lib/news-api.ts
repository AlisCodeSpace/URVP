import { ApiError, apiFetch } from "@/lib/api";
import {
  getNewsNeighborsFrom,
  newsArticles,
  type NewsArticle,
} from "@/lib/news";

export type NewsArticleDto = {
  id: string;
  slug: string;
  title: string;
  excerpt: string;
  category: string;
  author: string;
  ticker: string;
  body: string[];
  publishedAt: string;
  featured: boolean;
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
  publishedAt: string;
  featured: boolean;
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

export function formatNewsDate(iso: string): string {
  const day = iso.slice(0, 10);
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(day);
  if (!match) return iso;
  const date = new Date(
    Date.UTC(Number(match[1]), Number(match[2]) - 1, Number(match[3])),
  );
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
    timeZone: "UTC",
  });
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
  };
}

export async function listNews(params: {
  search?: string;
  pageNumber?: number;
  pageSize?: number;
} = {}): Promise<PaginatedNews> {
  const query = new URLSearchParams();
  if (params.search?.trim()) query.set("search", params.search.trim());
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

export async function loadPublicNews(): Promise<NewsArticle[]> {
  try {
    const page = await listNews({ pageNumber: 1, pageSize: 200 });
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
