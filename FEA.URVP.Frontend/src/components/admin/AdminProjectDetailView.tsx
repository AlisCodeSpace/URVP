"use client";

import { useCallback, useEffect, useMemo, useState, type ReactNode } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { Tag } from "@/components/ui/Tag";
import { AdminFormSkeleton, RankingsListSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import {
  getAdminProject,
  type AdminProjectDetailDto,
  type ProjectRankingStudentDto,
} from "@/lib/admin-projects-api";
import { formatProjectDate } from "@/lib/project-form";
import { formatRankedAt, rankLabel } from "@/lib/project-rankings-api";
import type { ProjectDto } from "@/lib/projects-api";

export function AdminProjectDetailView({ projectId }: { projectId: string }) {
  const [data, setData] = useState<AdminProjectDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

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

  const { project, rankings } = data;

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
          {rankings.length} student{rankings.length === 1 ? "" : "s"} ranked
        </Tag>
        {rankSummary ? (
          <span className="admin-detail-rank-split">
            {rankSummary[1]} first · {rankSummary[2]} second · {rankSummary[3]}{" "}
            third
          </span>
        ) : null}
      </div>

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
                </tr>
              </thead>
              <tbody>
                {rankings.map((ranking) => (
                  <StudentRow key={ranking.rankingId} ranking={ranking} />
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  );
}

function StudentRow({ ranking }: { ranking: ProjectRankingStudentDto }) {
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
