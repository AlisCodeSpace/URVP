import type { Metadata } from "next";
import { Suspense } from "react";
import { MyProjectsRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "My Projects | URVP",
  description:
    "Manage research projects you have posted for the Undergraduate Research Volunteer Program.",
};

export default function MyProjectsPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <MyProjectsRoute />
    </Suspense>
  );
}
