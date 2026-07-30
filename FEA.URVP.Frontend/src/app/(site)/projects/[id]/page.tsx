import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { ProjectDetail } from "@/components/projects/ProjectDetail";
import { catalogProjects, getProjectById } from "@/lib/projects";

type ProjectPageProps = {
  params: Promise<{ id: string }>;
};

export function generateStaticParams() {
  return catalogProjects.map((project) => ({ id: project.id }));
}

export async function generateMetadata({
  params,
}: ProjectPageProps): Promise<Metadata> {
  const { id } = await params;
  const project = getProjectById(id);
  if (!project) {
    return { title: "Project | URVP" };
  }
  return {
    title: `${project.title} | URVP`,
    description: project.description,
  };
}

export default async function ProjectPage({ params }: ProjectPageProps) {
  const { id } = await params;
  const project = getProjectById(id);
  if (!project) notFound();

  return (
    <main className="flex-1 bg-background">
      <section className="mx-auto max-w-6xl px-6 py-14 sm:py-16">
        <ProjectDetail project={project} />
      </section>
    </main>
  );
}
