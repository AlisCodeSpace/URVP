"use client";

import { useEffect, useId, useState } from "react";
import Link from "next/link";
import { Button } from "@/components/ui/Button";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { useScrollLock } from "@/hooks/useScrollLock";
import { ApiError } from "@/lib/api";
import { adminStudentProfileHref } from "@/lib/auth";
import {
  assignStudentToProject,
  listAssignmentCandidates,
  type AssignmentCandidate,
} from "@/lib/matching-api";

const PAGE_SIZE = 6;

type CandidateList = "recommended" | "all";

export function AssignStudentModal({
  projectId,
  projectTitle,
  canAssign,
  blockedReason,
  onClose,
  onAssigned,
}: {
  projectId: string;
  projectTitle: string;
  canAssign: boolean;
  blockedReason?: string;
  onClose: () => void;
  onAssigned: () => Promise<void>;
}) {
  const titleId = useId();
  const searchId = useId();
  const [list, setList] = useState<CandidateList>("recommended");
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [busyId, setBusyId] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);

  useScrollLock(true);

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      setSearch(searchInput.trim());
      setPageNumber(1);
    }, 250);
    return () => window.clearTimeout(timeout);
  }, [searchInput]);

  useEffect(() => {
    function onKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape" && busyId === null) onClose();
    }

    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [busyId, onClose]);

  const { data, loading, error, reload } = useCancellableQuery(
    () =>
      listAssignmentCandidates({
        projectId,
        search,
        recommendedOnly: list === "recommended",
        pageNumber,
        pageSize: PAGE_SIZE,
      }),
    [projectId, search, list, pageNumber],
    { fallbackError: "Failed to load students.", keepDataOnError: true },
  );

  const totalPages = Math.max(1, data?.totalPages ?? 1);
  const candidates = data?.items ?? [];

  async function assign(studentUserId: string) {
    setBusyId(studentUserId);
    setActionError(null);
    try {
      await assignStudentToProject({ projectId, studentUserId });
      await onAssigned();
      await reload({ silent: true });
    } catch (err) {
      setActionError(
        err instanceof ApiError
          ? (err.errors[0] ?? err.message)
          : "Failed to assign student.",
      );
    } finally {
      setBusyId(null);
    }
  }

  function showList(next: CandidateList) {
    setList(next);
    setPageNumber(1);
    setActionError(null);
  }

  return (
    <div
      className="dialog-layer fixed inset-0 z-50 flex items-center justify-center"
      role="presentation"
    >
      <button
        type="button"
        className="absolute inset-0 bg-primary/45 backdrop-blur-[2px]"
        aria-label="Close"
        disabled={busyId !== null}
        onClick={() => {
          if (busyId === null) onClose();
        }}
      />

      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className="dialog-layer-panel assign-student-dialog relative z-10 w-full rounded-[var(--radius-lg)] border border-primary/12 bg-surface shadow-[0_24px_60px_-28px_rgba(61,18,72,0.45)]"
      >
        <div className="assign-student-dialog-head">
          <div className="assign-student-dialog-copy">
            <h2 id={titleId} className="assign-student-dialog-title">
              Assign a student
            </h2>
            <p className="assign-student-dialog-sub">
              Choose any student for &ldquo;{projectTitle}&rdquo;, including
              students who have not ranked a project.
            </p>
          </div>
          <Button
            type="button"
            variant="outline"
            size="sm"
            className="assign-student-close"
            disabled={busyId !== null}
            onClick={onClose}
          >
            Close
          </Button>
        </div>

        <div className="assign-student-toolbar">
          <div className="admin-assign-search">
            <label className="field-label" htmlFor={searchId}>
              Search students
            </label>
            <input
              id={searchId}
              type="search"
              className="field-input"
              placeholder="Name, email, faculty, or major"
              value={searchInput}
              onChange={(event) => setSearchInput(event.target.value)}
            />
          </div>
          <div className="assign-student-tabs" role="tablist" aria-label="Student lists">
            <button
              type="button"
              role="tab"
              aria-selected={list === "recommended"}
              className={`assign-student-tab${list === "recommended" ? " is-active" : ""}`}
              onClick={() => showList("recommended")}
            >
              Recommended
            </button>
            <button
              type="button"
              role="tab"
              aria-selected={list === "all"}
              className={`assign-student-tab${list === "all" ? " is-active" : ""}`}
              onClick={() => showList("all")}
            >
              All students
            </button>
          </div>
        </div>

        <p className="assign-student-hint">
          {list === "recommended"
            ? "Students whose research interests overlap this project, or whose faculty or major is named in its qualifications."
            : "Every student account. Cards show only the profile details that relate to this project."}
        </p>

        {actionError ? (
          <p className="admin-users-banner is-error" role="alert">
            {actionError}
          </p>
        ) : null}
        {error && candidates.length === 0 ? (
          <p className="admin-users-banner is-error" role="alert">
            {error}
          </p>
        ) : null}

        <div className="assign-student-list">
          {loading ? (
            <div className="assign-student-loading" role="status">
              <span className="assign-student-spinner" aria-hidden />
              Loading students
            </div>
          ) : candidates.length === 0 ? (
            <p className="admin-users-status">
              {list === "recommended"
                ? "No students match this project's research interests or qualifications."
                : "No students match this search."}
            </p>
          ) : (
            <ul className="assign-student-cards">
              {candidates.map((candidate) => (
                <CandidateCard
                  key={candidate.userId}
                  candidate={candidate}
                  projectId={projectId}
                  canAssign={canAssign}
                  blockedReason={blockedReason}
                  busy={busyId === candidate.userId}
                  disabled={busyId !== null}
                  onAssign={() => void assign(candidate.userId)}
                />
              ))}
            </ul>
          )}
        </div>

        <div className="admin-users-pager">
          <p className="admin-users-count">
            {loading
              ? "Loading students…"
              : `${data?.totalCount ?? 0} student${(data?.totalCount ?? 0) === 1 ? "" : "s"}`}
          </p>
          <div className="admin-users-pager-actions">
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={pageNumber <= 1 || loading || busyId !== null}
              onClick={() => setPageNumber((page) => Math.max(1, page - 1))}
            >
              Previous
            </Button>
            <span className="admin-users-page-label">
              Page {pageNumber} of {totalPages}
            </span>
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={pageNumber >= totalPages || loading || busyId !== null}
              onClick={() =>
                setPageNumber((page) => Math.min(totalPages, page + 1))
              }
            >
              Next
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
}

