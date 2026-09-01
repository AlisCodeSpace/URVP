import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
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
    <RequireAuth>
      <main className="flex-1 bg-background">
        <ProjectDetailLoader id={id} />
      </main>
    </RequireAuth>
  );
}
