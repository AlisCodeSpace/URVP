import type { Metadata } from "next";
import { Suspense } from "react";
import { ProjectDetailRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Project | URVP",
  description: "Research opportunity details for undergraduate volunteers.",
};

export default function ProjectDetailPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <ProjectDetailRoute />
    </Suspense>
  );
}
