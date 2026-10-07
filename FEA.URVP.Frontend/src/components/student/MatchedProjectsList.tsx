"use client";

import { ProjectCard } from "@/components/projects/ProjectCard";
import { Heading, Text } from "@/components/ui/Typography";
import { ProjectCardsSkeleton } from "@/components/ui/SectionSkeletons";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { useStudentResearchTopics } from "@/hooks/useStudentResearchTopics";
import { studentRankingsHref } from "@/lib/auth";
import { Button } from "@/components/ui/Button";
import type { MyProjectStatus } from "@/lib/project-form";
import {
  formatRankedAt,
  getMyPlacements,
  type MyPlacementDto,
} from "@/lib/project-rankings-api";

const STATUS_BY_CODE: MyProjectStatus[] = ["Open", "Matching", "Closed", "Inactive"];

function statusLabel(code: number): MyProjectStatus {
  return STATUS_BY_CODE[code] ?? "Open";
}

export function MatchedProjectsList() {
  const studentTopics = useStudentResearchTopics();
  const placementsQuery = useCancellableQuery(() => getMyPlacements(), [], {
    dataOnError: () => [],
    fallbackError: "Could not load your matched projects.",
  });
  const placements = placementsQuery.data;
  const error = placementsQuery.error;

  if (error && (placements?.length ?? 0) === 0 && placements != null) {
    return (
      <div className="rounded-lg border border-dashed border-red-200 px-6 py-10 text-center">
        <Text as="p" size="3" className="!text-red-800">
          {error}
        </Text>
      </div>
    );
  }

  if (placements == null) {
    return <ProjectCardsSkeleton count={2} className="" />;
  }

  if (placements.length === 0) {
    return <MatchedEmptyState />;
  }

  return (
    <ul className="grid w-full gap-5">
      {placements.map((placement) => (
        <MatchedProjectCard
          key={placement.id}
          placement={placement}
          studentTopics={studentTopics}
        />
      ))}
    </ul>
  );
}

function MatchedProjectCard({
  placement,
  studentTopics,
}: {
  placement: MyPlacementDto;
  studentTopics: ReadonlySet<string>;
}) {
  const ranked = placement.studentRank >= 1;
  const meta = [
    placement.semesterName,
    placement.assignedAt ? `Assigned ${formatRankedAt(placement.assignedAt)}` : "",
  ]
    .filter(Boolean)
    .join(" · ");

  return (
    <ProjectCard
      project={{
        id: placement.projectId,
        title: placement.projectTitle,
        facultyName: placement.facultyName,
        affiliation: placement.facultyAffiliation,
        description: placement.briefDescription,
        researchAreas: placement.researchAreas,
        activityTypes: placement.activityTypes,
      }}
      studentTopics={studentTopics}
      eyebrow={statusLabel(placement.projectStatus)}
      rank={ranked ? placement.studentRank : undefined}
      matched
      meta={meta}
    />
  );
}

function MatchedEmptyState() {
  return (
    <div className="ranked-empty-cta px-6 py-12 text-center sm:px-10 sm:py-16">
      <Heading
        as="h2"
        size="6"
        weight="medium"
        className="!font-[family-name:var(--font-display)] !text-primary"
      >
        No matched projects yet
      </Heading>
      <Text
        as="p"
        size="3"
        mt="3"
        className="mx-auto max-w-lg !leading-relaxed !text-muted"
      >
        When you are assigned to a project, it will appear here.
      </Text>
      <div className="mt-8 flex justify-center">
        <Button href={studentRankingsHref()} variant="outline" size="lg">
          View ranked projects
        </Button>
      </div>
    </div>
  );
}
