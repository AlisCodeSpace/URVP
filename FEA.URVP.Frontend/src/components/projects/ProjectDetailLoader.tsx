"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Text } from "@radix-ui/themes";
import { PageHeader } from "@/components/layout/PageHeader";
import { ProjectDetail } from "@/components/projects/ProjectDetail";
import { ApiError } from "@/lib/api";
import type { CatalogProject } from "@/lib/projects";
import { projectsHref } from "@/lib/auth";
import { getProject, toCatalogProject } from "@/lib/projects-api";

export function ProjectDetailLoader({ id }: { id: string }) {
  const [project, setProject] = useState<CatalogProject | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [notFound, setNotFound] = useState(false);

  useEffect(() => {
    let cancelled = false;

    void (async () => {
      try {
        const dto = await getProject(id);
        if (!cancelled) setProject(toCatalogProject(dto));
      } catch (err) {
        if (cancelled) return;
        if (err instanceof ApiError && err.status === 404) {
          setNotFound(true);
        } else {
          setError(
            err instanceof ApiError
              ? err.message
              : "Could not load this project.",
          );
        }
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [id]);

  if (notFound) {
    return (
      <>
        <PageHeader
          title="Project"
          description="This listing could not be found."
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
          <Text as="p" size="3" className="!text-muted">
            Project not found.
          </Text>
        </section>
      </>
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
          <Text as="p" size="3" className="!text-muted">
            Loading project…
          </Text>
        </section>
      </>
    );
  }

  return <ProjectDetail project={project} />;
}