function CandidateCard({
  candidate,
  projectId,
  canAssign,
  blockedReason,
  busy,
  disabled,
  onAssign,
}: {
  candidate: AssignmentCandidate;
  projectId: string;
  canAssign: boolean;
  blockedReason?: string;
  busy: boolean;
  disabled: boolean;
  onAssign: () => void;
}) {
  const assignedHere = candidate.assignedProjectId === projectId;
  const assignedElsewhere =
    candidate.assignedProjectId != null && !assignedHere;
  const study = [candidate.degree, candidate.faculty, candidate.major]
    .filter(Boolean)
    .join(" · ");

  return (
    <li className="assign-student-card">
      <div className="assign-student-card-main">
        <p className="assign-student-name">{candidate.name}</p>
        <p className="assign-student-line">
          <a className="admin-users-email" href={`mailto:${candidate.email}`}>
            {candidate.email}
          </a>
        </p>
        <p className="assign-student-line">
          {candidate.hasProfile ? study || "Profile saved" : "No URVP profile yet"}
        </p>
        <div className="assign-student-chips">
          {candidate.matchedResearchTopics.map((topic) => (
            <span key={topic} className="admin-rank-badge assign-student-chip" title={topic}>
              {topic}
            </span>
          ))}
          {candidate.qualificationsMentionMajor ? (
            <span className="admin-rank-badge assign-student-chip" title="Qualifications mention this major">
              Major in qualifications
            </span>
          ) : null}
          {candidate.qualificationsMentionFaculty ? (
            <span className="admin-rank-badge assign-student-chip" title="Qualifications mention this faculty">
              Faculty in qualifications
            </span>
          ) : null}
          {candidate.matchedResearchTopics.length === 0 &&
          !candidate.qualificationsMentionMajor &&
          !candidate.qualificationsMentionFaculty ? (
            <span className="admin-rank-badge is-muted assign-student-chip">
              No shared research interests
            </span>
          ) : null}
        </div>
      </div>
      <div className="assign-student-card-actions">
        {assignedHere ? (
          <span className="admin-rank-badge is-assigned">Assigned here</span>
        ) : assignedElsewhere ? (
          <span
            className="admin-rank-badge is-muted assign-student-chip"
            title={candidate.assignedProjectTitle ?? "Another project"}
          >
            On {candidate.assignedProjectTitle ?? "another project"}
          </span>
        ) : (
          <Button
            type="button"
            variant="primary"
            size="sm"
            disabled={!canAssign || disabled}
            title={!canAssign ? blockedReason : undefined}
            onClick={onAssign}
          >
            {busy ? "Assigning…" : "Assign"}
          </Button>
        )}
      </div>
      <Link
        href={adminStudentProfileHref(projectId, candidate.userId)}
        className="admin-person-profile assign-student-profile"
      >
        View Profile
        <span aria-hidden>→</span>
      </Link>
    </li>
  );
}
