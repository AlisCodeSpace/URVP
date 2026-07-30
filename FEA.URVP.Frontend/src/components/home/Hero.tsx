import { Button } from "@/components/ui/Button";
import { PageHero } from "@/components/layout/PageHero";

export function Hero() {
  return (
    <PageHero
      title="URVP"
      titleScale="brand"
      headline="Match with faculty research. Shape your academic path."
      description="AUB’s Undergraduate Research Volunteer Program — AY 2025–26. Faculty post projects; students find their fit."
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
