import type { Metadata } from "next";
import { AdminSemestersView } from "@/components/admin/AdminSemestersView";

export const metadata: Metadata = {
  title: "URVP Cycles | Admin",
  description: "Manage URVP cycles and application windows.",
};

export default function AdminSemestersPage() {
  return <AdminSemestersView />;
}
