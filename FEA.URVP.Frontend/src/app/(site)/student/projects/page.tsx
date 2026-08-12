import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PageHeader } from "@/components/layout/PageHeader";
import { ProjectsBrowse } from "@/components/projects/ProjectsBrowse";
import { StudentPortalNav } from "@/components/student/StudentPortalNav";
import { STUDENT_ROLES } from "@/lib/auth";
import { projectsIntro } from "@/lib/projects";

export const metadata: Metadata = {
  title: "Projects | URVP Student Portal",
  description:
    "Browse faculty research projects and find volunteer opportunities in the Undergraduate Research Volunteer Program at AUB.",
};

export default function StudentProjectsPage() {
  return (
    <RequireAuth roles={STUDENT_ROLES}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="Student portal"
          title="Projects"
          description={projectsIntro}
        />

        <section
          id="projects-catalog"
          className="site-container scroll-mt-24 py-10 sm:py-14"
        >
          <StudentPortalNav />
          <div className="mt-8">
            <ProjectsBrowse />
          </div>
        </section>
      </main>
    </RequireAuth>
  );
}
