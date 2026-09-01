import type { Metadata } from "next";
import { AdminSemesterForm } from "@/components/admin/AdminSemesterForm";

export const metadata: Metadata = {
  title: "New semester | Admin",
  description: "Create a new academic semester.",
};

export default function AdminNewSemesterPage() {
  return <AdminSemesterForm />;
}
