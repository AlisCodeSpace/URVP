import type { Metadata } from "next";
import { Suspense } from "react";
import { AdminSemesterEditRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Edit URVP cycle | Admin",
  description: "Update a URVP cycle's details and application window.",
};

export default function AdminEditSemesterPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <AdminSemesterEditRoute />
    </Suspense>
  );
}
