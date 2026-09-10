"use client";

import { useCallback, useEffect, useId, useMemo, useState, type ReactNode } from "react";
import Link from "next/link";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { BackLink } from "@/components/ui/BackLink";
import { Button } from "@/components/ui/Button";
import { DeleteIconButton } from "@/components/ui/DeleteIconButton";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
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
import {
  formatProjectDate,
  projectStatusClass,
} from "@/lib/project-form";
import {
  formatRankedAt,
  optionalRankLabel,
  rankLabel,
} from "@/lib/project-rankings-api";
import { adminStudentProfileHref } from "@/lib/auth";
import type { ProjectDto } from "@/lib/projects-api";
import { getActiveSemester, type SemesterDto } from "@/lib/semesters-api";

const RANK_GROUPS = [
  { rank: 1, label: "1st choice" },
  { rank: 2, label: "2nd choice" },
  { rank: 3, label: "3rd choice" },
] as const;

function assignmentRank(ranking: ProjectRankingStudentDto, projectId: string) {
  if (ranking.assignedProjectId === projectId) return 2;
  if (ranking.assignedProjectId) return 1;
  return 0;
}

function groupedRankings(
  rankings: ProjectRankingStudentDto[],
  projectId: string,
) {
  const byRank = new Map<number, ProjectRankingStudentDto[]>();
  for (const ranking of rankings) {
    const key =
      ranking.rank === 1 || ranking.rank === 2 || ranking.rank === 3
        ? ranking.rank
        : 0;
    const list = byRank.get(key) ?? [];
    list.push(ranking);
    byRank.set(key, list);
  }

  const sortGroup = (items: ProjectRankingStudentDto[]) =>
    [...items].sort(
      (a, b) =>
        assignmentRank(a, projectId) - assignmentRank(b, projectId) ||
        a.studentName.localeCompare(b.studentName),
    );

  const groups: {
    rank: number;
    label: string;
    items: ProjectRankingStudentDto[];
  }[] = RANK_GROUPS.map(({ rank, label }) => ({
    rank,
    label,
    items: sortGroup(byRank.get(rank) ?? []),
  })).filter((group) => group.items.length > 0);

  const other = byRank.get(0);
  if (other?.length) {
    groups.push({ rank: 0, label: "Other ranks", items: sortGroup(other) });
  }

  return groups;
}

