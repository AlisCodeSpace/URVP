import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PageHeader } from "@/components/layout/PageHeader";
import { MatchedProjectsList } from "@/components/student/MatchedProjectsList";
import { StudentPortalNav } from "@/components/student/StudentPortalNav";
import { STUDENT_ROLES } from "@/lib/auth";

export const metadata: Metadata = {
  title: "Matched Projects | URVP Student Portal",
  description:
    "Projects you are participating in, including assignments to projects you did not rank.",
};

export default function StudentMatchedProjectsPage() {
  return (
    <RequireAuth roles={STUDENT_ROLES}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="Student portal"
          title="Matched Projects"
          description="Projects you are participating in. Assignments appear here after matching, including projects you did not rank."
        />

        <section
          id="matched-projects"
          className="site-container scroll-mt-24 py-10 sm:py-14"
          data-tour="student-matches"
        >
          <StudentPortalNav />
          <div className="mt-8">
            <MatchedProjectsList />
          </div>
        </section>
      </main>
    </RequireAuth>
  );
}
