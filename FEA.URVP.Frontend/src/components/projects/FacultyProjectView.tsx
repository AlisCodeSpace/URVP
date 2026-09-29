"use client";

import Link from "next/link";
import { Text } from "@/components/ui/Typography";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { FacultyProjectAlumni } from "@/components/projects/FacultyProjectAlumni";
import { FacultyProjectParticipants } from "@/components/projects/FacultyProjectParticipants";
import { FacultyProjectRankings } from "@/components/projects/FacultyProjectRankings";
import { FacultyProjectReadonly } from "@/components/projects/FacultyProjectReadonly";
import { PageHeader } from "@/components/layout/PageHeader";
import { useState } from "react";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { useApplicationWindow } from "@/hooks/useApplicationWindow";
import { ApiError } from "@/lib/api";
import { FACULTY_PORTAL_ROLES, myProjectsHref } from "@/lib/auth";
import { getProjectRankings } from "@/lib/project-rankings-api";
import { isFacultyCandidateRankingLocked } from "@/lib/project-form";
import { getProject, getProjectAlumni, getProjectParticipants, reactivateProject } from "@/lib/projects-api";
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

  const inactive = project?.status === "Inactive";
  const [reactivateOpen, setReactivateOpen] = useState(false);
  const [reactivating, setReactivating] = useState(false);
  const [reactivateError, setReactivateError] = useState<string | null>(null);
  const rankingsQuery = useCancellableQuery(
    () => getProjectRankings(projectId),
    [projectId, project?.id],
    {
      enabled: Boolean(project) && !inactive,
      fallbackError: "Could not load ranked students.",
    },
  );
  const participantsQuery = useCancellableQuery(
    () => getProjectParticipants(projectId),
    [projectId, project?.id],
    {
      enabled: Boolean(project) && !inactive,
      fallbackError: "Could not load participating students.",
    },
  );
  const alumniQuery = useCancellableQuery(
    () => getProjectAlumni(projectId),
    [projectId, project?.id, project?.status],
    {
      enabled: Boolean(project),
      fallbackError: "Could not load previous students.",
    },
  );
  const phase = useApplicationWindow();
  const rankings = rankingsQuery.data;
  const rankingsError = rankingsQuery.error;
  const participants = participantsQuery.data;
  const participantsError = participantsQuery.error;
  const canReactivate = inactive && !phase.loading && phase.registrationOpen;

  async function handleReactivate() {
    setReactivating(true);
    setReactivateError(null);
    try {
      const next = await reactivateProject(projectId);
      projectQuery.setData({ project: next, message: null });
      setReactivateOpen(false);
      await alumniQuery.reload();
    } catch (err) {
      setReactivateError(
        err instanceof ApiError ? err.message : "Could not reactivate this project.",
      );
    } finally {
      setReactivating(false);
    }
  }

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
              {inactive ? (
                <div className="rounded-md border border-primary/15 bg-primary/[0.03] px-4 py-4">
                  <Text as="p" size="3" className="!text-primary">
                    This project is from {project.semesterName || "a previous cycle"} and is hidden from students.
                    {canReactivate
                      ? ` Reactivate it to list it under ${phase.semesterName ?? "the current cycle"}. The current roster starts empty.`
                      : " You can reactivate it when the registration window opens."}
                  </Text>
                  {reactivateError ? (
                    <Text as="p" size="2" mt="3" role="alert" className="!text-red-800">
                      {reactivateError}
                    </Text>
                  ) : null}
                  {canReactivate ? (
                    <div className="mt-4">
                      <Button
                        type="button"
                        variant="secondary"
                        size="sm"
                        onClick={() => {
                          setReactivateError(null);
                          setReactivateOpen(true);
                        }}
                      >
                        Reactivate for this cycle
                      </Button>
                    </div>
                  ) : null}
                </div>
              ) : (
                <>
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
                    applicationsClosed={!phase.loading && !phase.isOpen}
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
                </>
              )}
              <FacultyProjectAlumni
                userId={userId}
                projectId={projectId}
                projectStatus={project.status}
                alumni={alumniQuery.data}
                loading={alumniQuery.data == null && alumniQuery.error == null}
                error={alumniQuery.error}
              />
            </FacultyProjectReadonly>
          )}
        </section>
        <ConfirmModal
          open={reactivateOpen}
          onClose={() => {
            if (!reactivating) setReactivateOpen(false);
          }}
          onConfirm={handleReactivate}
          title="Reactivate project?"
          description={
            project
              ? `Reactivate “${project.title}” for ${phase.semesterName ?? "the current cycle"}? Students from ${project.semesterName || "the previous cycle"} stay under Previous students, and the live roster starts empty.`
              : "Reactivate this project for the current cycle?"
          }
          confirmLabel="Reactivate"
          busyLabel="Reactivating…"
          busy={reactivating}
        />
      </main>
    </RequireAuth>
  );
}
