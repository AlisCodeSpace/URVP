import { Button } from "@/components/ui/Button";
import { PageHero } from "@/components/layout/PageHero";

export function Hero() {
  return (
    <PageHero
      title="Undergraduate Research Volunteer Program"
      titleScale="brand"
      headline="Match with faculty research. Shape your academic path."
      description="AY 2025–26 · American University of Beirut. Faculty post projects; students find their fit."
      actions={
        <>
          <Button href="/sign-in" variant="secondary" size="lg">
            Apply
          </Button>
          <Button href="/projects" variant="outline-light" size="lg">
            Browse projects
          </Button>
        </>
      }
    />
  );
}
