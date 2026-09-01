import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PageHeader } from "@/components/layout/PageHeader";
import { ProjectsBrowse } from "@/components/projects/ProjectsBrowse";
import { StudentPortalNav } from "@/components/student/StudentPortalNav";
import { STUDENT_ROLES } from "@/lib/auth";

export const metadata: Metadata = {
  title: "Ranked Projects | URVP Student Portal",
  description:
    "Your top project choices, ordered from 1st to 3rd. The program team uses these rankings for matching.",
};

export default function StudentRankedProjectsPage() {
  return (
    <RequireAuth roles={STUDENT_ROLES}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="Student portal"
          title="Ranked Projects"
          description="Your top project choices, ordered from 1st to 3rd. The program team uses these rankings for matching."
        />

        <section
          id="ranked-projects"
          className="site-container scroll-mt-24 py-10 sm:py-14"
        >
          <StudentPortalNav />
          <div className="mt-8">
            <ProjectsBrowse variant="ranked" />
          </div>
        </section>
      </main>
    </RequireAuth>
  );
}
