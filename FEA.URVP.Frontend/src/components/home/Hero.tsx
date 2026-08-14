import Link from "next/link";
import { Button } from "@/components/ui/Button";
import { PageHero } from "@/components/layout/PageHero";

export function Hero() {
  return (
    <PageHero
      title="Undergraduate Research Volunteer Program"
      titleScale="brand"
      headline="Match with faculty research. Shape your academic path."
      announcement={
        <>
          Applications for the URVP 2026–27 cycle are open.{" "}
          <Link
            href="/sign-in"
            className="underline decoration-secondary/70 underline-offset-4 transition hover:text-white"
          >
            Apply now!
          </Link>
        </>
      }
      actions={
        <>
          <Button href="/sign-in" variant="secondary" size="lg">
            Log In
          </Button>
          <Button href="/projects" variant="outline-light" size="lg">
            Browse Projects
          </Button>
          <Button href="/my-projects" variant="outline-light" size="lg">
            Faculty Portal
          </Button>
        </>
      }
    />
  );
}
