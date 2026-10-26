"use client";

import { useState } from "react";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { DeleteIconButton } from "@/components/ui/DeleteIconButton";
import { RankingsListSkeleton } from "@/components/ui/SectionSkeletons";
import { Heading, Text } from "@/components/ui/Typography";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { ApiError } from "@/lib/api";
import {
  addResearchAssistant,
  listResearchAssistants,
  removeResearchAssistant,
  type AddResearchAssistantInput,
  type ProjectResearchAssistant,
} from "@/lib/research-assistants-api";
import { ResearchAssistantChooser } from "@/components/projects/ResearchAssistantChooser";

export function ProjectResearchAssistants({
  projectId,
  tone,
}: {
  projectId: string;
  tone: "admin" | "faculty";
}) {
  const query = useCancellableQuery(
    () => listResearchAssistants(projectId),
    [projectId],
    { fallbackError: "Could not load research assistants." },
  );
  const [adding, setAdding] = useState(false);
  const [removingId, setRemovingId] = useState<string | null>(null);
  const [removeTarget, setRemoveTarget] = useState<ProjectResearchAssistant | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  const assistants = query.data;
  const busy = adding || removingId !== null;

  async function add(selection: AddResearchAssistantInput) {
    setAdding(true);
    setActionError(null);
    try {
      await addResearchAssistant(projectId, selection);
      await query.reload();
    } catch (err) {
      setActionError(
        err instanceof ApiError ? (err.errors[0] ?? err.message) : "Could not add this research assistant.",
      );
    } finally {
      setAdding(false);
    }
  }

  async function confirmRemove() {
    if (!removeTarget) return;
    const target = removeTarget;
    setRemovingId(target.id);
    setActionError(null);
    try {
      await removeResearchAssistant(projectId, target.id);
      setRemoveTarget(null);
      await query.reload();
    } catch (err) {
      setActionError(
        err instanceof ApiError ? (err.errors[0] ?? err.message) : "Could not remove this research assistant.",
      );
    } finally {
      setRemovingId(null);
    }
  }

  const list = (
    <AssistantList
      tone={tone}
      assistants={assistants ?? []}
      loading={assistants == null && query.error == null}
      error={query.error}
      actionError={actionError}
      busy={busy}
      removingId={removingId}
      onRemove={setRemoveTarget}
      onSelect={(selection) => void add(selection)}
    />
  );

  const modal = (
    <ConfirmModal
      open={removeTarget !== null}
      onClose={() => {
        if (removingId === null) setRemoveTarget(null);
      }}
      onConfirm={confirmRemove}
      title="Remove this research assistant?"
      description={
        removeTarget
          ? `${removeTarget.name} will no longer be listed as a research assistant on this project.`
          : undefined
      }
      confirmLabel="Remove"
      busyLabel="Removing…"
      busy={removingId !== null}
    />
  );

  if (tone === "admin") {
    return (
      <section
        className="admin-widget admin-people admin-project-assistants"
        aria-labelledby="research-assistants-heading"
      >
        <header className="admin-widget-head">
          <h3 id="research-assistants-heading" className="admin-widget-title">
            Research assistants
          </h3>
          <p className="admin-widget-sub">
            People helping run this project. They do not take a volunteer seat.
          </p>
        </header>
        {list}
        {modal}
      </section>
    );
  }

  return (
    <section className="form-section" aria-labelledby="research-assistants-heading">
      <Heading
        as="h2"
        size="5"
        weight="medium"
        id="research-assistants-heading"
        className="!font-[family-name:var(--font-display)] !text-primary"
      >
        Research assistants
      </Heading>
      <Text as="p" size="2" mt="2" className="!text-muted">
        People helping run this project. They do not take a volunteer seat.
      </Text>
      {list}
      {modal}
    </section>
  );
}

function AssistantList({
  tone,
  assistants,
  loading,
  error,
  actionError,
  busy,
  removingId,
  onRemove,
  onSelect,
}: {
  tone: "admin" | "faculty";
  assistants: ProjectResearchAssistant[];
  loading: boolean;
  error: string | null;
  actionError: string | null;
  busy: boolean;
  removingId: string | null;
  onRemove: (assistant: ProjectResearchAssistant) => void;
  onSelect: (selection: AddResearchAssistantInput) => void;
}) {
  return (
    <>
      {error ? (
        <p className={tone === "admin" ? "admin-users-banner is-error" : undefined} role="alert">
          {tone === "faculty" ? (
            <Text as="span" size="3" className="!text-red-800">
              {error}
            </Text>
          ) : (
            error
          )}
        </p>
      ) : null}
      {actionError ? (
        <p className={tone === "admin" ? "admin-users-banner is-error" : undefined} role="alert">
          {tone === "faculty" ? (
            <Text as="span" size="3" className="!text-red-800">
              {actionError}
            </Text>
          ) : (
            actionError
          )}
        </p>
      ) : null}

      {loading ? (
        <RankingsListSkeleton />
      ) : error ? null : assistants.length === 0 ? (
        tone === "admin" ? (
          <p className="admin-users-status">No research assistant yet.</p>
        ) : (
          <Text as="p" size="3" mt="5" className="!text-muted">
            No research assistant yet.
          </Text>
        )
      ) : tone === "admin" ? (
        <ul className="admin-person-list">
          {assistants.map((assistant) => (
            <li key={assistant.id} className="admin-person-card bg-slate-50">
              <div className="admin-person-card-main">
                <p className="admin-person-name">{assistant.name}</p>
              </div>
              <div className="admin-person-card-actions">
                <DeleteIconButton
                  label={`Remove ${assistant.name}`}
                  disabled={busy}
                  loading={removingId === assistant.id}
                  onClick={() => onRemove(assistant)}
                />
              </div>
              <p className="admin-person-meta">
                <a className="admin-users-email" href={`mailto:${assistant.email}`}>
                  {assistant.email}
                </a>
              </p>
            </li>
          ))}
        </ul>
      ) : (
        <ul className="ranked-students-list">
          {assistants.map((assistant) => (
            <li key={assistant.id} className="ranked-students-row">
              <p className="ranked-students-name">{assistant.name}</p>
              <p className="ranked-students-meta">{assistant.email}</p>
              <div className="ranked-students-actions">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  disabled={busy}
                  onClick={() => onRemove(assistant)}
                >
                  {removingId === assistant.id ? "Removing…" : "Remove"}
                </Button>
              </div>
            </li>
          ))}
        </ul>
      )}

      <div className={tone === "faculty" ? "mt-5" : "research-assistant-chooser-wrap"}>
        <ResearchAssistantChooser disabled={busy} onSelect={onSelect} />
      </div>
    </>
  );
}
