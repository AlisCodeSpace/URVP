import type { Metadata } from "next";
import { AdminSemesterForm } from "@/components/admin/AdminSemesterForm";

export const metadata: Metadata = {
  title: "New URVP cycle | Admin",
  description: "Create a new URVP cycle.",
};

export default function AdminNewSemesterPage() {
  return <AdminSemesterForm />;
}
