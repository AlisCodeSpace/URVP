"use client";

import { useState } from "react";
import { Heading, Text } from "@/components/ui/Typography";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { DeleteIconButton } from "@/components/ui/DeleteIconButton";
import { IconPencil, IconPlus } from "@/components/ui/Icons";
import { RefreshIconButton } from "@/components/ui/RefreshIconButton";
import { ApiError } from "@/lib/api";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { useApplicationWindow } from "@/hooks/useApplicationWindow";
import { editProjectHref, newProjectHref, viewProjectHref } from "@/lib/auth";
import {
  isFacultyProjectEditable,
  type MyProject,
  type MyProjectStatus,
} from "@/lib/project-form";
import {
  deleteProject,
  listMyProjects,
  reactivateProject,
  toMyProject,
} from "@/lib/projects-api";
import { AdminTableSkeleton } from "@/components/ui/SectionSkeletons";

function statusClass(status: MyProjectStatus) {
  if (status === "Open") return "is-active";
  if (status === "Matching") return "is-matching";
  return "";
}

function ProjectRow({
  userId,
  project,
  busyId,
  canReactivate,
  onDelete,
  onReactivate,
}: {
  userId: string;
  project: MyProject;
  busyId: string | null;
  canReactivate: boolean;
  onDelete: (id: string) => void;
  onReactivate: (id: string) => void;
}) {
  const deleting = busyId === project.id;
  const inactive = project.status === "Inactive";
  const locked = !isFacultyProjectEditable(project);
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
      <td>{project.semesterName || "—"}</td>
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
          {inactive && canReactivate ? (
            <Button
              type="button"
              variant="secondary"
              size="sm"
              disabled={busyId !== null}
              onClick={() => onReactivate(project.id)}
            >
              Reactivate
            </Button>
          ) : null}
          {inactive || !locked ? (
            <DeleteIconButton
              variant="ghost"
              disabled={deleting || busyId !== null}
              loading={deleting}
              onClick={() => onDelete(project.id)}
              className="!text-red-800 hover:!text-red-900"
            />
          ) : null}
          {!inactive && !locked ? (
            <Button
              href={editProjectHref(userId, project.id)}
              variant="ghost"
              size="sm"
            >
              <IconPencil />
              Edit
            </Button>
          ) : null}
        </div>
      </td>
    </tr>
  );
}

export function MyProjectsList({ userId }: { userId: string }) {
  const [busyId, setBusyId] = useState<string | null>(null);
  const [pendingDelete, setPendingDelete] = useState<MyProject | null>(null);
  const [pendingReactivate, setPendingReactivate] = useState<MyProject | null>(null);
  const {
    data,
    loading,
    error,
    setData,
    setError,
    reload: load,
  } = useCancellableQuery(() => listMyProjects().then((items) => items.map(toMyProject)), [], {
    fallbackError: "Could not load your projects.",
    dataOnError: () => [],
  });
  const projects = data;
  const phase = useApplicationWindow();
  const canPost = !phase.loading && phase.registrationOpen;

  async function handleConfirmDelete() {
    if (!pendingDelete) return;

    const id = pendingDelete.id;
    setBusyId(id);
    setError(null);
    try {
      await deleteProject(id);
      setPendingDelete(null);
      setData((prev) => (prev ?? []).filter((project) => project.id !== id));
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Could not delete project.",
      );
    } finally {
      setBusyId(null);
    }
  }

  async function handleConfirmReactivate() {
    if (!pendingReactivate) return;

    const id = pendingReactivate.id;
    setBusyId(id);
    setError(null);
    try {
      const next = toMyProject(await reactivateProject(id));
      setPendingReactivate(null);
      setData((prev) =>
        (prev ?? []).map((project) => (project.id === id ? next : project)),
      );
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Could not reactivate project.",
      );
    } finally {
      setBusyId(null);
    }
  }

  if (projects === null) {
    return <AdminTableSkeleton columns={6} rows={4} />;
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
          {canPost ? (
            <Button
              href={newProjectHref(userId)}
              variant="secondary"
              size="md"
              data-tour="faculty-new-project"
            >
              <IconPlus />
              New project
            </Button>
          ) : (
            <Button
              type="button"
              variant="secondary"
              size="md"
              disabled
              title="Projects can be posted only while the registration window is open."
            >
              <IconPlus />
              New project
            </Button>
          )}
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

      <div className="mb-4 flex justify-end">
        <RefreshIconButton loading={loading} onClick={() => void load()} />
      </div>

      <div className="admin-users-table-wrap">
        <table className="admin-users-table">
          <thead>
            <tr>
              <th>Project</th>
              <th>Status</th>
              <th>Cycle</th>
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
                canReactivate={canPost}
                onReactivate={(id) => {
                  const next = projects.find((p) => p.id === id) ?? null;
                  setPendingReactivate(next);
                }}
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
      <ConfirmModal
        open={pendingReactivate !== null}
        onClose={() => {
          if (busyId === null) setPendingReactivate(null);
        }}
        onConfirm={handleConfirmReactivate}
        title="Reactivate project?"
        description={
          pendingReactivate
            ? `Reactivate “${pendingReactivate.title}” for ${phase.semesterName ?? "the current cycle"}? Students from ${pendingReactivate.semesterName || "the previous cycle"} stay on record, and the live roster starts empty.`
            : "Reactivate this project for the current cycle?"
        }
        confirmLabel="Reactivate"
        busyLabel="Reactivating…"
        busy={pendingReactivate !== null && busyId === pendingReactivate.id}
      />
    </div>
  );
}
