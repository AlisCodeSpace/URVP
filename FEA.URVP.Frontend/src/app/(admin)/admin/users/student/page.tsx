import type { Metadata } from "next";
import { Suspense } from "react";
import { AdminUserStudentProfileRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Student profile | Admin",
  description:
    "Review a student's URVP profile, assigned projects, and ranked projects.",
};

export default function AdminUserStudentProfilePage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <AdminUserStudentProfileRoute />
    </Suspense>
  );
}
