import type { Metadata } from "next";
import { Suspense } from "react";
import { AdminProjectRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Project | Admin",
  description: "Project details and students who ranked this listing.",
};

export default function AdminProjectPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <AdminProjectRoute />
    </Suspense>
  );
}
