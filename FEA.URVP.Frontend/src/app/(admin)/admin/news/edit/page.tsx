import type { Metadata } from "next";
import { Suspense } from "react";
import { AdminNewsEditRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Edit news | Admin",
  description: "Update a news story.",
};

export default function AdminEditNewsPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <AdminNewsEditRoute />
    </Suspense>
  );
}
