import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { NewsArticleView } from "@/components/news/NewsArticleView";
import { getNewsBySlug, newsArticles } from "@/lib/news";

type NewsArticlePageProps = {
  params: Promise<{ slug: string }>;
};

export function generateStaticParams() {
  return newsArticles.map((article) => ({ slug: article.slug }));
}

export async function generateMetadata({
  params,
}: NewsArticlePageProps): Promise<Metadata> {
  const { slug } = await params;
  const article = getNewsBySlug(slug);
  if (!article) {
    return { title: "News | URVP" };
  }
  return {
    title: `${article.title} | URVP News`,
    description: article.excerpt,
  };
}

export default async function NewsArticlePage({ params }: NewsArticlePageProps) {
  const { slug } = await params;
  const article = getNewsBySlug(slug);
  if (!article) {
    notFound();
  }

  return <NewsArticleView article={article} />;
}
