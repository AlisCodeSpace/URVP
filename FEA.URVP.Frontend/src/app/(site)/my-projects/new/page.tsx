import type { Metadata } from "next";
import { Suspense } from "react";
import { NewProjectRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Post a Project | URVP",
  description:
    "Post a new research project for undergraduate volunteers in the URVP portal.",
};

export default function NewProjectPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <NewProjectRoute />
    </Suspense>
  );
}
