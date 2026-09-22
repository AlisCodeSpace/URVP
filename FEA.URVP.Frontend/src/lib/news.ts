export type NewsArticle = {
  slug: string;
  title: string;
  excerpt: string;
  category: string;
  date: string;
  dateISO: string;
  author: string;
  featured?: boolean;
  /** Short line for home marquee */
  ticker: string;
  body: string[];
  images: string[];
};

export const newsIntro =
  "Announcements, cycle updates, and stories from undergraduates and mentors across AUB faculties.";

export const NEWS_PAGE_SIZE = 3;

export function pickFeaturedNews(articles: NewsArticle[]): NewsArticle | undefined {
  if (articles.length === 0) return undefined;
  return articles.find((article) => article.featured) ?? articles[0];
}

export function paginateNewsArticles(
  articles: NewsArticle[],
  page: number,
): {
  items: NewsArticle[];
  page: number;
  totalPages: number;
  total: number;
} {
  const featured = pickFeaturedNews(articles);
  const list = featured
    ? articles.filter((article) => article.slug !== featured.slug)
    : articles;
  const total = list.length;
  const totalPages = Math.max(1, Math.ceil(total / NEWS_PAGE_SIZE) || 1);
  const safePage = Math.min(Math.max(1, page), totalPages);
  const start = (safePage - 1) * NEWS_PAGE_SIZE;

  return {
    items: list.slice(start, start + NEWS_PAGE_SIZE),
    page: safePage,
    totalPages,
    total,
  };
}

export function getNewsNeighborsFrom(
  articles: NewsArticle[],
  slug: string,
): {
  previous: NewsArticle | null;
  next: NewsArticle | null;
} {
  const index = articles.findIndex((article) => article.slug === slug);
  if (index === -1) {
    return { previous: null, next: null };
  }
  return {
    previous: index > 0 ? articles[index - 1] : null,
    next: index < articles.length - 1 ? articles[index + 1] : null,
  };
}
