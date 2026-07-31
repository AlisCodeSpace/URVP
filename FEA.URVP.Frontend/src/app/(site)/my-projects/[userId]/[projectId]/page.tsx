import type { Metadata } from "next";
import { FacultyProjectView } from "@/components/projects/FacultyProjectView";

export const metadata: Metadata = {
  title: "View Project | URVP",
  description: "Review a research project you posted in the URVP portal.",
};

type ViewProjectPageProps = {
  params: Promise<{ userId: string; projectId: string }>;
};

export default async function ViewProjectPage({ params }: ViewProjectPageProps) {
  const { userId, projectId } = await params;
  return <FacultyProjectView userId={userId} projectId={projectId} />;
}
