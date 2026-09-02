import type { Metadata } from "next";
import { AdminMatchingRunDetailView } from "@/components/admin/AdminMatchingRunDetailView";

export const metadata: Metadata = {
  title: "Matching run | Admin",
  description: "Review proposed placements, warnings, and confirm or discard the run.",
};

type AdminMatchingRunPageProps = {
  params: Promise<{ id: string }>;
};

export default async function AdminMatchingRunPage({ params }: AdminMatchingRunPageProps) {
  const { id } = await params;
  return <AdminMatchingRunDetailView runId={id} />;
}
