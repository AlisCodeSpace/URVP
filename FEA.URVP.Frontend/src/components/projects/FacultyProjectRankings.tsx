"use client";

import { useState } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { FacultyCandidateRankModal } from "@/components/student/FacultyCandidateRankModal";
import { Button } from "@/components/ui/Button";
import { viewRankedStudentHref } from "@/lib/auth";
import {
  formatRankedAt,
  rankLabel,
  type ProjectRankingStudentDto,
} from "@/lib/project-rankings-api";
import { RankingsListSkeleton } from "@/components/ui/SectionSkeletons";

export function FacultyProjectRankings({
  userId,
  projectId,
  volunteersRequired,
  rankings,
  loading,
  error,
  onRankingsChanged,
}: {
  userId: string;
  projectId: string;
  volunteersRequired: number;
  rankings: ProjectRankingStudentDto[] | null;
  loading: boolean;
  error: string | null;
  onRankingsChanged?: () => void;
}) {
  const [rankTarget, setRankTarget] = useState<ProjectRankingStudentDto | null>(
    null,
  );
  const seats = Math.max(0, volunteersRequired);

  return (
    <section className="form-section" aria-labelledby="ranked-students-heading">
      <Heading
        as="h2"
        size="5"
        weight="medium"
        id="ranked-students-heading"
        className="!font-[family-name:var(--font-display)] !text-primary"
      >
        Students who ranked this project
      </Heading>
      <Text as="p" size="2" mt="2" className="!text-muted">
        Rank applicants as 1st, 2nd, or 3rd choice. Automatic matching fills
        this project&apos;s {seats} seat{seats === 1 ? "" : "s"} from your
        highest tiers first, honouring each student&apos;s own ranking.
      </Text>

      {error ? (
        <Text as="p" size="3" mt="5" role="alert" className="!text-red-800">
          {error}
        </Text>
      ) : loading || rankings == null ? (
        <RankingsListSkeleton />
      ) : rankings.length === 0 ? (
        <Text as="p" size="3" mt="5" className="!text-muted">
          No students have ranked this project yet.
        </Text>
      ) : (
        <ul className="ranked-students-list">
          {rankings.map((ranking) => (
            <li key={ranking.rankingId} className="ranked-students-row">
              <div className="ranked-students-badges">
                <span className="rank-badge">
                  Student {rankLabel(ranking.rank)}
                </span>
                {ranking.facultyRank != null ? (
                  <span className="rank-badge is-faculty">
                    Your {rankLabel(ranking.facultyRank)}
                  </span>
                ) : (
                  <span className="rank-badge is-muted">Not ranked by you</span>
                )}
              </div>
              <p className="ranked-students-name">{ranking.studentName}</p>
              <p className="ranked-students-meta">
                {ranking.studentEmail || "—"}
                <span aria-hidden> · </span>
                Ranked {formatRankedAt(ranking.rankedAt)}
              </p>
              <div className="ranked-students-actions">
                <Button
                  type="button"
                  variant="primary"
                  size="sm"
                  onClick={() => setRankTarget(ranking)}
                >
                  {ranking.facultyRank != null ? "Adjust rank" : "Rank"}
                </Button>
                <Button
                  href={viewRankedStudentHref(
                    userId,
                    projectId,
                    ranking.studentUserId,
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
      )}

      <FacultyCandidateRankModal
        open={rankTarget != null}
        onClose={() => setRankTarget(null)}
        projectId={projectId}
        studentUserId={rankTarget?.studentUserId ?? ""}
        studentName={rankTarget?.studentName ?? ""}
        volunteersRequired={volunteersRequired}
        onChanged={onRankingsChanged}
      />
    </section>
  );
}
