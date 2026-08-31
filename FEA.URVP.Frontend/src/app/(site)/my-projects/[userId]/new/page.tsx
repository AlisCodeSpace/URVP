import type { Metadata } from "next";
import Link from "next/link";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PostProjectForm } from "@/components/projects/PostProjectForm";
import { PageHeader } from "@/components/layout/PageHeader";
import { FACULTY_PORTAL_ROLES, myProjectsHref } from "@/lib/auth";

export const metadata: Metadata = {
  title: "Post a Project | URVP",
  description:
    "Post a new research project for undergraduate volunteers in the URVP portal.",
};

type NewProjectPageProps = {
  params: Promise<{ userId: string }>;
};

export default async function NewProjectPage({ params }: NewProjectPageProps) {
  const { userId } = await params;

  return (
    <RequireAuth userId={userId} roles={FACULTY_PORTAL_ROLES}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="New listing"
          title="Post a project"
          description="Share a research opportunity across AUB faculties, centers, and institutes for undergraduate matching."
        >
          <Link
            href={myProjectsHref(userId)}
            className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
          >
            <span aria-hidden>←</span>
            Back to my projects
          </Link>
        </PageHeader>

        <section className="site-container py-14 sm:py-16">
          <PostProjectForm userId={userId} />
        </section>
      </main>
    </RequireAuth>
  );
}
