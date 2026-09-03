import type { Metadata } from "next";
import { Suspense } from "react";
import { EditProjectRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Edit Project | URVP",
  description: "Update a research project listing in the URVP portal.",
};

export default function EditProjectPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <EditProjectRoute />
    </Suspense>
  );
}
