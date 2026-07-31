import type { Metadata } from "next";
import { ProjectDetailLoader } from "@/components/projects/ProjectDetailLoader";

type ProjectPageProps = {
  params: Promise<{ id: string }>;
};

export async function generateMetadata({
  params,
}: ProjectPageProps): Promise<Metadata> {
  const { id } = await params;
  return {
    title: "Project | URVP",
    description: `Research opportunity ${id}`,
  };
}

export default async function ProjectPage({ params }: ProjectPageProps) {
  const { id } = await params;

  return (
    <main className="flex-1 bg-background">
      <section className="mx-auto max-w-6xl px-6 py-14 sm:py-16">
        <ProjectDetailLoader id={id} />
      </section>
    </main>
  );
}
