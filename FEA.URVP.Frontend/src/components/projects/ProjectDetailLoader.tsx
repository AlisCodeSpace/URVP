"use client";

import Link from "next/link";
import { Text } from "@/components/ui/Typography";
import { PageHeader } from "@/components/layout/PageHeader";
import { ApplicationWindowClosedNotice } from "@/components/projects/ApplicationWindowClosedNotice";
import { ProjectDetail } from "@/components/projects/ProjectDetail";
import { useStudentProjectsLocked } from "@/hooks/useApplicationWindow";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { ApiError } from "@/lib/api";
import { projectsHref } from "@/lib/auth";
import { getProject, toCatalogProject } from "@/lib/projects-api";
import { NotFoundView } from "@/components/ui/NotFoundView";
import { ProjectDetailSkeleton } from "@/components/ui/SectionSkeletons";

export function ProjectDetailLoader({ id }: { id: string }) {
  const studentProjects = useStudentProjectsLocked();
  const projectQuery = useCancellableQuery(
    async () => {
      try {
        const dto = await getProject(id);
        return { kind: "ok" as const, project: toCatalogProject(dto), message: null };
      } catch (err) {
        if (err instanceof ApiError && err.status === 404) {
          return { kind: "missing" as const, project: null, message: null };
        }
        return {
          kind: "error" as const,
          project: null,
          message:
            err instanceof ApiError ? err.message : "Could not load this project.",
        };
      }
    },
    [id],
    {
      enabled: !studentProjects.pending && !studentProjects.locked,
    },
  );
  const project = projectQuery.data?.project ?? null;
  const notFound = projectQuery.data?.kind === "missing";
  const error = projectQuery.data?.message ?? null;

  if (studentProjects.locked) {
    return (
      <>
        <PageHeader
          title="Projects"
          description="Research listings are hidden while the student application window is closed."
        >
          <Link
            href={projectsHref()}
            className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
          >
            <span aria-hidden>←</span>
            Back to projects
          </Link>
        </PageHeader>
        <section className="site-container py-14 sm:py-16">
          <ApplicationWindowClosedNotice
            semesterName={studentProjects.semesterName}
          />
        </section>
      </>
    );
  }

  if (notFound) {
    return (
      <NotFoundView
        title="Project not found"
        description="This listing is missing or no longer available. Browse open opportunities or return home."
      />
    );
  }

  if (error) {
    return (
      <>
        <PageHeader
          title="Project"
          description="Something went wrong while loading this listing."
        >
          <Link
            href={projectsHref()}
            className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
          >
            <span aria-hidden>←</span>
            Back to projects
          </Link>
        </PageHeader>
        <section className="site-container py-14 sm:py-16">
          <Text as="p" size="3" className="!text-red-800" role="alert">
            {error}
          </Text>
        </section>
      </>
    );
  }

  if (!project) {
    return (
      <>
        <PageHeader
          title="Project"
          description="Loading research opportunity details…"
        >
          <Link
            href={projectsHref()}
            className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
          >
            <span aria-hidden>←</span>
            Back to projects
          </Link>
        </PageHeader>
        <section className="site-container py-14 sm:py-16">
          <ProjectDetailSkeleton />
        </section>
      </>
    );
  }

  return <ProjectDetail project={project} />;
}
