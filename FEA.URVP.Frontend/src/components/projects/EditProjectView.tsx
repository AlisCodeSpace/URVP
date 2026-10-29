"use client";

import Link from "next/link";
import { ErrorAlert } from "@/components/ui/ErrorAlert";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PostProjectForm } from "@/components/projects/PostProjectForm";
import { PageHeader } from "@/components/layout/PageHeader";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { apiErrorMessage } from "@/lib/api";
import { FACULTY_PORTAL_ROLES, myProjectsHref } from "@/lib/auth";
import {
  facultyProjectEditLockMessage,
  isFacultyProjectEditable,
  isProjectOwner,
} from "@/lib/project-form";
import { getProject, toFormValues } from "@/lib/projects-api";
import { AdminFormSkeleton } from "@/components/ui/SectionSkeletons";

export function EditProjectView({
  userId,
  projectId,
}: {
  userId: string;
  projectId: string;
}) {
  const projectQuery = useCancellableQuery(
    async () => {
      try {
        const project = await getProject(projectId);
        if (!isProjectOwner(project, userId)) {
          return { values: null, message: "You can only edit your own projects." };
        }
        if (!isFacultyProjectEditable(project)) {
          return { values: null, message: facultyProjectEditLockMessage(project) };
        }
        return { values: toFormValues(project), message: null };
      } catch (err) {
        return {
          values: null,
          message: apiErrorMessage(err, "Could not load project."),
        };
      }
    },
    [projectId, userId],
  );
  const initialValues = projectQuery.data?.values ?? null;
  const error = projectQuery.data?.message ?? null;

  return (
    <RequireAuth userId={userId} roles={FACULTY_PORTAL_ROLES}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="Edit listing"
          title="Edit project"
          description="Update your research opportunity details and volunteer requirements."
        >
          <Link
            href={myProjectsHref(userId)}
            className="inline-flex items-center gap-2 text-sm text-primary/70 transition hover:text-primary"
          >
            <span aria-hidden>←</span>
            Back to my projects
          </Link>
        </PageHeader>

        <section className="site-container py-14 sm:py-16">
          {error ? (
            <ErrorAlert>{error}</ErrorAlert>
          ) : initialValues == null ? (
            <AdminFormSkeleton fields={8} />
          ) : (
            <PostProjectForm
              userId={userId}
              mode="edit"
              projectId={projectId}
              initialValues={initialValues}
            />
          )}
        </section>
      </main>
    </RequireAuth>
  );
}
