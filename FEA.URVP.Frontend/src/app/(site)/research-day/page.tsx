import type { Metadata } from "next";
import { Button } from "@/components/ui/Button";
import { PageHero } from "@/components/layout/PageHero";
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
      <PageHero
        title="Research Day"
        headline={researchDayBanner.subtitle}
        description="Date to be announced · American University of Beirut. Application forms and deadlines are listed below."
        actions={
          <>
            <Button href="#research-day-forms" variant="secondary" size="lg">
              Application &amp; forms
            </Button>
            <Button
              href="#research-day-deadlines"
              variant="outline-light"
              size="lg"
            >
              View deadlines
            </Button>
          </>
        }
      />
      <ResearchDayContent />
    </main>
  );
}
