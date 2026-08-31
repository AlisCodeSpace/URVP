"use client";

import { useCallback, useEffect, useState } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { ExpressInterestModal } from "@/components/projects/ExpressInterestModal";
import { ProjectCard } from "@/components/projects/ProjectCard";
import { Button } from "@/components/ui/Button";
import { useStudentResearchTopics } from "@/hooks/useStudentResearchTopics";
import { ApiError } from "@/lib/api";
import {
  formatRankedAt,
  getMyProjectRankings,
  rankLabel,
  type ProjectRankingDto,
} from "@/lib/project-rankings-api";

export function StudentRankingsList() {
  const studentTopics = useStudentResearchTopics();
  const [rankings, setRankings] = useState<ProjectRankingDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [adjusting, setAdjusting] = useState<ProjectRankingDto | null>(null);

  const load = useCallback(async () => {
    try {
      const data = await getMyProjectRankings();
      setRankings(data);
      setError(null);
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : "Could not load your rankings.",
      );
    }
  }, []);

  useEffect(() => {
    let cancelled = false;

    void (async () => {
      try {
        const data = await getMyProjectRankings();
        if (!cancelled) {
          setRankings(data);
          setError(null);
        }
      } catch (err) {
        if (!cancelled) {
          setError(
            err instanceof ApiError
              ? err.message
              : "Could not load your rankings.",
          );
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  async function handleRankingChanged() {
    await load();
  }

  if (loading) {
    return (
      <Text as="p" size="3" className="!text-muted">
        Loading your rankings…
      </Text>
    );
  }

  if (error && rankings.length === 0) {
    return (
      <Text as="p" size="3" role="alert" className="!text-red-700">
        {error}
      </Text>
    );
  }

  if (rankings.length === 0) {
    return (
      <div className="rounded-[var(--radius-lg)] border border-dashed border-primary/20 bg-surface px-6 py-14 text-center">
        <Heading
          as="h2"
          size="5"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          No rankings yet
        </Heading>
        <Text as="p" size="3" mt="3" className="!text-muted">
          Browse open projects and express interest to rank up to 3 choices.
        </Text>
        <div className="mt-6">
          <Button href="/student/projects" variant="primary" size="md">
            Browse projects
          </Button>
        </div>
      </div>
    );
  }

  return (
    <>
      <ul className="grid w-full gap-4">
        {rankings.map((ranking) => (
          <ProjectCard
            key={ranking.id}
            project={{
              id: ranking.projectId,
              title: ranking.projectTitle,
              facultyName: ranking.facultyName,
              affiliation: ranking.facultyAffiliation,
              researchAreas: ranking.researchAreas,
            }}
            studentTopics={studentTopics}
            eyebrow={rankLabel(ranking.rank)}
            meta={`Ranked ${formatRankedAt(ranking.rankedAt)}`}
            actions={
              <>
                <Button
                  href={`/projects/${ranking.projectId}`}
                  variant="primary"
                  size="sm"
                >
                  View project
                </Button>
                <Button
                  type="button"
                  variant="ghost"
                  size="sm"
                  onClick={() => setAdjusting(ranking)}
                >
                  Adjust rank
                </Button>
              </>
            }
          />
        ))}
      </ul>

      {adjusting ? (
        <ExpressInterestModal
          open
          mode="adjust"
          onClose={() => setAdjusting(null)}
          projectId={adjusting.projectId}
          projectTitle={adjusting.projectTitle}
          onChanged={() => void handleRankingChanged()}
        />
      ) : null}
    </>
  );
}
