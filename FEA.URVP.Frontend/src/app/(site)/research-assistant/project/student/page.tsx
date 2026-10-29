import type { Metadata } from "next";
import { Suspense } from "react";
import { AssistedStudentRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Student Profile | URVP",
  description: "Review a student who applied to a research project you assist.",
};

export default function AssistedStudentPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <AssistedStudentRoute />
    </Suspense>
  );
}
