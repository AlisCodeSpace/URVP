import type { Metadata } from "next";
import { AdminWorkshopsView } from "@/components/admin/AdminWorkshopsView";

export const metadata: Metadata = {
  title: "Workshops | Admin",
  description: "Publish workshop sessions and card photos.",
};

export default function AdminWorkshopsPage() {
  return <AdminWorkshopsView />;
}
