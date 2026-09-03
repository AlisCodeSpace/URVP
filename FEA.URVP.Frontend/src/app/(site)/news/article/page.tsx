import type { Metadata } from "next";
import { Suspense } from "react";
import { NewsArticleRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

/**
 * Static, article-independent metadata. Per-article titles would need the article at build time,
 * which a static export cannot fetch for an arbitrary slug.
 */
export const metadata: Metadata = {
  title: "News | URVP News",
  description:
    "Deadlines, workshops, and stories from the Undergraduate Research Volunteer Program.",
};

export default function NewsArticlePage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <NewsArticleRoute />
    </Suspense>
  );
}
