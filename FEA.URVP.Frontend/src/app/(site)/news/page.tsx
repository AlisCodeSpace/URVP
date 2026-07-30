import type { Metadata } from "next";
import { Button } from "@/components/ui/Button";
import { PageHero } from "@/components/layout/PageHero";
import { NewsList } from "@/components/news/NewsList";
import { newsIntro } from "@/lib/news";

export const metadata: Metadata = {
  title: "News | URVP",
  description:
    "URVP news, deadlines, and program updates for undergraduate research volunteers at AUB.",
};

type NewsPageProps = {
  searchParams: Promise<{ page?: string }>;
};

export default async function NewsPage({ searchParams }: NewsPageProps) {
  const { page: pageParam } = await searchParams;
  const parsed = Number(pageParam);
  const page = Number.isFinite(parsed) && parsed > 0 ? Math.floor(parsed) : 1;

  return (
    <main className="flex-1 bg-background">
      <PageHero
        title="News"
        headline="Deadlines, milestones, and program updates."
        description={newsIntro}
        actions={
          <Button href="#news-list" variant="secondary" size="lg">
            Latest stories
          </Button>
        }
      />

      <div id="news-list" className="scroll-mt-24">
        <NewsList page={page} />
      </div>
    </main>
  );
}
