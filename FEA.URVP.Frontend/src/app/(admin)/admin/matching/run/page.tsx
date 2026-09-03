import type { Metadata } from "next";
import { Suspense } from "react";
import { AdminMatchingRunRoute } from "@/components/routing/QueryRoutes";
import { PageLoader } from "@/components/ui/PageLoader";

export const metadata: Metadata = {
  title: "Matching run | Admin",
  description:
    "Review proposed placements, warnings, and confirm or discard the run.",
};

export default function AdminMatchingRunPage() {
  return (
    <Suspense fallback={<PageLoader />}>
      <AdminMatchingRunRoute />
    </Suspense>
  );
}
