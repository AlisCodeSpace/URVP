"use client";

import Link from "next/link";
import { Text } from "@/components/ui/Typography";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { FacultyProjectParticipants } from "@/components/projects/FacultyProjectParticipants";
import { FacultyProjectRankings } from "@/components/projects/FacultyProjectRankings";
import { FacultyProjectReadonly } from "@/components/projects/FacultyProjectReadonly";
import { PageHeader } from "@/components/layout/PageHeader";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { ApiError } from "@/lib/api";
import { FACULTY_PORTAL_ROLES, myProjectsHref } from "@/lib/auth";
import { getProjectRankings } from "@/lib/project-rankings-api";
import { isFacultyCandidateRankingLocked } from "@/lib/project-form";
import { getProject, getProjectParticipants } from "@/lib/projects-api";
import { ProjectDetailSkeleton } from "@/components/ui/SectionSkeletons";

export function FacultyProjectView({
  userId,
  projectId,
}: {
  userId: string;
  projectId: string;
}) {
  const projectQuery = useCancellableQuery(
    async () => {
      try {
        const next = await getProject(projectId);
        if (next.createdByUserId.toLowerCase() !== userId.toLowerCase()) {
          return {
            project: null,
            message: "You can only view your own projects here.",
          };
        }
        return { project: next, message: null };
      } catch (err) {
        return {
          project: null,
          message:
            err instanceof ApiError ? err.message : "Could not load project.",
        };
      }
    },
    [projectId, userId],
  );
  const project = projectQuery.data?.project ?? null;
  const error = projectQuery.data?.message ?? null;

  const rankingsQuery = useCancellableQuery(
    () => getProjectRankings(projectId),
    [projectId, project?.id],
    {
      enabled: Boolean(project),
      fallbackError: "Could not load ranked students.",
    },
  );
  const participantsQuery = useCancellableQuery(
    () => getProjectParticipants(projectId),
    [projectId, project?.id],
    {
      enabled: Boolean(project),
      fallbackError: "Could not load participating students.",
    },
  );
  const rankings = rankingsQuery.data;
  const rankingsError = rankingsQuery.error;
  const participants = participantsQuery.data;
  const participantsError = participantsQuery.error;

  return (
    <RequireAuth userId={userId} roles={FACULTY_PORTAL_ROLES}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="Faculty portal"
          title="View project"
          description="Review the details of your posted research opportunity."
        >
          <Link
            href={myProjectsHref(userId)}
            className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
          >
            <span aria-hidden>←</span>
            Back to my projects
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
          ) : project == null ? (
            <ProjectDetailSkeleton />
          ) : (
            <FacultyProjectReadonly userId={userId} project={project}>
              <FacultyProjectParticipants
                userId={userId}
                projectId={projectId}
                volunteersFilled={project.volunteersFilled}
                participants={participants}
                loading={participants == null && participantsError == null}
                error={participantsError}
              />
              <FacultyProjectRankings
                userId={userId}
                projectId={projectId}
                volunteersRequired={project.volunteersRequired}
                rankings={rankings}
                loading={rankings == null && rankingsError == null}
                error={rankingsError}
                locked={isFacultyCandidateRankingLocked(project)}
                onRankingsChanged={() => {
                  void getProjectRankings(projectId)
                    .then((next) => {
                      rankingsQuery.setData(next);
                      rankingsQuery.setError(null);
                    })
                    .catch((err: unknown) => {
                      rankingsQuery.setError(
                        err instanceof ApiError
                          ? err.message
                          : "Could not refresh ranked students.",
                      );
                    });
                }}
              />
            </FacultyProjectReadonly>
          )}
        </section>
      </main>
    </RequireAuth>
  );
}
