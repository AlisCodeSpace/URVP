import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PageHeader } from "@/components/layout/PageHeader";
import { StudentPortalNav } from "@/components/student/StudentPortalNav";
import { StudentProfileForm } from "@/components/student/StudentProfileForm";
import { STUDENT_ROLES } from "@/lib/auth";

export const metadata: Metadata = {
  title: "My Profile | URVP",
  description:
    "Create or update your Undergraduate Research Volunteer Program student profile.",
};

export default function StudentProfilePage() {
  return (
    <RequireAuth roles={STUDENT_ROLES}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="Student portal"
          title="My profile"
          description="Complete your profile, research interests, and weekly availability so faculty can match you with open projects."
        />

        <section className="site-container py-10 sm:py-14">
          <StudentPortalNav />
          <div className="mt-8">
            <StudentProfileForm />
          </div>
        </section>
      </main>
    </RequireAuth>
  );
}
