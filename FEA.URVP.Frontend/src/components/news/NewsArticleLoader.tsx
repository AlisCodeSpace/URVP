"use client";

import { NewsArticleView } from "@/components/news/NewsArticleView";
import { NewsArticleSkeleton } from "@/components/ui/SectionSkeletons";
import { NotFoundView } from "@/components/ui/NotFoundView";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { loadPublicNewsArticle } from "@/lib/news-api";

export function NewsArticleLoader({ slug }: { slug: string }) {
  const articleQuery = useCancellableQuery(async () => {
    try {
      const next = await loadPublicNewsArticle(slug);
      if (!next) return { status: "missing" as const, result: null };
      return { status: "ready" as const, result: next };
    } catch {
      return { status: "failed" as const, result: null };
    }
  }, [slug]);

  const status = articleQuery.data?.status ?? "loading";
  const result = articleQuery.data?.result ?? null;

  // Rendered inline rather than via notFound(): the article is fetched in the browser, and
  // notFound() belongs to server rendering, which a static export does not perform.
  if (status === "missing") {
    return (
      <NotFoundView
        title="Story not found"
        description="This news story is no longer available. Browse the latest updates below."
      />
    );
  }

  if (status === "failed") {
    return (
      <NotFoundView
        title="Story unavailable"
        description="This news story could not be loaded right now. Please refresh to try again."
      />
    );
  }

  if (status === "loading" || !result) {
    return <NewsArticleSkeleton />;
  }

  return (
    <NewsArticleView
      article={result.article}
      previous={result.previous}
      next={result.next}
    />
  );
}
