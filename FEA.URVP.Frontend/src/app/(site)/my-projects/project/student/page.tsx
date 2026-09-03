import type { Metadata } from "next";
import { Suspense } from "react";
import { FacultyStudentProfileRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Student Profile | URVP",
  description:
    "Review a student who ranked a research project you posted in the URVP portal.",
};

export default function FacultyStudentProfilePage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <FacultyStudentProfileRoute />
    </Suspense>
  );
}
