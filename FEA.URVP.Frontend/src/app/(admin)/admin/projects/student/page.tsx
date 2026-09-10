import type { Metadata } from "next";
import { Suspense } from "react";
import { AdminStudentProfileRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Student profile | Admin",
  description: "Review a student's URVP profile from a project listing.",
};

export default function AdminStudentProfilePage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <AdminStudentProfileRoute />
    </Suspense>
  );
}
