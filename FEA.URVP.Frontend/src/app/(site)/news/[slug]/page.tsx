import type { Metadata } from "next";
import { NewsArticleLoader } from "@/components/news/NewsArticleLoader";
import { getNewsBySlug } from "@/lib/news";
import { loadPublicNewsArticle } from "@/lib/news-api";

type NewsArticlePageProps = {
  params: Promise<{ slug: string }>;
};

export async function generateMetadata({
  params,
}: NewsArticlePageProps): Promise<Metadata> {
  const { slug } = await params;
  const result = await loadPublicNewsArticle(slug);
  const article = result?.article ?? getNewsBySlug(slug);
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
  return <NewsArticleLoader slug={slug} />;
}
