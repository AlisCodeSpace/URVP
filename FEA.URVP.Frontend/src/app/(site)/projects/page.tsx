import type { Metadata } from "next";
import { Heading, Text } from "@radix-ui/themes";
import { ProjectsBrowse } from "@/components/projects/ProjectsBrowse";
import { Button } from "@/components/ui/Button";
import { projectsIntro } from "@/lib/projects";

export const metadata: Metadata = {
  title: "Projects | URVP",
  description:
    "Browse faculty research projects and find volunteer opportunities in the Undergraduate Research Volunteer Program at AUB.",
};

export default function ProjectsPage() {
  return (
    <main className="flex-1 bg-background">
      <section className="border-b border-primary/10 bg-primary-deep text-white">
        <div className="mx-auto flex max-w-6xl flex-col gap-6 px-6 py-16 sm:flex-row sm:items-end sm:justify-between sm:py-20">
          <div>
            <Text
              as="p"
              size="2"
              weight="medium"
              className="!uppercase !tracking-[0.2em] !text-secondary"
            >
              Student portal
            </Text>
            <Heading
              as="h1"
              size="8"
              weight="medium"
              mt="3"
              className="!font-[family-name:var(--font-display)] !text-white"
            >
              Projects
            </Heading>
            <Text
              as="p"
              size="4"
              mt="3"
              className="max-w-xl !leading-relaxed !text-white/75"
            >
              {projectsIntro}
            </Text>
          </div>
          <Button href="#projects-catalog" variant="secondary" size="lg">
            Browse catalog
          </Button>
        </div>
      </section>

      <section
        id="projects-catalog"
        className="mx-auto max-w-6xl scroll-mt-24 px-6 py-14 sm:py-16"
      >
        <ProjectsBrowse />
      </section>
    </main>
  );
}
