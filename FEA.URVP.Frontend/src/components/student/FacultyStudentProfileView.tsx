"use client";

import { useState } from "react";
import Link from "next/link";
import { Heading, Text } from "@/components/ui/Typography";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PageHeader } from "@/components/layout/PageHeader";
import { FacultyCandidateRankModal } from "@/components/student/FacultyCandidateRankModal";
import { StudentProfileReadonly } from "@/components/student/StudentProfileReadonly";
import { Button } from "@/components/ui/Button";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { ApiError } from "@/lib/api";
import { FACULTY_PORTAL_ROLES, viewProjectHref } from "@/lib/auth";
import {
  getStudentProfile,
  toStudentProfileValues,
} from "@/lib/student-profile-api";
import { ProfileFormSkeleton } from "@/components/ui/SectionSkeletons";
import { getProjectRankings, rankLabel } from "@/lib/project-rankings-api";
import { isFacultyCandidateRankingLocked } from "@/lib/project-form";
import { getProject } from "@/lib/projects-api";

export function FacultyStudentProfileView({
  userId,
  projectId,
  studentUserId,
}: {
  userId: string;
  projectId: string;
  studentUserId: string;
}) {
  const [rankOpen, setRankOpen] = useState(false);
  const profileQuery = useCancellableQuery(
    async () => {
      const dto = await getStudentProfile(studentUserId);
      return { exists: dto.exists, values: toStudentProfileValues(dto) };
    },
    [studentUserId],
    { fallbackError: "Could not load this student profile." },
  );
  const projectQuery = useCancellableQuery(
    async () => {
      const next = await getProject(projectId);
      if (next.createdByUserId.toLowerCase() !== userId.toLowerCase()) {
        throw new Error("You can only rank candidates on your own projects.");
      }
      return next;
    },
    [projectId, userId],
    { fallbackError: "Could not load this project." },
  );
  const project = projectQuery.data;
  const rankingsQuery = useCancellableQuery(
    () => getProjectRankings(projectId),
    [projectId, project?.id],
    {
      enabled: Boolean(project),
      fallbackError: "Could not load candidate rankings.",
    },
  );
  const values = profileQuery.data?.values ?? null;
  const exists = profileQuery.data?.exists ?? true;
  const error = profileQuery.error;
  const rankings = rankingsQuery.data;
  const rankingsError = projectQuery.error ?? rankingsQuery.error;

  const displayName = values
    ? `${values.firstName} ${values.lastName}`.trim() || "Student profile"
    : "Student profile";
  const current = rankings?.find((r) => r.studentUserId === studentUserId);
  const facultyRank = current?.facultyRank ?? null;
  const canRank = Boolean(
    project && current && !isFacultyCandidateRankingLocked(project),
  );
  const seats = project?.volunteersRequired ?? 0;
  const firstChoiceUsed =
    rankings?.filter((r) => r.facultyRank === 1).length ?? 0;

  return (
    <RequireAuth userId={userId} roles={FACULTY_PORTAL_ROLES}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="Faculty portal"
          title={error ? "Student profile" : displayName}
          description="Review this student's qualifications and rank them as a candidate for your project."
        >
          <Link
            href={viewProjectHref(userId, projectId)}
            className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
          >
            <span aria-hidden>←</span>
            Back to project
          </Link>
        </PageHeader>

        <section className="site-container py-14 sm:py-16">
          {error ? (
            <Text
              as="p"
              size="3"
              role="alert"
              className="rounded-md bg-red-50 px-3 py-2 !text-red-800"
            >
              {error}
            </Text>
          ) : values == null ? (
            <ProfileFormSkeleton />
          ) : (
            <div className="flex flex-col gap-8">
              {rankingsError ? (
                <Text
                  as="p"
                  size="3"
                  role="alert"
                  className="rounded-md bg-red-50 px-3 py-2 !text-red-800"
                >
                  {rankingsError}
                </Text>
              ) : project ? (
                <section className="rounded-[var(--radius-lg)] border border-primary/12 bg-surface p-5 sm:p-7">
                  <Heading
                    as="h2"
                    size="5"
                    weight="medium"
                    className="!font-[family-name:var(--font-display)] !text-primary"
                  >
                    Rank this candidate
                  </Heading>
                  <Text
                    as="p"
                    size="2"
                    mt="2"
                    className="!leading-relaxed !text-muted"
                  >
                    Choose 1st, 2nd, or 3rd choice. 1st choice is limited to{" "}
                    {seats} seat{seats === 1 ? "" : "s"}
                    {rankings
                      ? ` (${firstChoiceUsed} of ${seats} used)`
                      : ""}
                    .
                    {project && isFacultyCandidateRankingLocked(project)
                      ? " Rankings are locked after matching."
                      : ""}
                  </Text>
                  <Text
                    as="p"
                    size="2"
                    mt="2"
                    weight="medium"
                    className="!text-primary"
                  >
                    {facultyRank != null
                      ? `Currently ${rankLabel(facultyRank)}`
                      : "Not yet ranked"}
                  </Text>
                  <div className="mt-5">
                    {project && isFacultyCandidateRankingLocked(project) ? null : (
                      <Button
                        type="button"
                        variant="primary"
                        size="md"
                        disabled={!canRank}
                        onClick={() => setRankOpen(true)}
                      >
                        {facultyRank != null ? "Adjust rank" : "Rank candidate"}
                      </Button>
                    )}
                  </div>
                </section>
              ) : null}
              {!exists ? (
                <Text
                  as="p"
                  size="3"
                  className="rounded-md border border-primary/12 bg-surface px-4 py-3 !text-muted"
                >
                  This student has not completed their profile yet. Name and
                  email are shown from their account.
                </Text>
              ) : null}
              <StudentProfileReadonly values={values} />
            </div>
          )}
        </section>

        {project ? (
          <FacultyCandidateRankModal
            open={rankOpen}
            onClose={() => setRankOpen(false)}
            projectId={projectId}
            studentUserId={studentUserId}
            studentName={displayName}
            volunteersRequired={project.volunteersRequired}
            onChanged={() => {
              void getProjectRankings(projectId)
                .then((next) => {
                  rankingsQuery.setData(next);
                  rankingsQuery.setError(null);
                })
                .catch((err: unknown) => {
                  rankingsQuery.setError(
                    err instanceof ApiError
                      ? err.message
                      : "Could not refresh candidate rankings.",
                  );
                });
            }}
          />
        ) : null}
      </main>
    </RequireAuth>
  );
}
