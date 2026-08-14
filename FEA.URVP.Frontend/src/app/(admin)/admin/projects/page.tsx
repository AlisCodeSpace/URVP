import type { Metadata } from "next";
import { AdminProjectsView } from "@/components/admin/AdminProjectsView";

export const metadata: Metadata = {
  title: "Projects | Admin",
  description: "All research listings and student ranking interest.",
};

export default function AdminProjectsPage() {
  return <AdminProjectsView />;
}