function rankingMatchesSearch(ranking: ProjectRankingStudentDto, query: string) {
  if (!query) return true;
  const q = query.toLowerCase();
  return (
    ranking.studentName.toLowerCase().includes(q) ||
    ranking.studentEmail.toLowerCase().includes(q) ||
    (ranking.studentUserName?.toLowerCase().includes(q) ?? false)
  );
}

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
  const [activeSemester, setActiveSemester] = useState<SemesterDto | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [project, semester] = await Promise.all([
        getAdminProject(projectId),
        getActiveSemester(),
      ]);
      setData(project);
      setActiveSemester(semester);
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
        <AdminPageHeader title="Project" description="Loading project details." backHref="/admin/projects" backLabel="Back to projects" />
        <AdminFormSkeleton fields={8} />
        <RankingsListSkeleton count={4} />
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="admin-panel admin-panel--wide">
        <div className="admin-detail-back">
          <BackLink href="/admin/projects">Back to projects</BackLink>
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
  const fill =
    project.volunteersRequired > 0
      ? Math.round((assigned.length / project.volunteersRequired) * 100)
      : 0;
  const seatsOpen = project.status !== "Closed" && remaining > 0;
  const applicationsOpen = Boolean(activeSemester?.isApplicationWindowOpen);
  const canAssign =
    seatsOpen &&
    busyStudentId === null &&
    Boolean(activeSemester) &&
    !applicationsOpen;
  const rankGroups = groupedRankings(rankings, project.id);
  const rankingQuery = searchInput.trim();
  const filteredRankGroups = rankingQuery
    ? rankGroups
        .map((group) => ({
          ...group,
          items: group.items.filter((ranking) =>
            rankingMatchesSearch(ranking, rankingQuery),
          ),
        }))
        .filter((group) => group.items.length > 0)
    : rankGroups;

  return (
    <div className="admin-panel admin-panel--wide">
      <div className="admin-detail-back">
        <BackLink href="/admin/projects">Back to projects</BackLink>
      </div>

      <header className="admin-project-hero">
        <div className="admin-project-hero-copy">
          <div className="admin-page-title-row">
            <h2 className="admin-page-title">{project.title}</h2>
            <span
              className={`admin-value-status ${projectStatusClass(project.status)}`}
            >
              {project.status}
            </span>
          </div>
          <p className="admin-page-desc">
            {project.facultyName} · {project.affiliation}
          </p>
          <p className="admin-project-hero-meta">
            <a className="admin-users-email" href={`mailto:${project.email}`}>
              {project.email}
            </a>
            {project.userName ? (
              <>
                <span aria-hidden> · </span>
                <span>{project.userName}</span>
              </>
            ) : null}
            <span aria-hidden> · </span>
            Posted {formatProjectDate(project.createdAt)}
          </p>
        </div>
      </header>

      <section aria-label="Project snapshot" className="admin-project-stats">
        <div className="admin-kpi">
          <p className="admin-kpi-label">Seats</p>
          <p className="admin-kpi-value">
            {assigned.length}/{project.volunteersRequired}
          </p>
          <p className="admin-kpi-hint">Confirmed onto this listing</p>
          {project.volunteersRequired > 0 ? (
            <div className="admin-meter" style={{ marginTop: "0.65rem" }}>
              <div
                className="admin-meter-track"
                role="progressbar"
                aria-valuenow={fill}
                aria-valuemin={0}
                aria-valuemax={100}
                aria-label="Seat fill"
              >
                <span className="admin-meter-fill" style={{ width: `${fill}%` }} />
              </div>
            </div>
          ) : null}
        </div>
        <div className="admin-kpi">
          <p className="admin-kpi-label">Remaining</p>
          <p className="admin-kpi-value">{remaining}</p>
          <p className="admin-kpi-hint">
            {project.status === "Closed"
              ? "Listing is closed"
              : remaining === 0
                ? "At capacity"
                : applicationsOpen
                  ? "Assignments locked until applications close"
                  : "Open for assignment"}
          </p>
        </div>
        <div className="admin-kpi">
          <p className="admin-kpi-label">Student ranks</p>
          <p className="admin-kpi-value">{rankings.length}</p>
          <p className="admin-kpi-hint">Students who ranked this project</p>
        </div>
        <div className="admin-kpi">
          <p className="admin-kpi-label">Choice split</p>
          <p className="admin-kpi-value">
            {rankSummary
              ? `${rankSummary[1]} · ${rankSummary[2]} · ${rankSummary[3]}`
              : "—"}
          </p>
          <p className="admin-kpi-hint">1st · 2nd · 3rd choice</p>
        </div>
      </section>

      {actionError ? (
        <p className="admin-users-banner is-error" role="alert">
          {actionError}
        </p>
      ) : null}

      <ListingPanel project={project} />

      <section
        className="admin-widget admin-project-copy"
        aria-labelledby="briefing-heading"
      >
        <header className="admin-widget-head">
          <h3 id="briefing-heading" className="admin-widget-title">
            Briefing
          </h3>
        </header>
        <ListingBlock label="Brief description">
          <p className="admin-detail-prose">{project.briefDescription}</p>
        </ListingBlock>
        <ListingBlock label="Minimum qualifications">
          {project.minQualifications?.trim() ? (
            <p className="admin-detail-prose">{project.minQualifications}</p>
          ) : (
            "—"
          )}
        </ListingBlock>
        <ListingBlock label="Additional comments">
          {project.additionalComments?.trim() ? (
            <p className="admin-detail-prose">{project.additionalComments}</p>
          ) : (
            "—"
          )}
        </ListingBlock>
      </section>

      {applicationsOpen ? (
        <p className="admin-users-banner" role="status">
          Close the student application window before assigning students.
        </p>
      ) : !activeSemester ? (
        <p className="admin-users-banner" role="status">
          No active URVP cycle. Start a cycle before assigning students.
        </p>
      ) : null}

      <div className="admin-project-main">
        <section
          className="admin-widget admin-people admin-assigned"
          aria-labelledby="assigned-students-heading"
        >
          <header className="admin-widget-head">
            <h3 id="assigned-students-heading" className="admin-widget-title">
              Assigned students
            </h3>
            <p className="admin-widget-sub">
              Confirmed seats. Assign from ranked applicants.
            </p>
          </header>

          {assigned.length === 0 ? (
            <p className="admin-users-status">No students assigned yet.</p>
          ) : (
            <ul className="admin-person-list">
              {assigned.map((assignment) => (
                <AssignedCard
                  key={assignment.id}
                  assignment={assignment}
                  projectId={project.id}
                  disabled={busyStudentId !== null || removing}
                  onRemove={() => setRemoveTarget(assignment)}
                />
              ))}
            </ul>
          )}
        </section>

        <section
          className="admin-widget admin-people admin-rankings"
          aria-labelledby="ranked-students-heading"
        >
          <header className="admin-widget-head">
            <h3 id="ranked-students-heading" className="admin-widget-title">
              Rankings
            </h3>
            <p className="admin-widget-sub">
              Grouped by the student&apos;s preference. Faculty rank is the
              project owner&apos;s ranking of that candidate.
            </p>
          </header>

          <div className="admin-assign-toolbar">
            <div className="admin-assign-search">
              <label className="field-label" htmlFor={searchId}>
                Search rankings
              </label>
              <input
                id={searchId}
                type="search"
                className="field-input"
                placeholder="Search by name or email"
                value={searchInput}
                disabled={rankings.length === 0}
                onChange={(e) => setSearchInput(e.target.value)}
              />
            </div>
          </div>

          {rankings.length === 0 ? (
            <p className="admin-users-status">
              No students have ranked this project yet.
            </p>
          ) : filteredRankGroups.length === 0 ? (
            <p className="admin-users-status">No matching students.</p>
          ) : (
            filteredRankGroups.map((group) => (
              <div key={group.rank} className="admin-rank-group">
                <h4 className="admin-rank-group-title">
                  {group.label}
                  <span> · {group.items.length}</span>
                </h4>
                <ul className="admin-person-list">
                  {group.items.map((ranking) => (
                    <RankedCard
                      key={ranking.rankingId}
                      ranking={ranking}
                      projectId={project.id}
                      canAssign={canAssign}
                      busy={busyStudentId === ranking.studentUserId}
                      onAssign={() => void assign(ranking.studentUserId)}
                    />
                  ))}
                </ul>
              </div>
            ))
          )}
        </section>
      </div>

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

function AssignedCard({
  assignment,
  projectId,
  disabled,
  onRemove,
}: {
  assignment: PlacementDto;
  projectId: string;
  disabled: boolean;
  onRemove: () => void;
}) {
  const studentChoice = optionalRankLabel(assignment.studentRank);
  const facultyChoice = optionalRankLabel(assignment.facultyRank);

  return (
    <li className="admin-person-card bg-slate-50">
      <div className="admin-person-card-main">
        <div className="admin-person-badges">
          <span className="admin-rank-badge">
            {assignment.source === "Manual" ? "Manual" : "Algorithm"}
          </span>
          {studentChoice ? (
            <span className="admin-rank-badge">{studentChoice}</span>
          ) : null}
          {facultyChoice ? (
            <span className="admin-rank-badge is-faculty">
              Faculty {facultyChoice}
            </span>
          ) : null}
        </div>
        <p className="admin-person-name">{assignment.studentName}</p>
      </div>
      <div className="admin-person-card-actions">
        <DeleteIconButton
          label="Remove student"
          disabled={disabled}
          onClick={onRemove}
        />
      </div>
      <div className="admin-person-meta-row">
        <p className="admin-person-meta">
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
          <span aria-hidden> · </span>
          Assigned {formatRankedAt(assignment.createdAt)}
        </p>
        <ProfileArrow
          href={adminStudentProfileHref(projectId, assignment.studentUserId)}
        />
      </div>
    </li>
  );
}

function RankedCard({
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
    <li className="admin-person-card bg-slate-50">
      <div className="admin-person-card-main">
        <div className="admin-person-badges">
          <span className="admin-rank-badge">{rankLabel(ranking.rank)}</span>
          {ranking.facultyRank != null ? (
            <span className="admin-rank-badge is-faculty">
              Faculty {rankLabel(ranking.facultyRank)}
            </span>
          ) : (
            <span className="admin-rank-badge is-muted">Not ranked by faculty</span>
          )}
        </div>
        <p className="admin-person-name">{ranking.studentName}</p>
        {assignedElsewhere ? (
          <p className="admin-person-meta">
            On {ranking.assignedProjectTitle ?? "another project"}
          </p>
        ) : null}
      </div>
      <div className="admin-person-card-actions">
        {assignedHere ? (
          <span className="admin-rank-badge is-assigned">Assigned here</span>
        ) : assignedElsewhere ? (
          <span className="admin-rank-badge is-muted">On another project</span>
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
      </div>
      <div className="admin-person-meta-row">
        <p className="admin-person-meta">
          {ranking.studentEmail ? (
            <a className="admin-users-email" href={`mailto:${ranking.studentEmail}`}>
              {ranking.studentEmail}
            </a>
          ) : (
            "—"
          )}
          <span aria-hidden> · </span>
          Ranked {formatRankedAt(ranking.rankedAt)}
        </p>
        <ProfileArrow
          href={adminStudentProfileHref(projectId, ranking.studentUserId)}
        />
      </div>
    </li>
  );
}

function ProfileArrow({ href }: { href: string }) {
  return (
    <Link href={href} className="admin-person-profile">
      View Profile
      <span aria-hidden>→</span>
    </Link>
  );
}

function ListingPanel({ project }: { project: ProjectDto }) {
  return (
    <section
      className="admin-widget admin-project-listing"
      aria-labelledby="listing-heading"
    >
      <header className="admin-widget-head">
        <h3 id="listing-heading" className="admin-widget-title">
          Listing
        </h3>
        <p className="admin-widget-sub">Details students see on this project.</p>
      </header>

      <dl className="admin-listing-facts">
        <div>
          <dt>IRB stage</dt>
          <dd>{project.irbStageLabel}</dd>
        </div>
        <div>
          <dt>Posted</dt>
          <dd>{formatProjectDate(project.createdAt)}</dd>
        </div>
      </dl>

      <div className="admin-listing-chips">
        <ListingBlock label="Research area">
          <ChipList items={project.researchAreas} />
        </ListingBlock>
        <ListingBlock label="Research activity type">
          <ChipList items={project.activityTypes} />
        </ListingBlock>
      </div>
    </section>
  );
}

function ListingBlock({
  label,
  children,
}: {
  label: string;
  children: ReactNode;
}) {
  return (
    <div className="admin-listing-block">
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
