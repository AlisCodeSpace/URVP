import Link from "next/link";
import { Heading, Text } from "@radix-ui/themes";
import type { NewsArticle } from "@/lib/news";
import {
  NEWS_PAGE_SIZE,
  getFeaturedNews,
  getNewsPage,
} from "@/lib/news";

function formatIndex(i: number) {
  return String(i + 1).padStart(2, "0");
}

function newsPageHref(page: number) {
  return page <= 1 ? "/news#news-list" : `/news?page=${page}#news-list`;
}

export function NewsFeatured({ article }: { article: NewsArticle }) {
  return (
    <article className="border-b border-primary/10 bg-surface">
      <div className="site-container grid gap-8 py-14 sm:py-16 lg:grid-cols-[1fr_1.35fr] lg:items-start lg:gap-16">
        <div>
          <Text
            as="p"
            size="2"
            weight="medium"
            className="!uppercase !tracking-[0.22em] !text-secondary-deep"
          >
            Featured · {article.category}
          </Text>
          <time
            dateTime={article.dateISO}
            className="mt-4 block text-sm uppercase tracking-[0.16em] text-muted"
          >
            {article.date}
          </time>
        </div>
        <div>
          <Heading
            as="h2"
            size="7"
            weight="medium"
            className="!font-[family-name:var(--font-display)] !leading-[1.08] !text-primary"
          >
            <Link
              href={`/news/${article.slug}`}
              className="transition hover:text-primary-soft"
            >
              {article.title}
            </Link>
          </Heading>
          <Text
            as="p"
            size="3"
            mt="4"
            className="max-w-xl !leading-relaxed !text-muted"
          >
            {article.excerpt}
          </Text>
          <Link
            href={`/news/${article.slug}`}
            className="mt-8 inline-flex items-center gap-2 text-sm font-medium text-secondary-deep transition hover:gap-3"
          >
            Read story
            <span aria-hidden>→</span>
          </Link>
        </div>
      </div>
    </article>
  );
}

function NewsRow({
  article,
  index,
}: {
  article: NewsArticle;
  index: number;
}) {
  return (
    <li>
      <Link
        href={`/news/${article.slug}`}
        className="news-row group grid gap-4 border-b border-primary/10 py-8 transition sm:grid-cols-[4.5rem_7.5rem_1fr_auto] sm:items-baseline sm:gap-6"
      >
        <span
          aria-hidden
          className="font-[family-name:var(--font-display)] text-2xl font-medium text-secondary-deep/80 transition group-hover:text-secondary-deep"
        >
          {formatIndex(index)}
        </span>
        <div className="flex flex-col gap-1">
          <span className="text-xs font-semibold uppercase tracking-[0.18em] text-secondary-deep">
            {article.category}
          </span>
          <time dateTime={article.dateISO} className="text-sm text-muted">
            {article.date}
          </time>
        </div>
        <div>
          <Heading
            as="h3"
            size="5"
            weight="medium"
            className="!font-[family-name:var(--font-display)] !leading-snug !text-primary transition group-hover:!text-primary-soft"
          >
            {article.title}
          </Heading>
          <Text
            as="p"
            size="2"
            mt="2"
            className="max-w-2xl !leading-relaxed !text-muted"
          >
            {article.excerpt}
          </Text>
        </div>
        <span
          aria-hidden
          className="hidden text-secondary transition-transform duration-300 group-hover:translate-x-1 sm:inline"
        >
          →
        </span>
      </Link>
    </li>
  );
}

function NewsPagination({
  page,
  totalPages,
}: {
  page: number;
  totalPages: number;
}) {
  if (totalPages <= 1) return null;

  const pages = Array.from({ length: totalPages }, (_, i) => i + 1);

  return (
    <nav
      aria-label="News pagination"
      className="mt-10 flex flex-col items-center gap-4 border-t border-primary/10 pt-8 sm:flex-row sm:justify-between"
    >
      <Text as="p" size="2" className="!text-muted">
        Page {page} of {totalPages}
      </Text>

      <div className="flex flex-wrap items-center justify-center gap-2">
        {page > 1 ? (
          <Link
            href={newsPageHref(page - 1)}
            className="btn btn-outline btn-sm"
            rel="prev"
          >
            Previous
          </Link>
        ) : (
          <span className="btn btn-outline btn-sm pointer-events-none opacity-40">
            Previous
          </span>
        )}

        {pages.map((n) => (
          <Link
            key={n}
            href={newsPageHref(n)}
            aria-label={`Page ${n}`}
            aria-current={n === page ? "page" : undefined}
            className={
              n === page
                ? "btn btn-primary btn-sm min-w-10"
                : "btn btn-outline btn-sm min-w-10"
            }
          >
            {n}
          </Link>
        ))}

        {page < totalPages ? (
          <Link
            href={newsPageHref(page + 1)}
            className="btn btn-outline btn-sm"
            rel="next"
          >
            Next
          </Link>
        ) : (
          <span className="btn btn-outline btn-sm pointer-events-none opacity-40">
            Next
          </span>
        )}
      </div>
    </nav>
  );
}

export function NewsList({ page = 1 }: { page?: number }) {
  const featured = getFeaturedNews();
  const { items, page: currentPage, totalPages, total } = getNewsPage(page);
  const startIndex = (currentPage - 1) * NEWS_PAGE_SIZE;

  return (
    <div>
      {currentPage === 1 ? <NewsFeatured article={featured} /> : null}

      <section className="site-container py-14 sm:py-16">
        <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
          <Heading
            as="h2"
            size="7"
            weight="medium"
            className="!font-[family-name:var(--font-display)] !text-primary"
          >
            Latest updates
          </Heading>
          <Text as="p" size="2" className="!text-muted">
            {total} {total === 1 ? "story" : "stories"}
            {totalPages > 1 ? ` · page ${currentPage}` : null}
          </Text>
        </div>

        <ol className="mt-4 border-t border-primary/10">
          {items.map((article, index) => (
            <NewsRow
              key={article.slug}
              article={article}
              index={startIndex + index}
            />
          ))}
        </ol>

        <NewsPagination page={currentPage} totalPages={totalPages} />
      </section>
    </div>
  );
}
