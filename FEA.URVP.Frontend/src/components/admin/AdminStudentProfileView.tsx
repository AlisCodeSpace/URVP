"use client";

import type { ReactNode } from "react";
import Link from "next/link";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { StudentProfileReadonly } from "@/components/student/StudentProfileReadonly";
import { BackLink } from "@/components/ui/BackLink";
import { ProfileFormSkeleton } from "@/components/ui/SectionSkeletons";
import { Heading, Text } from "@/components/ui/Typography";
import { adminProjectHref } from "@/lib/auth";
import type { PlacementDto } from "@/lib/matching-api";
import {
  formatRankedAt,
  optionalRankLabel,
  rankLabel,
  type ProjectRankingDto,
} from "@/lib/project-rankings-api";
import {
  getStudentProfile,
  toStudentProfileValues,
} from "@/lib/student-profile-api";
import {
  getStudentProjectActivity,
  type StudentProjectActivityDto,
} from "@/lib/users-api";

export function AdminStudentProfileView({
  projectId,
  studentUserId,
}: {
  projectId?: string;
  studentUserId: string;
}) {
  const profileQuery = useCancellableQuery(
    async () => {
      const dto = await getStudentProfile(studentUserId);
      return { exists: dto.exists, values: toStudentProfileValues(dto) };
    },
    [studentUserId],
    { fallbackError: "Could not load this student profile." },
  );
  const activityQuery = useCancellableQuery(
    () => getStudentProjectActivity(studentUserId),
    [studentUserId],
    { fallbackError: "Could not load this student's projects." },
  );
  const values = profileQuery.data?.values ?? null;
  const exists = profileQuery.data?.exists ?? true;
  const error = profileQuery.error;

  const displayName = values
    ? `${values.firstName} ${values.lastName}`.trim() || "Student profile"
    : "Student profile";
  const backHref = projectId ? adminProjectHref(projectId) : "/admin/users";

  return (
    <div className="admin-panel admin-panel--wide">
      <div className="admin-detail-back">
        <BackLink href={backHref}>
          {projectId ? "Back to project" : "Back to users"}
        </BackLink>
      </div>

      {error ? (
        <>
          <AdminPageHeader
            title="Student profile"
            description="This profile could not be loaded."
          />
          <p className="admin-users-banner is-error" role="alert">
            {error}
          </p>
        </>
      ) : values == null ? (
        <>
          <AdminPageHeader
            title="Student profile"
            description="Loading student details."
          />
          <ProfileFormSkeleton />
        </>
      ) : (
        <>
          <AdminPageHeader
            title={displayName}
            description="Qualifications, research interests, and documents on this student's URVP profile, plus the projects they are assigned to and have ranked."
          />
          {!exists ? (
            <p className="admin-users-banner" role="status">
              This student has not completed their profile yet. Name and email
              are shown from their account.
            </p>
          ) : null}
          <div className="space-y-6">
            <StudentProjects
              activity={activityQuery.data}
              loading={activityQuery.loading && activityQuery.data == null}
              error={activityQuery.error}
            />
            <StudentProfileReadonly values={values} />
          </div>
        </>
      )}
    </div>
  );
}

function StudentProjects({
  activity,
  loading,
  error,
}: {
  activity: StudentProjectActivityDto | null;
  loading: boolean;
  error: string | null;
}) {
  if (error) {
    return (
      <p className="admin-users-banner is-error" role="alert">
        {error}
      </p>
    );
  }

  if (loading || activity == null) {
    return (
      <>
        <ProjectSection title="Assigned projects">
          <Text as="p" size="2" className="!text-muted">
            Loading assigned projects.
          </Text>
        </ProjectSection>
        <ProjectSection title="Ranked projects">
          <Text as="p" size="2" className="!text-muted">
            Loading ranked projects.
          </Text>
        </ProjectSection>
      </>
    );
  }

  return (
    <>
      <ProjectSection title="Assigned projects">
        {activity.assignments.length === 0 ? (
          <Text as="p" size="2" className="!text-muted">
            This student is not assigned to a project.
          </Text>
        ) : (
          <ul className="admin-profile-projects">
            {activity.assignments.map((assignment) => (
              <AssignedProject key={assignment.id} assignment={assignment} />
            ))}
          </ul>
        )}
      </ProjectSection>

      <ProjectSection title="Ranked projects">
        {activity.rankings.length === 0 ? (
          <Text as="p" size="2" className="!text-muted">
            This student has not ranked a project.
          </Text>
        ) : (
          <ul className="admin-profile-projects">
            {activity.rankings.map((ranking) => (
              <RankedProject key={ranking.id} ranking={ranking} />
            ))}
          </ul>
        )}
      </ProjectSection>
    </>
  );
}

function ProjectSection({
  title,
  children,
}: {
  title: string;
  children: ReactNode;
}) {
  return (
    <section className="rounded-[var(--radius-lg)] border border-primary/12 bg-surface p-5 sm:p-7">
      <Heading
        as="h2"
        size="5"
        weight="medium"
        className="!font-[family-name:var(--font-display)] !text-primary"
      >
        {title}
      </Heading>
      <div className="mt-5">{children}</div>
    </section>
  );
}

function AssignedProject({ assignment }: { assignment: PlacementDto }) {
  const studentChoice = optionalRankLabel(assignment.studentRank);
  const facultyChoice = optionalRankLabel(assignment.facultyRank);

  return (
    <li className="admin-profile-project">
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
      <Link
        href={adminProjectHref(assignment.projectId)}
        className="admin-profile-project-title"
      >
        {assignment.projectTitle}
      </Link>
      <p className="admin-person-meta">
        {assignment.facultyName || "Faculty"}
        <span aria-hidden> · </span>
        Assigned {formatRankedAt(assignment.createdAt)}
      </p>
    </li>
  );
}

function RankedProject({ ranking }: { ranking: ProjectRankingDto }) {
  return (
    <li className="admin-profile-project">
      <div className="admin-person-badges">
        <span className="admin-rank-badge">{rankLabel(ranking.rank)}</span>
        {ranking.isMatched ? (
          <span className="admin-rank-badge is-assigned">Assigned</span>
        ) : null}
      </div>
      <Link
        href={adminProjectHref(ranking.projectId)}
        className="admin-profile-project-title"
      >
        {ranking.projectTitle}
      </Link>
      <p className="admin-person-meta">
        {ranking.facultyName || "Faculty"}
        {ranking.facultyAffiliation ? ` · ${ranking.facultyAffiliation}` : ""}
        <span aria-hidden> · </span>
        Ranked {formatRankedAt(ranking.rankedAt)}
      </p>
    </li>
  );
}
