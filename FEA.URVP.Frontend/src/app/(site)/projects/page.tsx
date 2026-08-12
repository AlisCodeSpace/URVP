import type { Metadata } from "next";
import { ProjectsBrowse } from "@/components/projects/ProjectsBrowse";
import { PageHeader } from "@/components/layout/PageHeader";
import { projectsIntro } from "@/lib/projects";

export const metadata: Metadata = {
  title: "Projects | URVP",
  description:
    "Browse faculty research projects and find volunteer opportunities in the Undergraduate Research Volunteer Program at AUB.",
};

export default function ProjectsPage() {
  return (
    <main className="flex-1 bg-background">
      <PageHeader
        eyebrow="Student portal"
        title="Projects"
        description={projectsIntro}
      />

      <section
        id="projects-catalog"
        className="site-container scroll-mt-24 py-14 sm:py-16"
      >
        <ProjectsBrowse />
      </section>
    </main>
  );
}
