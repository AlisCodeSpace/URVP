"use client";

import { useCallback, useEffect, useState } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { IconPencil, IconPlus, IconTrash } from "@/components/ui/Icons";
import { ApiError } from "@/lib/api";
import { editProjectHref, newProjectHref, viewProjectHref } from "@/lib/auth";
import type { MyProject, MyProjectStatus } from "@/lib/project-form";
import {
  deleteProject,
  listMyProjects,
  toMyProject,
} from "@/lib/projects-api";

function statusClass(status: MyProjectStatus) {
  if (status === "Open") return "is-active";
  if (status === "Matching") return "is-matching";
  return "";
}

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
  const areas = project.researchAreas.slice(0, 2).join(" · ");
  const extraAreas = project.researchAreas.length - 2;

  return (
    <tr>
      <td>
        <div className="admin-users-name">{project.title}</div>
        {areas ? (
          <div className="admin-users-meta">
            {areas}
            {extraAreas > 0 ? ` · +${extraAreas}` : ""}
          </div>
        ) : null}
      </td>
      <td>
        <span className={`admin-value-status ${statusClass(project.status)}`}>
          {project.status}
        </span>
      </td>
      <td>
        {project.volunteersFilled}/{project.volunteersRequired}
      </td>
      <td>{project.updatedAt}</td>
      <td>
        <div className="admin-value-actions">
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
            <IconPencil />
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
            <IconTrash />
            {deleting ? "Deleting…" : "Delete"}
          </Button>
        </div>
      </td>
    </tr>
  );
}

export function MyProjectsList({ userId }: { userId: string }) {
  const [projects, setProjects] = useState<MyProject[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [pendingDelete, setPendingDelete] = useState<MyProject | null>(null);

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

  async function handleConfirmDelete() {
    if (!pendingDelete) return;

    const id = pendingDelete.id;
    setBusyId(id);
    setError(null);
    try {
      await deleteProject(id);
      setPendingDelete(null);
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
            <IconPlus />
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

      <div className="admin-users-table-wrap">
        <table className="admin-users-table">
          <thead>
            <tr>
              <th>Project</th>
              <th>Status</th>
              <th>Seats</th>
              <th>Updated</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {projects.map((project) => (
              <ProjectRow
                key={project.id}
                userId={userId}
                project={project}
                busyId={busyId}
                onDelete={(id) => {
                  const next = projects.find((p) => p.id === id) ?? null;
                  setPendingDelete(next);
                }}
              />
            ))}
          </tbody>
        </table>
      </div>

      <ConfirmModal
        open={pendingDelete !== null}
        onClose={() => {
          if (busyId === null) setPendingDelete(null);
        }}
        onConfirm={handleConfirmDelete}
        title="Delete project?"
        description={
          pendingDelete
            ? `Delete “${pendingDelete.title}”? This cannot be undone.`
            : "Delete this project? This cannot be undone."
        }
        confirmLabel="Delete"
        busyLabel="Deleting…"
        busy={pendingDelete !== null && busyId === pendingDelete.id}
      />
    </div>
  );
}
