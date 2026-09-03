import type { Metadata } from "next";
import { Suspense } from "react";
import { PageHeader } from "@/components/layout/PageHeader";
import { NewsList } from "@/components/news/NewsList";
import { NewsListSkeleton } from "@/components/ui/SectionSkeletons";
import { newsIntro } from "@/lib/news";

export const metadata: Metadata = {
  title: "News | URVP",
  description:
    "URVP news, deadlines, and program updates for undergraduate research volunteers at AUB.",
};

export default function NewsPage() {
  return (
    <main className="flex-1 bg-background">
      <PageHeader title="News" description={newsIntro} />

      <div id="news-list" className="scroll-mt-24">
        <Suspense fallback={<NewsListSkeleton />}>
          <NewsList />
        </Suspense>
      </div>
    </main>
  );
}
