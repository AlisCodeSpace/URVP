import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PageHeader } from "@/components/layout/PageHeader";
import { StudentPortalNav } from "@/components/student/StudentPortalNav";
import { StudentRankingsList } from "@/components/student/StudentRankingsList";
import { STUDENT_ROLES } from "@/lib/auth";

export const metadata: Metadata = {
  title: "My Rankings | URVP",
  description:
    "View and manage your ranked project preferences for the Undergraduate Research Volunteer Program.",
};

export default function StudentRankingsPage() {
  return (
    <RequireAuth roles={STUDENT_ROLES}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="Student portal"
          title="My rankings"
          description="Your top project choices, ordered from 1st to 3rd. The program team uses these rankings for matching."
        />

        <section className="mx-auto max-w-6xl px-6 py-10 sm:py-14">
          <StudentPortalNav />
          <div className="mt-8">
            <StudentRankingsList />
          </div>
        </section>
      </main>
    </RequireAuth>
  );
}
