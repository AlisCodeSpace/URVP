import type { Metadata } from "next";
import { PageHeader } from "@/components/layout/PageHeader";
import { ResearchDayContent } from "@/components/research-day/ResearchDayView";
import { researchDayBanner } from "@/lib/research-day";

export const metadata: Metadata = {
  title: "Research Day | URVP",
  description:
    "URVP Research Day — deadlines, updates, and Google Form links for applications and registration at AUB.",
};

export default function ResearchDayPage() {
  return (
    <main className="flex-1 bg-background">
      <PageHeader
        title="Research Day"
        description={`${researchDayBanner.subtitle} Date TBA · American University of Beirut.`}
      />
      <ResearchDayContent />
    </main>
  );
}
