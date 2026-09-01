import type { Metadata } from "next";
import { AdminSemestersView } from "@/components/admin/AdminSemestersView";

export const metadata: Metadata = {
  title: "Semesters | Admin",
  description: "Manage academic semesters, cycles, and application windows.",
};

export default function AdminSemestersPage() {
  return <AdminSemestersView />;
}
