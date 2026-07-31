"use client";

import { useEffect, useState } from "react";
import { Text } from "@radix-ui/themes";
import { ProjectDetail } from "@/components/projects/ProjectDetail";
import { ApiError } from "@/lib/api";
import type { CatalogProject } from "@/lib/projects";
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
      <Text as="p" size="3" className="!text-muted">
        Project not found.
      </Text>
    );
  }

  if (error) {
    return (
      <Text as="p" size="3" className="!text-red-800" role="alert">
        {error}
      </Text>
    );
  }

  if (!project) {
    return (
      <Text as="p" size="3" className="!text-muted">
        Loading project…
      </Text>
    );
  }

  return <ProjectDetail project={project} />;
}
