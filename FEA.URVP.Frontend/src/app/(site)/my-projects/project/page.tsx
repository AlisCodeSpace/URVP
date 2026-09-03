import type { Metadata } from "next";
import { Suspense } from "react";
import { FacultyProjectRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "View Project | URVP",
  description: "Review a research project you posted in the URVP portal.",
};

export default function ViewProjectPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <FacultyProjectRoute />
    </Suspense>
  );
}
