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
  type ProjectResearchAssistant,
} from "@/lib/research-assistants-api";
import { ResearchAssistantChooser } from "@/components/projects/ResearchAssistantChooser";

const SECTION_COPY =
  "People helping run this project outside the portal. They do not take a volunteer seat and get no access here.";

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
  const [removeTarget, setRemoveTarget] = useState<ProjectResearchAssistant | null>(null);
  const [removing, setRemoving] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  const assistants = query.data;
  const busy = adding || removing;

  function messageFor(err: unknown, fallback: string) {
    return err instanceof ApiError ? (err.errors[0] ?? err.message) : fallback;
  }

  /** Resolves true when saved, so the form can clear itself. */
  async function add({ email }: { email: string }): Promise<boolean> {
    setAdding(true);
    setActionError(null);
    try {
      await addResearchAssistant(projectId, email);
      await query.reload({ silent: true });
      return true;
    } catch (err) {
      setActionError(messageFor(err, "Could not add this research assistant."));
      return false;
    } finally {
      setAdding(false);
    }
  }

  async function confirmRemove() {
    if (!removeTarget) return;
    setRemoving(true);
    setActionError(null);
    try {
      await removeResearchAssistant(projectId, removeTarget.id);
      setRemoveTarget(null);
      await query.reload({ silent: true });
    } catch (err) {
      setActionError(messageFor(err, "Could not remove this research assistant."));
      setRemoveTarget(null);
    } finally {
      setRemoving(false);
    }
  }

  const body = (
    <>
      <Messages tone={tone} error={query.error} actionError={actionError} />
      <AssistantList
        tone={tone}
        assistants={assistants ?? []}
        loading={assistants == null && query.error == null}
        failed={query.error != null}
        busy={busy}
        onRemove={setRemoveTarget}
      />
      <div className={tone === "faculty" ? "mt-5" : "research-assistant-chooser-wrap"}>
        <ResearchAssistantChooser disabled={busy} onSelect={add} />
      </div>
      <ConfirmModal
        open={removeTarget !== null}
        onClose={() => {
          if (!removing) setRemoveTarget(null);
        }}
        onConfirm={confirmRemove}
        title="Remove this research assistant?"
        description={
          removeTarget
            ? `${removeTarget.name ?? removeTarget.email} will no longer be listed as a research assistant on this project.`
            : undefined
        }
        confirmLabel="Remove"
        busyLabel="Removing…"
        busy={removing}
      />
    </>
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
          <p className="admin-widget-sub">{SECTION_COPY}</p>
        </header>
        {body}
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
        {SECTION_COPY}
      </Text>
      {body}
    </section>
  );
}

function Messages({
  tone,
  error,
  actionError,
}: {
  tone: "admin" | "faculty";
  error: string | null;
  actionError: string | null;
}) {
  const messages = [error, actionError].filter((message): message is string => Boolean(message));
  if (messages.length === 0) return null;

  return (
    <>
      {messages.map((message) =>
        tone === "admin" ? (
          <p key={message} className="admin-users-banner is-error" role="alert">
            {message}
          </p>
        ) : (
          <Text key={message} as="p" size="3" mt="4" role="alert" className="!text-red-800">
            {message}
          </Text>
        ),
      )}
    </>
  );
}

function AssistantList({
  tone,
  assistants,
  loading,
  failed,
  busy,
  onRemove,
}: {
  tone: "admin" | "faculty";
  assistants: ProjectResearchAssistant[];
  loading: boolean;
  failed: boolean;
  busy: boolean;
  onRemove: (assistant: ProjectResearchAssistant) => void;
}) {
  if (loading) return <RankingsListSkeleton />;
  if (failed) return null;

  if (assistants.length === 0) {
    return tone === "admin" ? (
      <p className="admin-users-status">No research assistant yet.</p>
    ) : (
      <Text as="p" size="3" mt="5" className="!text-muted">
        No research assistant yet.
      </Text>
    );
  }

  if (tone === "admin") {
    return (
      <ul className="admin-person-list">
        {assistants.map((assistant) => (
          <li key={assistant.id} className="admin-person-card bg-slate-50">
            <div className="admin-person-card-main">
              {!assistant.hasSignedIn ? (
                <div className="admin-person-badges">
                  <span className="admin-rank-badge is-muted">Hasn&apos;t signed in yet</span>
                </div>
              ) : null}
              <p className="admin-person-name">{assistant.name ?? assistant.email}</p>
            </div>
            <div className="admin-person-card-actions">
              <DeleteIconButton
                label={`Remove ${assistant.name ?? assistant.email}`}
                disabled={busy}
                onClick={() => onRemove(assistant)}
              />
            </div>
            {assistant.name ? (
              <p className="admin-person-meta">
                <a className="admin-users-email" href={`mailto:${assistant.email}`}>
                  {assistant.email}
                </a>
              </p>
            ) : null}
          </li>
        ))}
      </ul>
    );
  }

  return (
    <ul className="ranked-students-list">
      {assistants.map((assistant) => (
        <li key={assistant.id} className="ranked-students-row">
          {!assistant.hasSignedIn ? (
            <div className="ranked-students-badges">
              <span className="rank-badge is-muted">Hasn&apos;t signed in yet</span>
            </div>
          ) : null}
          <p className="ranked-students-name">{assistant.name ?? assistant.email}</p>
          {assistant.name ? <p className="ranked-students-meta">{assistant.email}</p> : null}
          <div className="ranked-students-actions">
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={busy}
              onClick={() => onRemove(assistant)}
            >
              Remove
            </Button>
          </div>
        </li>
      ))}
    </ul>
  );
}
