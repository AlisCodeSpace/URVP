import type { Metadata } from "next";
import { PageHeader } from "@/components/layout/PageHeader";
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
      <PageHeader
        title="Workshops"
        description={workshopsIntro}
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
