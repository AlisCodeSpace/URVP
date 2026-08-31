"use client";

import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { viewRankedStudentHref } from "@/lib/auth";
import {
  formatRankedAt,
  rankLabel,
  type ProjectRankingStudentDto,
} from "@/lib/project-rankings-api";

export function FacultyProjectRankings({
  userId,
  projectId,
  rankings,
  loading,
  error,
}: {
  userId: string;
  projectId: string;
  rankings: ProjectRankingStudentDto[] | null;
  loading: boolean;
  error: string | null;
}) {
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
        Open a profile to review qualifications and download attached documents.
      </Text>

      {error ? (
        <Text as="p" size="3" mt="5" role="alert" className="!text-red-800">
          {error}
        </Text>
      ) : loading || rankings == null ? (
        <Text as="p" size="3" mt="5" className="!text-muted">
          Loading rankings…
        </Text>
      ) : rankings.length === 0 ? (
        <Text as="p" size="3" mt="5" className="!text-muted">
          No students have ranked this project yet.
        </Text>
      ) : (
        <ul className="ranked-students-list">
          {rankings.map((ranking) => (
            <li key={ranking.rankingId} className="ranked-students-row">
              <div>
                <span className="rank-badge">{rankLabel(ranking.rank)}</span>
                <p className="ranked-students-name">{ranking.studentName}</p>
                <p className="ranked-students-meta">
                  {ranking.studentEmail || "—"}
                  <span aria-hidden> · </span>
                  Ranked {formatRankedAt(ranking.rankedAt)}
                </p>
              </div>
              <Button
                href={viewRankedStudentHref(
                  userId,
                  projectId,
                  ranking.studentUserId,
                )}
                variant="outline"
                size="sm"
              >
                View profile
              </Button>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
