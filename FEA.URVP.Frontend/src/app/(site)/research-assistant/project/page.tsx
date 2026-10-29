import type { Metadata } from "next";
import { Suspense } from "react";
import { AssistedProjectRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "View Project | URVP",
  description: "Review a research project you assist and rank the students who applied.",
};

export default function AssistedProjectPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <AssistedProjectRoute />
    </Suspense>
  );
}
