import type { Metadata } from "next";
import { AdminNewsForm } from "@/components/admin/AdminNewsForm";

export const metadata: Metadata = {
  title: "Edit news | Admin",
  description: "Update a news story.",
};

type AdminEditNewsPageProps = {
  params: Promise<{ id: string }>;
};

export default async function AdminEditNewsPage({
  params,
}: AdminEditNewsPageProps) {
  const { id } = await params;
  return <AdminNewsForm newsId={id} />;
}
