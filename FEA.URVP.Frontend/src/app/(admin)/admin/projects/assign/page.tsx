import type { Metadata } from "next";
import { Suspense } from "react";
import { AdminAssignStudentsRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Assign students | Admin",
  description: "Find and assign students to a project.",
};

export default function AdminAssignStudentsPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <AdminAssignStudentsRoute />
    </Suspense>
  );
}
