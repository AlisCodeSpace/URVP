"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Text } from "@radix-ui/themes";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PostProjectForm } from "@/components/projects/PostProjectForm";
import { PageHeader } from "@/components/layout/PageHeader";
import { ApiError } from "@/lib/api";
import { FACULTY_PORTAL_ROLES, myProjectsHref } from "@/lib/auth";
import type { ProjectFormValues } from "@/lib/project-form";
import { getProject, toFormValues } from "@/lib/projects-api";

export function EditProjectView({
  userId,
  projectId,
}: {
  userId: string;
  projectId: string;
}) {
  const [initialValues, setInitialValues] = useState<ProjectFormValues | null>(
    null,
  );
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    void (async () => {
      try {
        const project = await getProject(projectId);
        if (cancelled) return;
        if (project.createdByUserId.toLowerCase() !== userId.toLowerCase()) {
          setError("You can only edit your own projects.");
          return;
        }
        setInitialValues(toFormValues(project));
      } catch (err) {
        if (cancelled) return;
        setError(
          err instanceof ApiError ? err.message : "Could not load project.",
        );
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [projectId, userId]);

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
            className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
          >
            <span aria-hidden>←</span>
            Back to my projects
          </Link>
        </PageHeader>

        <section className="site-container py-14 sm:py-16">
          {error ? (
            <Text
              as="p"
              size="3"
              role="alert"
              className="rounded-md bg-red-50 px-3 py-2 !text-red-800"
            >
              {error}
            </Text>
          ) : initialValues == null ? (
            <Text as="p" size="3" className="!text-muted">
              Loading project…
            </Text>
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
