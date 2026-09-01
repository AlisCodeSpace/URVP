import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PageHeader } from "@/components/layout/PageHeader";
import { ProjectsBrowse } from "@/components/projects/ProjectsBrowse";
import { projectsIntro } from "@/lib/projects";

export const metadata: Metadata = {
  title: "Projects | URVP",
  description:
    "Browse faculty research projects and find volunteer opportunities in the Undergraduate Research Volunteer Program at AUB.",
};

export default function ProjectsPage() {
  return (
    <RequireAuth>
      <main className="flex-1 bg-background">
        <PageHeader title="Projects" description={projectsIntro} />

        <section
          id="projects-catalog"
          className="site-container scroll-mt-24 py-10 sm:py-14"
        >
          <ProjectsBrowse variant="catalog" />
        </section>
      </main>
    </RequireAuth>
  );
}
