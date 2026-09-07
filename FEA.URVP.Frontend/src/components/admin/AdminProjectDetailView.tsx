"use client";

import { useCallback, useEffect, useId, useMemo, useState, type ReactNode } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { Tag } from "@/components/ui/Tag";
import { AdminFormSkeleton, RankingsListSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import {
  getAdminProject,
  type AdminProjectDetailDto,
  type ProjectRankingStudentDto,
} from "@/lib/admin-projects-api";
import {
  assignStudentToProject,
  updatePlacementStatus,
  type PlacementDto,
} from "@/lib/matching-api";
import { formatProjectDate } from "@/lib/project-form";
import {
  formatRankedAt,
  rankLabel,
} from "@/lib/project-rankings-api";
import type { ProjectDto } from "@/lib/projects-api";
import { listUsers, type UserDto } from "@/lib/users-api";

export function AdminProjectDetailView({ projectId }: { projectId: string }) {
  const searchId = useId();
  const [data, setData] = useState<AdminProjectDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [actionError, setActionError] = useState<string | null>(null);
  const [busyStudentId, setBusyStudentId] = useState<string | null>(null);
  const [removeTarget, setRemoveTarget] = useState<PlacementDto | null>(null);
  const [removing, setRemoving] = useState(false);
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [candidates, setCandidates] = useState<UserDto[]>([]);
  const [searching, setSearching] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await getAdminProject(projectId));
    } catch (err) {
      setData(null);
      setError(
        err instanceof ApiError ? err.message : "Failed to load project.",
      );
    } finally {
      setLoading(false);
    }
  }, [projectId]);

  useEffect(() => {
    void load();
  }, [load]);

  useEffect(() => {
    const handle = window.setTimeout(() => setSearch(searchInput.trim()), 300);
    return () => window.clearTimeout(handle);
  }, [searchInput]);

  const assignedIds = useMemo(
    () => new Set((data?.assignments ?? []).map((a) => a.studentUserId)),
    [data],
  );

  useEffect(() => {
    if (!search || search.length < 2) {
      setCandidates([]);
      setSearching(false);
      return;
    }

    let cancelled = false;
    setSearching(true);
    void listUsers({
      search,
      role: "Student",
      pageSize: 8,
      sortBy: "Name",
      sortDir: "Asc",
    })
      .then((page) => {
        if (!cancelled) setCandidates(page.items);
      })
      .catch(() => {
        if (!cancelled) setCandidates([]);
      })
      .finally(() => {
        if (!cancelled) setSearching(false);
      });

    return () => {
      cancelled = true;
    };
  }, [search]);

  const rankSummary = useMemo(() => {
    if (!data?.rankings.length) return null;
    const counts = { 1: 0, 2: 0, 3: 0 };
    for (const ranking of data.rankings) {
      if (ranking.rank === 1 || ranking.rank === 2 || ranking.rank === 3) {
        counts[ranking.rank] += 1;
      }
    }
    return counts;
  }, [data]);

  const actionMessage = (err: unknown, fallback: string) =>
    err instanceof ApiError ? (err.errors[0] ?? err.message) : fallback;

  async function assign(studentUserId: string) {
    setBusyStudentId(studentUserId);
    setActionError(null);
    try {
      await assignStudentToProject({ projectId, studentUserId });
      setSearchInput("");
      setSearch("");
      await load();
    } catch (err) {
      setActionError(actionMessage(err, "Failed to assign student."));
    } finally {
      setBusyStudentId(null);
    }
  }

  async function confirmRemove() {
    if (!removeTarget) return;
    setRemoving(true);
    setActionError(null);
    try {
      await updatePlacementStatus(removeTarget.id, "Cancelled");
      setRemoveTarget(null);
      await load();
    } catch (err) {
      setActionError(actionMessage(err, "Failed to remove student."));
    } finally {
      setRemoving(false);
    }
  }

  if (loading && !data) {
    return (
      <div className="admin-panel admin-panel--wide">
        <AdminPageHeader title="Project" description="Loading project details." />
        <AdminFormSkeleton fields={8} />
        <RankingsListSkeleton count={4} />
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="admin-panel admin-panel--wide">
        <div className="admin-detail-back">
          <Button href="/admin/projects" variant="outline" size="sm">
            Back to projects
          </Button>
        </div>
        <p className="admin-users-banner is-error" role="alert">
          {error ?? "Project not found."}
        </p>
        <Button type="button" variant="outline" size="sm" onClick={() => void load()}>
          Retry
        </Button>
      </div>
    );
  }

  const { project, rankings, assignments } = data;
  const assigned = assignments ?? [];
  const remaining = Math.max(0, project.volunteersRequired - assigned.length);
  const seatsOpen = project.status !== "Closed" && remaining > 0;
  const canAssign = seatsOpen && busyStudentId === null;
  const searchableCandidates = candidates.filter((u) => !assignedIds.has(u.id));

  return (
    <div className="admin-panel admin-panel--wide">
      <div className="admin-detail-back">
        <Button href="/admin/projects" variant="outline" size="sm">
          Back to projects
        </Button>
      </div>

      <AdminPageHeader
        title={project.title}
        description={`${project.facultyName} · ${project.affiliation}`}
        tag={project.status}
      />

      <div className="admin-detail-tags">
        <Tag>
          {assigned.length} of {project.volunteersRequired} seat
          {project.volunteersRequired === 1 ? "" : "s"} filled
        </Tag>
        <Tag>
          {rankings.length} student{rankings.length === 1 ? "" : "s"} ranked
        </Tag>
        {rankSummary ? (
          <span className="admin-detail-rank-split">
            {rankSummary[1]} first · {rankSummary[2]} second · {rankSummary[3]}{" "}
            third
          </span>
        ) : null}
      </div>

      {actionError ? (
        <p className="admin-users-banner is-error" role="alert">
          {actionError}
        </p>
      ) : null}

      <section className="admin-detail-section" aria-labelledby="assigned-students-heading">
        <header className="admin-detail-section-head">
          <h3 id="assigned-students-heading" className="admin-detail-section-title">
            Assigned students
          </h3>
          <p className="admin-detail-section-desc">
            These students are confirmed onto the project. You can assign from
            the ranked list below or search any student account.
          </p>
        </header>

        <div className="admin-assign-toolbar">
          <div className="admin-assign-search">
            <label className="field-label" htmlFor={searchId}>
              Add a student
            </label>
            <input
              id={searchId}
              type="search"
              className="field-input"
              placeholder={
                project.status === "Closed"
                  ? "Project is closed"
                  : remaining === 0
                    ? "No seats remaining"
                    : "Search by name or email"
              }
              value={searchInput}
              disabled={!seatsOpen}
              onChange={(e) => setSearchInput(e.target.value)}
            />
            {search.length >= 2 ? (
              <ul className="admin-assign-results" role="listbox">
                {searching ? (
                  <li className="admin-assign-empty">Searching…</li>
                ) : searchableCandidates.length === 0 ? (
                  <li className="admin-assign-empty">No matching students.</li>
                ) : (
                  searchableCandidates.map((user) => (
                    <li key={user.id}>
                      <button
                        type="button"
                        className="admin-assign-result"
                        disabled={!canAssign || busyStudentId === user.id}
                        onClick={() => void assign(user.id)}
                      >
                        <span className="admin-users-name">{user.name}</span>
                        <span className="admin-users-meta">{user.email}</span>
                      </button>
                    </li>
                  ))
                )}
              </ul>
            ) : null}
          </div>
          <p className="admin-users-meta admin-assign-seats">
            {remaining} seat{remaining === 1 ? "" : "s"} remaining
          </p>
        </div>

        {assigned.length === 0 ? (
          <p className="admin-users-status">
            No students assigned yet. Assign from the ranked list or search above.
          </p>
        ) : (
          <div className="admin-users-table-wrap">
            <table className="admin-users-table">
              <thead>
                <tr>
                  <th scope="col">Student</th>
                  <th scope="col">Email</th>
                  <th scope="col">Source</th>
                  <th scope="col">Actions</th>
                </tr>
              </thead>
              <tbody>
                {assigned.map((assignment) => (
                  <tr key={assignment.id}>
                    <td>
                      <div className="admin-users-name">{assignment.studentName}</div>
                    </td>
                    <td>
                      {assignment.studentEmail ? (
                        <a
                          className="admin-users-email"
                          href={`mailto:${assignment.studentEmail}`}
                        >
                          {assignment.studentEmail}
                        </a>
                      ) : (
                        "—"
                      )}
                    </td>
                    <td>
                      <span className="admin-users-meta">
                        {assignment.source === "Manual" ? "Manual" : "Algorithm"}
                      </span>
                    </td>
                    <td>
                      <Button
                        type="button"
                        variant="danger"
                        size="sm"
                        disabled={busyStudentId !== null || removing}
                        onClick={() => setRemoveTarget(assignment)}
                      >
                        Remove
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <ProjectDetails project={project} />

      <section className="admin-detail-section" aria-labelledby="ranked-students-heading">
        <header className="admin-detail-section-head">
          <h3 id="ranked-students-heading" className="admin-detail-section-title">
            Students who ranked this project
          </h3>
          <p className="admin-detail-section-desc">
            Preference order is 1st choice through 3rd choice. Faculty rank is
            the project owner&apos;s ranking of the student as a candidate.
          </p>
        </header>

        {rankings.length === 0 ? (
          <p className="admin-users-status">
            No students have ranked this project yet.
          </p>
        ) : (
          <div className="admin-users-table-wrap">
            <table className="admin-users-table">
              <thead>
                <tr>
                  <th scope="col">Student rank</th>
                  <th scope="col">Faculty rank</th>
                  <th scope="col">Student</th>
                  <th scope="col">Email</th>
                  <th scope="col">Ranked on</th>
                  <th scope="col">Actions</th>
                </tr>
              </thead>
              <tbody>
                {rankings.map((ranking) => (
                  <StudentRow
                    key={ranking.rankingId}
                    ranking={ranking}
                    projectId={project.id}
                    canAssign={canAssign}
                    busy={busyStudentId === ranking.studentUserId}
                    onAssign={() => void assign(ranking.studentUserId)}
                  />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <ConfirmModal
        open={removeTarget !== null}
        onClose={() => setRemoveTarget(null)}
        onConfirm={confirmRemove}
        title="Remove this student?"
        description={
          removeTarget
            ? `${removeTarget.studentName} will be released from "${project.title}" and the seat will reopen.`
            : undefined
        }
        confirmLabel="Remove"
        confirmVariant="danger"
        busy={removing}
      />
    </div>
  );
}

function StudentRow({
  ranking,
  projectId,
  canAssign,
  busy,
  onAssign,
}: {
  ranking: ProjectRankingStudentDto;
  projectId: string;
  canAssign: boolean;
  busy: boolean;
  onAssign: () => void;
}) {
  const assignedHere = ranking.assignedProjectId === projectId;
  const assignedElsewhere =
    ranking.assignedProjectId != null && ranking.assignedProjectId !== projectId;

  return (
    <tr>
      <td>
        <span className="admin-rank-badge">{rankLabel(ranking.rank)}</span>
      </td>
      <td>
        {ranking.facultyRank != null ? (
          <span className="admin-rank-badge is-faculty">
            {rankLabel(ranking.facultyRank)}
          </span>
        ) : (
          <span className="admin-users-meta">—</span>
        )}
      </td>
      <td>
        <div className="admin-users-name">{ranking.studentName}</div>
        {ranking.studentUserName ? (
          <div className="admin-users-meta">@{ranking.studentUserName}</div>
        ) : null}
      </td>
      <td>
        {ranking.studentEmail ? (
          <a className="admin-users-email" href={`mailto:${ranking.studentEmail}`}>
            {ranking.studentEmail}
          </a>
        ) : (
          "—"
        )}
      </td>
      <td>{formatRankedAt(ranking.rankedAt)}</td>
      <td>
        {assignedHere ? (
          <span className="admin-users-meta">Assigned</span>
        ) : assignedElsewhere ? (
          <span className="admin-users-meta" title={ranking.assignedProjectTitle ?? undefined}>
            On {ranking.assignedProjectTitle ?? "another project"}
          </span>
        ) : (
          <Button
            type="button"
            variant="primary"
            size="sm"
            disabled={!canAssign || busy}
            onClick={onAssign}
          >
            {busy ? "Assigning…" : "Assign"}
          </Button>
        )}
      </td>
    </tr>
  );
}

function ProjectDetails({ project }: { project: ProjectDto }) {
  return (
    <div className="admin-detail-grid">
      <Field label="Faculty name">{project.facultyName}</Field>
      <Field label="Affiliation">{project.affiliation}</Field>
      <Field label="Email">
        <a className="admin-users-email" href={`mailto:${project.email}`}>
          {project.email}
        </a>
      </Field>
      <Field label="User name">{project.userName || "—"}</Field>
      <Field label="Status">{project.status}</Field>
      <Field label="IRB stage">{project.irbStageLabel}</Field>
      <Field label="Volunteers">
        {project.volunteersFilled} filled of {project.volunteersRequired} required
      </Field>
      <Field label="Posted">{formatProjectDate(project.createdAt)}</Field>
      <Field label="Research areas" wide>
        <ChipList items={project.researchAreas} />
      </Field>
      <Field label="Activity types" wide>
        <ChipList items={project.activityTypes} />
      </Field>
      <Field label="Brief description" wide>
        <p className="admin-detail-prose">{project.briefDescription}</p>
      </Field>
      <Field label="Minimum qualifications" wide>
        {project.minQualifications?.trim() ? (
          <p className="admin-detail-prose">{project.minQualifications}</p>
        ) : (
          "—"
        )}
      </Field>
      <Field label="Additional comments" wide>
        {project.additionalComments?.trim() ? (
          <p className="admin-detail-prose">{project.additionalComments}</p>
        ) : (
          "—"
        )}
      </Field>
    </div>
  );
}

function Field({
  label,
  wide,
  children,
}: {
  label: string;
  wide?: boolean;
  children: ReactNode;
}) {
  return (
    <div className={`admin-detail-field${wide ? " is-wide" : ""}`}>
      <p className="field-label">{label}</p>
      <div className="admin-detail-value">{children}</div>
    </div>
  );
}

function ChipList({ items }: { items: string[] }) {
  if (items.length === 0) return "—";

  return (
    <div className="field-display-chips">
      {items.map((item) => (
        <span key={item} className="multi-select-chip">
          <span className="multi-select-chip-label">{item}</span>
        </span>
      ))}
    </div>
  );
}
