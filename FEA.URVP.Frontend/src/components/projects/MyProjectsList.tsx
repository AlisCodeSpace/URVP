"use client";

import { useCallback, useEffect, useState } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { ApiError } from "@/lib/api";
import { editProjectHref, newProjectHref, viewProjectHref } from "@/lib/auth";
import type { MyProject, MyProjectStatus } from "@/lib/project-form";
import {
  deleteProject,
  listMyProjects,
  toMyProject,
} from "@/lib/projects-api";

const statusClass: Record<MyProjectStatus, string> = {
  Open: "text-secondary-deep",
  Matching: "text-primary",
  Closed: "text-muted",
};

function ProjectRow({
  userId,
  project,
  busyId,
  onDelete,
}: {
  userId: string;
  project: MyProject;
  busyId: string | null;
  onDelete: (id: string) => void;
}) {
  const deleting = busyId === project.id;

  return (
    <li className="grid gap-4 border-b border-primary/10 py-7 last:border-b-0 sm:grid-cols-[1fr_auto] sm:items-end">
      <div>
        <div className="flex flex-wrap items-center gap-x-3 gap-y-1">
          <Text
            as="p"
            size="1"
            weight="bold"
            className={`!uppercase !tracking-[0.18em] ${statusClass[project.status]}`}
          >
            {project.status}
          </Text>
          <Text as="p" size="1" className="!text-muted">
            Updated {project.updatedAt}
          </Text>
        </div>
        <Heading
          as="h2"
          size="5"
          weight="medium"
          mt="2"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          {project.title}
        </Heading>
        <Text as="p" size="2" mt="2" className="!text-muted">
          {project.researchArea}
          <span className="mx-2 text-primary/25" aria-hidden>
            ·
          </span>
          {project.activityType}
          <span className="mx-2 text-primary/25" aria-hidden>
            ·
          </span>
          {project.volunteersRequired} volunteer
          {project.volunteersRequired === 1 ? "" : "s"}
        </Text>
      </div>
      <div className="flex flex-wrap gap-2">
        <Button
          href={viewProjectHref(userId, project.id)}
          variant="primary"
          size="sm"
        >
          View
        </Button>
        <Button
          href={editProjectHref(userId, project.id)}
          variant="ghost"
          size="sm"
        >
          Edit
        </Button>
        <Button
          type="button"
          variant="ghost"
          size="sm"
          disabled={deleting || busyId !== null}
          onClick={() => onDelete(project.id)}
          className="!text-red-800 hover:!text-red-900"
        >
          {deleting ? "Deleting…" : "Delete"}
        </Button>
      </div>
    </li>
  );
}

export function MyProjectsList({ userId }: { userId: string }) {
  const [projects, setProjects] = useState<MyProject[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);

  const load = useCallback(async () => {
    setError(null);
    try {
      const items = await listMyProjects();
      setProjects(items.map(toMyProject));
    } catch (err) {
      setProjects([]);
      setError(
        err instanceof ApiError
          ? err.message
          : "Could not load your projects.",
      );
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  async function handleDelete(id: string) {
    if (!window.confirm("Delete this project? This cannot be undone.")) return;

    setBusyId(id);
    setError(null);
    try {
      await deleteProject(id);
      setProjects((prev) => (prev ?? []).filter((p) => p.id !== id));
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Could not delete project.",
      );
    } finally {
      setBusyId(null);
    }
  }

  if (projects === null) {
    return (
      <Text as="p" size="3" className="!text-muted">
        Loading your projects…
      </Text>
    );
  }

  if (error && projects.length === 0) {
    return (
      <div className="rounded-lg border border-dashed border-red-200 px-6 py-10 text-center">
        <Text as="p" size="3" className="!text-red-800">
          {error}
        </Text>
        <div className="mt-4 flex justify-center">
          <Button type="button" variant="outline" size="md" onClick={() => void load()}>
            Try again
          </Button>
        </div>
      </div>
    );
  }

  if (projects.length === 0) {
    return (
      <div className="rounded-lg border border-dashed border-primary/20 px-6 py-16 text-center">
        <Heading
          as="h2"
          size="5"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          No projects yet
        </Heading>
        <Text as="p" size="3" mt="2" className="mx-auto max-w-md !text-muted">
          Post your first research opportunity so undergraduates can apply and
          match with your work.
        </Text>
        <div className="mt-6 flex justify-center">
          <Button href={newProjectHref(userId)} variant="secondary" size="md">
            New project
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div>
      {error ? (
        <Text
          as="p"
          size="2"
          mb="4"
          role="alert"
          className="rounded-md bg-red-50 px-3 py-2 !text-red-800"
        >
          {error}
        </Text>
      ) : null}
      <ul className="border-y border-primary/10">
        {projects.map((project) => (
          <ProjectRow
            key={project.id}
            userId={userId}
            project={project}
            busyId={busyId}
            onDelete={handleDelete}
          />
        ))}
      </ul>
    </div>
  );
}
