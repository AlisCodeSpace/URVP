"use client";

import { useEffect, useState } from "react";
import { NewsArticleView } from "@/components/news/NewsArticleView";
import { NewsArticleSkeleton } from "@/components/ui/SectionSkeletons";
import { NotFoundView } from "@/components/ui/NotFoundView";
import { loadPublicNewsArticle } from "@/lib/news-api";
import type { NewsArticle } from "@/lib/news";

export function NewsArticleLoader({ slug }: { slug: string }) {
  const [result, setResult] = useState<{
    article: NewsArticle;
    previous: NewsArticle | null;
    next: NewsArticle | null;
  } | null>(null);
  const [status, setStatus] = useState<"loading" | "ready" | "missing">(
    "loading",
  );

  useEffect(() => {
    let cancelled = false;
    void loadPublicNewsArticle(slug).then((next) => {
      if (cancelled) return;
      if (!next) {
        setStatus("missing");
        return;
      }
      setResult(next);
      setStatus("ready");
    });
    return () => {
      cancelled = true;
    };
  }, [slug]);

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
