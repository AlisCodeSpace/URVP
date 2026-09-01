import type { Metadata } from "next";
import { AdminWorkshopForm } from "@/components/admin/AdminWorkshopForm";

export const metadata: Metadata = {
  title: "New workshop | Admin",
  description: "Publish a workshop session.",
};

export default function AdminNewWorkshopPage() {
  return <AdminWorkshopForm />;
}
