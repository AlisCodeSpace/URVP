import type { Metadata } from "next";
import { EditProjectView } from "@/components/projects/EditProjectView";

export const metadata: Metadata = {
  title: "Edit Project | URVP",
  description: "Update a research project listing in the URVP portal.",
};

type EditProjectPageProps = {
  params: Promise<{ userId: string; projectId: string }>;
};

export default async function EditProjectPage({ params }: EditProjectPageProps) {
  const { userId, projectId } = await params;
  return <EditProjectView userId={userId} projectId={projectId} />;
}
