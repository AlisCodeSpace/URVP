import type { Metadata } from "next";
import { AdminWorkshopForm } from "@/components/admin/AdminWorkshopForm";

export const metadata: Metadata = {
  title: "Edit workshop | Admin",
  description: "Update a workshop session.",
};

type AdminEditWorkshopPageProps = {
  params: Promise<{ id: string }>;
};

export default async function AdminEditWorkshopPage({
  params,
}: AdminEditWorkshopPageProps) {
  const { id } = await params;
  return <AdminWorkshopForm workshopId={id} />;
}
