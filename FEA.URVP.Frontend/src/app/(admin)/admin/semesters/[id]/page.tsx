import type { Metadata } from "next";
import { AdminSemesterForm } from "@/components/admin/AdminSemesterForm";

export const metadata: Metadata = {
  title: "Edit semester | Admin",
  description: "Update a semester's details and application window.",
};

type AdminEditSemesterPageProps = {
  params: Promise<{ id: string }>;
};

export default async function AdminEditSemesterPage({
  params,
}: AdminEditSemesterPageProps) {
  const { id } = await params;
  return <AdminSemesterForm semesterId={id} />;
}
