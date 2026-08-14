import type { Metadata } from "next";
import { AdminProjectDetailView } from "@/components/admin/AdminProjectDetailView";

export const metadata: Metadata = {
  title: "Project | Admin",
  description: "Project details and students who ranked this listing.",
};

type AdminProjectPageProps = {
  params: Promise<{ id: string }>;
};

export default async function AdminProjectPage({ params }: AdminProjectPageProps) {
  const { id } = await params;
  return <AdminProjectDetailView projectId={id} />;
}
