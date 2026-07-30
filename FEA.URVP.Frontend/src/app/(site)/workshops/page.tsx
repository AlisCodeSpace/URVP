import type { Metadata } from "next";
import { Button } from "@/components/ui/Button";
import { PageHero } from "@/components/layout/PageHero";
import { WorkshopsList } from "@/components/workshops/WorkshopsList";
import { workshopsIntro } from "@/lib/workshops";

export const metadata: Metadata = {
  title: "Workshops | URVP",
  description:
    "Upcoming URVP workshops for undergraduate research volunteers at the American University of Beirut.",
};

export default function WorkshopsPage() {
  return (
    <main className="flex-1 bg-background">
      <PageHero
        title="Workshops"
        headline="Prepare to match. Thrive once you join a team."
        description={workshopsIntro}
        actions={
          <Button href="#workshops-list" variant="secondary" size="lg">
            View schedule
          </Button>
        }
      />

      <section
        id="workshops-list"
        className="mx-auto max-w-6xl scroll-mt-24 px-6 py-14 sm:py-16"
      >
        <WorkshopsList />
      </section>
    </main>
  );
}
