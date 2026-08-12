"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Text } from "@radix-ui/themes";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { FacultyProjectReadonly } from "@/components/projects/FacultyProjectReadonly";
import { PageHeader } from "@/components/layout/PageHeader";
import { ApiError } from "@/lib/api";
import { FACULTY_PORTAL_ROLES, myProjectsHref } from "@/lib/auth";
import { getProject, type ProjectDto } from "@/lib/projects-api";

export function FacultyProjectView({
  userId,
  projectId,
}: {
  userId: string;
  projectId: string;
}) {
  const [project, setProject] = useState<ProjectDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    void (async () => {
      try {
        const next = await getProject(projectId);
        if (cancelled) return;
        if (next.createdByUserId.toLowerCase() !== userId.toLowerCase()) {
          setError("You can only view your own projects here.");
          return;
        }
        setProject(next);
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
          eyebrow="Faculty portal"
          title="View project"
          description="Review the details of your posted research opportunity."
          maxWidth="3xl"
        >
          <Link
            href={myProjectsHref(userId)}
            className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
          >
            <span aria-hidden>←</span>
            Back to my projects
          </Link>
        </PageHeader>

        <section className="site-container site-container--narrow py-12 sm:py-16">
          {error ? (
            <Text
              as="p"
              size="3"
              role="alert"
              className="rounded-md bg-red-50 px-3 py-2 !text-red-800"
            >
              {error}
            </Text>
          ) : project == null ? (
            <Text as="p" size="3" className="!text-muted">
              Loading project…
            </Text>
          ) : (
            <FacultyProjectReadonly userId={userId} project={project} />
          )}
        </section>
      </main>
    </RequireAuth>
  );
}
