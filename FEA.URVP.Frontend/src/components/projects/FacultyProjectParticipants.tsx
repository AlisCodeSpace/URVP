"use client";

import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { RankingsListSkeleton } from "@/components/ui/SectionSkeletons";
import { viewRankedStudentHref } from "@/lib/auth";
import { rankLabel } from "@/lib/project-rankings-api";
import type { ProjectParticipantDto } from "@/lib/projects-api";

export function FacultyProjectParticipants({
  userId,
  projectId,
  volunteersFilled,
  participants,
  loading,
  error,
}: {
  userId: string;
  projectId: string;
  volunteersFilled: number;
  participants: ProjectParticipantDto[] | null;
  loading: boolean;
  error: string | null;
}) {
  const matchingDone = volunteersFilled > 0 || (participants?.length ?? 0) > 0;

  if (!matchingDone && !error) {
    return null;
  }

  if (!error && participants != null && participants.length === 0) {
    return null;
  }

  return (
    <section
      className="form-section"
      aria-labelledby="participating-students-heading"
    >
      <Heading
        as="h2"
        size="5"
        weight="medium"
        id="participating-students-heading"
        className="!font-[family-name:var(--font-display)] !text-primary"
      >
        Participating students
      </Heading>

      {error ? (
        <Text as="p" size="3" mt="5" role="alert" className="!text-red-800">
          {error}
        </Text>
      ) : loading || participants == null ? (
        <RankingsListSkeleton />
      ) : (
        <>
          <Text as="p" size="2" mt="2" className="!text-muted">
            Students confirmed onto this project after matching.
          </Text>
          <ul className="ranked-students-list">
            {participants.map((participant) => (
              <li
                key={participant.studentUserId}
                className="ranked-students-row"
              >
                <div className="ranked-students-badges">
                  <span className="rank-badge is-matched">Matched</span>
                  <span className="rank-badge">
                    Student {rankLabel(participant.studentRank)}
                  </span>
                  <span className="rank-badge is-faculty">
                    Your {rankLabel(participant.facultyRank)}
                  </span>
                </div>
                <p className="ranked-students-name">{participant.studentName}</p>
                <p className="ranked-students-meta">
                  {participant.studentEmail || "—"}
                </p>
                <div className="ranked-students-actions">
                  <Button
                    href={viewRankedStudentHref(
                      userId,
                      projectId,
                      participant.studentUserId,
                    )}
                    variant="outline-secondary"
                    size="sm"
                  >
                    View profile
                  </Button>
                </div>
              </li>
            ))}
          </ul>
        </>
      )}
    </section>
  );
}
