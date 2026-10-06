"use client";

import { useEffect, useId, useState } from "react";
import Link from "next/link";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { BackLink } from "@/components/ui/BackLink";
import { Button } from "@/components/ui/Button";
import { AdminFormSkeleton } from "@/components/ui/SectionSkeletons";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { getAdminProject } from "@/lib/admin-projects-api";
import { ApiError } from "@/lib/api";
import { getAssignmentGate } from "@/lib/assignment-gate";
import { adminProjectHref, adminStudentProfileHref } from "@/lib/auth";
import {
  assignStudentToProject,
  listAssignmentCandidates,
  type AssignmentCandidate,
} from "@/lib/matching-api";
import { projectStatusClass } from "@/lib/project-form";
import { getActiveSemester } from "@/lib/semesters-api";

const PAGE_SIZE = 6;

type CandidateList = "recommended" | "all";

export function AdminAssignStudentsView({ projectId }: { projectId: string }) {
  const searchId = useId();
  const [list, setList] = useState<CandidateList>("recommended");
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [busyIds, setBusyIds] = useState<ReadonlySet<string>>(new Set());
  const [selected, setSelected] = useState<ReadonlySet<string>>(new Set());
  const [actionError, setActionError] = useState<string | null>(null);

  useEffect(() => {
    const timeout = window.setTimeout(() => {
      setSearch(searchInput.trim());
      setPageNumber(1);
    }, 250);
    return () => window.clearTimeout(timeout);
  }, [searchInput]);

  const {
    data: loaded,
    error: projectError,
    reload: reloadProject,
  } = useCancellableQuery(
    async () => {
      const [detail, semester] = await Promise.all([
        getAdminProject(projectId),
        getActiveSemester(),
      ]);
      return { detail, semester };
    },
    [projectId],
    { fallbackError: "Failed to load project." },
  );

  const {
    data,
    loading,
    error,
    reload: reloadCandidates,
  } = useCancellableQuery(
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

  const backHref = adminProjectHref(projectId);

  if (!loaded) {
    return (
      <div className="admin-panel admin-panel--wide">
        <AdminPageHeader
          title="Assign students"
          description={projectError ?? "Loading project details."}
          backHref={backHref}
          backLabel="Back to project"
        />
        {projectError ? null : <AdminFormSkeleton fields={6} />}
      </div>
    );
  }

  const { project, assignments } = loaded.detail;
  const assigned = assignments ?? [];
  const { assignmentAllowed, blockedReason } = getAssignmentGate(
    project,
    loaded.semester,
  );
  const filled = assigned.length;
  const total = project.volunteersRequired;
  const remaining = Math.max(0, total - filled);
  const totalPages = Math.max(1, data?.totalPages ?? 1);
  const candidates = data?.items ?? [];
  const count = data?.totalCount ?? 0;

  const selectable = assignmentAllowed
    ? candidates.filter((candidate) => candidate.assignedProjectId == null)
    : [];
  const allPageSelected =
    selectable.length > 0 &&
    selectable.every((candidate) => selected.has(candidate.userId));

  const working = busyIds.size > 0;
  const selectedCount = selected.size;

  function toggle(studentUserId: string) {
    setSelected((current) => {
      const next = new Set(current);
      if (!next.delete(studentUserId)) next.add(studentUserId);
      return next;
    });
  }

  function togglePage() {
    setSelected((current) => {
      const next = new Set(current);
      for (const candidate of selectable) {
        if (allPageSelected) next.delete(candidate.userId);
        else next.add(candidate.userId);
      }
      return next;
    });
  }

  /** Assigns students one by one; failures stay selected and are reported together. */
  async function assign(studentUserIds: string[]) {
    setBusyIds(new Set(studentUserIds));
    setActionError(null);
    const failures: string[] = [];
    const done = new Set<string>();
    for (const studentUserId of studentUserIds) {
      try {
        await assignStudentToProject({ projectId, studentUserId });
        done.add(studentUserId);
      } catch (err) {
        const name =
          candidates.find((c) => c.userId === studentUserId)?.name ?? "Student";
        failures.push(
          `${name}: ${
            err instanceof ApiError
              ? (err.errors[0] ?? err.message)
              : "Failed to assign."
          }`,
        );
      }
    }
    setSelected((current) => new Set([...current].filter((id) => !done.has(id))));
    if (failures.length > 0) setActionError(failures.join(" "));
    await Promise.all([reloadProject(), reloadCandidates({ silent: true })]);
    setBusyIds(new Set());
  }

  function showList(next: CandidateList) {
    setList(next);
    setPageNumber(1);
    setActionError(null);
  }

  return (
    <div className="admin-panel admin-panel--wide">
      <div className="admin-detail-back">
        <BackLink href={backHref}>Back to project</BackLink>
      </div>

      <div className="assign-page">
        <aside className="assign-page-side" aria-label="Project summary">
          <section className="admin-widget">
            <p className="field-label">Assigning to</p>
            <h3 className="assign-page-project">{project.title}</h3>
            <p className="admin-widget-sub">
              {project.facultyName} · {project.affiliation}
            </p>
            <span
              className={`admin-value-status ${projectStatusClass(project.status)}`}
            >
              {project.status}
            </span>

            <div className="assign-page-seats">
              <p className="assign-page-seats-count">
                <strong>{filled}</strong>/{total} seats filled
              </p>
              <div
                className="admin-meter-track"
                role="progressbar"
                aria-valuenow={total > 0 ? Math.min(100, (filled / total) * 100) : 0}
                aria-valuemin={0}
                aria-valuemax={100}
                aria-label="Seat fill"
              >
                <span
                  className="admin-meter-fill"
                  style={{
                    width: `${total > 0 ? Math.min(100, (filled / total) * 100) : 0}%`,
                  }}
                />
              </div>
              <p className="admin-widget-sub">
                {remaining === 0
                  ? "All seats are taken."
                  : `${remaining} seat${remaining === 1 ? "" : "s"} remaining.`}
              </p>
            </div>
          </section>

          <section className="admin-widget">
            <h3 className="admin-widget-title">Assigned ({filled})</h3>
            {assigned.length === 0 ? (
              <p className="admin-widget-sub">No students assigned yet.</p>
            ) : (
              <ul className="assign-page-assigned">
                {assigned.map((assignment) => (
                  <li key={assignment.id}>
                    <Link
                      href={adminStudentProfileHref(
                        projectId,
                        assignment.studentUserId,
                      )}
                    >
                      {assignment.studentName}
                    </Link>
                    <span className="admin-widget-sub">
                      {assignment.studentEmail}
                    </span>
                  </li>
                ))}
              </ul>
            )}
          </section>

          <section className="admin-widget">
            <h3 className="admin-widget-title">What this project looks for</h3>
            <p className="assign-page-quals">
              {project.minQualifications?.trim() || "No minimum qualifications."}
            </p>
            {project.researchAreas.length > 0 ? (
              <div className="field-display-chips">
                {project.researchAreas.map((area) => (
                  <span key={area} className="multi-select-chip">
                    <span className="multi-select-chip-label">{area}</span>
                  </span>
                ))}
              </div>
            ) : null}
          </section>
        </aside>

        <div className="assign-page-main">
          <AdminPageHeader
            title="Assign students"
            description="Choose any student, including students who have not ranked this project."
            tag={`${count} student${count === 1 ? "" : "s"}`}
          />

          {!assignmentAllowed && blockedReason ? (
            <p className="admin-users-banner" role="status">
              {blockedReason}
            </p>
          ) : null}

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
            <div
              className="assign-student-tabs"
              role="tablist"
              aria-label="Student lists"
            >
              {(
                [
                  ["recommended", "Recommended"],
                  ["all", "All students"],
                ] as const
              ).map(([key, label]) => (
                <button
                  key={key}
                  type="button"
                  role="tab"
                  aria-selected={list === key}
                  className={`assign-student-tab${list === key ? " is-active" : ""}`}
                  onClick={() => showList(key)}
                >
                  {label}
                </button>
              ))}
            </div>
          </div>

          <div className="assign-student-hint-row">
            <p className="assign-student-hint">
              {list === "recommended"
                ? "Students whose research interests overlap this project, or whose faculty or major is named in its qualifications."
                : "Every unassigned student account. Cards show only the profile details that relate to this project."}
            </p>
            {selectable.length > 0 ? (
              <Button
                type="button"
                variant="outline"
                size="sm"
                disabled={working}
                onClick={togglePage}
              >
                {allPageSelected ? "Deselect page" : "Select page"}
              </Button>
            ) : null}
          </div>

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

          {loading && candidates.length === 0 ? (
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
            <div className="admin-people" aria-busy={loading}>
              <ul className="admin-person-list">
                {candidates.map((candidate) => (
                  <CandidateCard
                    key={candidate.userId}
                    candidate={candidate}
                    projectId={projectId}
                    canAssign={assignmentAllowed}
                    blockedReason={blockedReason}
                    selected={selected.has(candidate.userId)}
                    busy={busyIds.has(candidate.userId)}
                    disabled={working}
                    onToggle={() => toggle(candidate.userId)}
                    onAssign={() => void assign([candidate.userId])}
                  />
                ))}
              </ul>
            </div>
          )}

          {selectedCount > 0 ? (
            <div className="assign-selection-bar" role="region" aria-label="Selection">
              <span className="assign-selection-count">
                {selectedCount} selected
                {selectedCount > remaining
                  ? ` · only ${remaining} seat${remaining === 1 ? "" : "s"} left`
                  : ""}
              </span>
              <Button
                type="button"
                variant="outline"
                size="sm"
                disabled={working}
                onClick={() => setSelected(new Set())}
              >
                Clear
              </Button>
              <Button
                type="button"
                variant="primary"
                size="sm"
                disabled={working || !assignmentAllowed}
                onClick={() => void assign([...selected])}
              >
                {working ? "Assigning…" : `Assign ${selectedCount}`}
              </Button>
            </div>
          ) : null}

          <div className="admin-users-pager">
            <p className="admin-users-count">
              {loading ? "Loading students…" : `${count} student${count === 1 ? "" : "s"}`}
            </p>
            <div className="admin-users-pager-actions">
              <Button
                type="button"
                variant="outline"
                size="sm"
                disabled={pageNumber <= 1 || loading || working}
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
                disabled={pageNumber >= totalPages || loading || working}
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
    </div>
  );
}

function CandidateCard({
  candidate,
  projectId,
  canAssign,
  blockedReason,
  selected,
  busy,
  disabled,
  onToggle,
  onAssign,
}: {
  candidate: AssignmentCandidate;
  projectId: string;
  canAssign: boolean;
  blockedReason?: string;
  selected: boolean;
  busy: boolean;
  disabled: boolean;
  onToggle: () => void;
  onAssign: () => void;
}) {
  const assignedElsewhere = candidate.assignedProjectId != null;
  const study = [candidate.degree, candidate.faculty, candidate.major]
    .filter(Boolean)
    .join(" · ");
  const noMatch =
    candidate.matchedResearchTopics.length === 0 &&
    !candidate.qualificationsMentionMajor &&
    !candidate.qualificationsMentionFaculty;

  return (
    <li
      className={`admin-person-card bg-slate-50${selected ? " is-selected" : ""}`}
    >
      <div className="admin-person-card-main">
        <div className="admin-person-badges">
          {candidate.matchedResearchTopics.map((topic) => (
            <span key={topic} className="admin-rank-badge" title={topic}>
              {topic}
            </span>
          ))}
          {candidate.qualificationsMentionMajor ? (
            <span className="admin-rank-badge">Major in qualifications</span>
          ) : null}
          {candidate.qualificationsMentionFaculty ? (
            <span className="admin-rank-badge">Faculty in qualifications</span>
          ) : null}
          {noMatch ? (
            <span className="admin-rank-badge is-muted">
              No shared research interests
            </span>
          ) : null}
        </div>
        <p className="admin-person-name">{candidate.name}</p>
        {assignedElsewhere ? (
          <p className="admin-person-meta">
            On {candidate.assignedProjectTitle ?? "another project"}
          </p>
        ) : null}
      </div>
      <div className="admin-person-card-actions">
        {assignedElsewhere || !canAssign ? null : (
          <input
            type="checkbox"
            className="assign-student-check"
            aria-label={`Select ${candidate.name}`}
            checked={selected}
            disabled={disabled}
            onChange={onToggle}
          />
        )}
        {assignedElsewhere ? (
          <span className="admin-rank-badge is-muted">On another project</span>
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
      <div className="admin-person-meta-row">
        <p className="admin-person-meta">
          <a className="admin-users-email" href={`mailto:${candidate.email}`}>
            {candidate.email}
          </a>
          <span aria-hidden> · </span>
          {candidate.hasProfile ? study || "Profile saved" : "No URVP profile yet"}
        </p>
        <Link
          href={adminStudentProfileHref(projectId, candidate.userId)}
          className="admin-person-profile"
        >
          View Profile
          <span aria-hidden>→</span>
        </Link>
      </div>
    </li>
  );
}