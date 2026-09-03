import type { Metadata } from "next";
import { Suspense } from "react";
import { AdminWorkshopEditRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Edit workshop | Admin",
  description: "Update a workshop session.",
};

export default function AdminEditWorkshopPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <AdminWorkshopEditRoute />
    </Suspense>
  );
}
