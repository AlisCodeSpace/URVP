import Link from "next/link";
import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import type { NewsArticle } from "@/lib/news";
import { getNewsNeighbors } from "@/lib/news";

export function NewsArticleView({ article }: { article: NewsArticle }) {
  const { previous, next } = getNewsNeighbors(article.slug);

  return (
    <article className="flex-1 bg-background">
      <header className="news-article-hero relative overflow-hidden text-white">
        <div className="news-article-hero-grid absolute inset-0" aria-hidden />
        <div className="relative z-10 mx-auto max-w-3xl px-6 pb-16 pt-28 sm:pb-20 sm:pt-32">
          <Link
            href="/news"
            className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
          >
            <span aria-hidden>←</span>
            All news
          </Link>

          <div className="mt-8 flex flex-wrap items-center gap-x-4 gap-y-2">
            <Text
              as="p"
              size="2"
              weight="medium"
              className="!uppercase !tracking-[0.2em] !text-secondary"
            >
              {article.category}
            </Text>
            <span className="hidden text-white/30 sm:inline" aria-hidden>
              ·
            </span>
            <time
              dateTime={article.dateISO}
              className="text-sm uppercase tracking-[0.14em] text-white/60"
            >
              {article.date}
            </time>
          </div>

          <Heading
            as="h1"
            size="8"
            weight="medium"
            mt="4"
            className="animate-fade-up !font-[family-name:var(--font-display)] !leading-[1.08] !text-white"
          >
            {article.title}
          </Heading>

          <Text
            as="p"
            size="4"
            mt="5"
            className="animate-fade-up-delay max-w-2xl !leading-relaxed !text-white/75"
          >
            {article.excerpt}
          </Text>

          <p className="animate-fade-up-delay-2 mt-8 text-sm text-white/55">
            By{" "}
            <span className="font-medium text-white/80">{article.author}</span>
          </p>
        </div>
      </header>

      <div className="mx-auto max-w-3xl px-6 py-14 sm:py-16">
        <div className="news-article-body">
          {article.body.map((paragraph, i) => (
            <Text
              as="p"
              key={i}
              size="4"
              className="!leading-[1.75] !text-foreground/90"
            >
              {paragraph}
            </Text>
          ))}
        </div>

        <aside className="news-pullquote mt-14 rounded-lg px-6 py-7 sm:px-8">
          <Text
            as="p"
            size="5"
            className="!font-[family-name:var(--font-display)] !leading-snug !text-primary"
          >
            “{article.excerpt}”
          </Text>
          <Text as="p" size="2" mt="3" className="!text-muted">
            — {article.author}
          </Text>
        </aside>

        <div className="mt-12 flex flex-wrap gap-3 border-t border-primary/10 pt-8">
          <Button href="/news" variant="outline" size="md">
            Back to news
          </Button>
          <Button href="/research-day" variant="ghost" size="md">
            Research Day
          </Button>
          <Button href="/workshops" variant="ghost" size="md">
            Workshops
          </Button>
        </div>
      </div>

      <nav
        aria-label="Adjacent stories"
        className="border-t border-primary/10 bg-surface"
      >
        <div className="mx-auto grid max-w-6xl sm:grid-cols-2">
          {previous ? (
            <Link
              href={`/news/${previous.slug}`}
              className="group border-b border-primary/10 px-6 py-10 transition hover:bg-background sm:border-b-0 sm:border-r"
            >
              <Text
                as="p"
                size="1"
                weight="bold"
                className="!uppercase !tracking-[0.18em] !text-secondary-deep"
              >
                Previous
              </Text>
              <Heading
                as="h2"
                size="4"
                weight="medium"
                mt="3"
                className="!font-[family-name:var(--font-display)] !text-primary transition group-hover:!text-primary-soft"
              >
                {previous.title}
              </Heading>
            </Link>
          ) : (
            <div className="hidden sm:block" />
          )}
          {next ? (
            <Link
              href={`/news/${next.slug}`}
              className="group px-6 py-10 text-left transition hover:bg-background sm:text-right"
            >
              <Text
                as="p"
                size="1"
                weight="bold"
                className="!uppercase !tracking-[0.18em] !text-secondary-deep"
              >
                Next
              </Text>
              <Heading
                as="h2"
                size="4"
                weight="medium"
                mt="3"
                className="!font-[family-name:var(--font-display)] !text-primary transition group-hover:!text-primary-soft"
              >
                {next.title}
              </Heading>
            </Link>
          ) : null}
        </div>
      </nav>
    </article>
  );
}
