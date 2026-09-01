"use client";

import { useEffect, useState } from "react";
import { notFound } from "next/navigation";
import { NewsArticleView } from "@/components/news/NewsArticleView";
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

  if (status === "missing") {
    notFound();
  }

  if (status === "loading" || !result) {
    return (
      <div className="site-container py-16">
        <p className="text-muted">Loading story…</p>
      </div>
    );
  }

  return (
    <NewsArticleView
      article={result.article}
      previous={result.previous}
      next={result.next}
    />
  );
}
