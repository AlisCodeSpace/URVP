import type { Metadata } from "next";
import { Suspense } from "react";
import { AdminSemesterEditRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Edit semester | Admin",
  description: "Update a semester's details and application window.",
};

export default function AdminEditSemesterPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <AdminSemesterEditRoute />
    </Suspense>
  );
}
