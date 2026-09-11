"use client";

import { PageHeader } from "@/components/layout/PageHeader";
import { ProjectsBrowse } from "@/components/projects/ProjectsBrowse";
import { useStudentProjectsLocked } from "@/hooks/useApplicationWindow";
import { projectsIntro } from "@/lib/projects";

export function ProjectsCatalogView() {
  const { locked } = useStudentProjectsLocked();

  return (
    <>
      <PageHeader
        title="Projects"
        description={
          locked
            ? "The student application window is currently closed. Project listings will appear here when it opens."
            : projectsIntro
        }
      />

      <section
        id="projects-catalog"
        className="site-container scroll-mt-24 py-10 sm:py-14"
      >
        <ProjectsBrowse variant="catalog" />
      </section>
    </>
  );
}
