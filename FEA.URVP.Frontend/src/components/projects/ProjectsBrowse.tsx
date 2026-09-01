"use client";

import { useEffect, useMemo, useState } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { useAuth } from "@/components/auth/AuthProvider";
import { ProjectCard } from "@/components/projects/ProjectCard";
import { Button } from "@/components/ui/Button";
import { FieldSelect } from "@/components/ui/FieldSelect";
import { useStudentResearchTopics } from "@/hooks/useStudentResearchTopics";
import { ApiError } from "@/lib/api";
import { projectsHref } from "@/lib/auth";
import {
  openingsLeft,
  researchActivityTypes,
  researchAreas,
  type CatalogProject,
} from "@/lib/projects";
import type { MyProjectStatus } from "@/lib/project-form";
import {
  getMyProjectRankings,
  rankLabel,
  RANK_OPTIONS,
  type ProjectRankingDto,
} from "@/lib/project-rankings-api";
import { listProjects, toCatalogProject } from "@/lib/projects-api";

const PAGE_SIZE = 6;

type SortKey = "newest" | "openings" | "title";
type ProjectsBrowseVariant = "catalog" | "ranked";

const areaFilterOptions = ["All areas", ...researchAreas] as const;
const activityFilterOptions = [
  "All activities",
  ...researchActivityTypes,
] as const;

const sortOptions: { value: SortKey; label: string }[] = [
  { value: "newest", label: "Newest first" },
  { value: "openings", label: "Most openings" },
  { value: "title", label: "Title A–Z" },
];

const statusByCode: MyProjectStatus[] = ["Open", "Matching", "Closed"];

function listValues(joined: string): string[] {
  return joined
    .split(",")
    .map((part) => part.trim())
    .filter(Boolean);
}

function isAvailable(project: CatalogProject) {
  return project.status !== "Closed" && openingsLeft(project) > 0;
}

function catalogFromRanking(ranking: ProjectRankingDto): CatalogProject {
  return {
    id: ranking.projectId,
    title: ranking.projectTitle,
    researchArea: ranking.researchAreas.join(", "),
    activityType: "",
    volunteersRequired: 0,
    volunteersFilled: 0,
    status: statusByCode[ranking.projectStatus] ?? "Open",
    postedAt: "",
    postedAtISO: ranking.rankedAt.slice(0, 10),
    facultyName: ranking.facultyName,
    affiliation: ranking.facultyAffiliation,
    description: "",
    irbStage: "",
  };
}

function matchesSearchAndFilters(
  project: CatalogProject,
  query: string,
  area: string,
  activity: string,
): boolean {
  if (
    area !== "All areas" &&
    !listValues(project.researchArea).includes(area)
  ) {
    return false;
  }
  if (
    activity !== "All activities" &&
    !listValues(project.activityType).includes(activity)
  ) {
    return false;
  }
  if (!query) return true;

  const haystack = [
    project.title,
    project.description,
    project.facultyName,
    project.affiliation,
    project.researchArea,
    project.activityType,
    project.minQualifications ?? "",
  ]
    .join(" ")
    .toLowerCase();

  return haystack.includes(query);
}

function CatalogProjectCard({
  project,
  studentTopics,
  rank,
}: {
  project: CatalogProject;
  studentTopics: ReadonlySet<string>;
  rank?: number;
}) {
  const open = openingsLeft(project);
  const isClosed = project.status === "Closed" || open === 0;
  const hasOpeningsData =
    project.volunteersRequired > 0 || project.volunteersFilled > 0;

  return (
    <ProjectCard
      project={{
        id: project.id,
        title: project.title,
        facultyName: project.facultyName,
        affiliation: project.affiliation,
        description: project.description || undefined,
        researchAreas: listValues(project.researchArea),
        activityTypes: listValues(project.activityType),
      }}
      studentTopics={studentTopics}
      eyebrow={project.status}
      eyebrowMuted={isClosed}
      rank={rank}
      meta={project.postedAt ? `Posted ${project.postedAt}` : ""}
      metaEnd={
        hasOpeningsData ? (
          <span
            className={`text-xs font-medium uppercase tracking-[0.14em] ${
              isClosed ? "text-muted" : "text-primary"
            }`}
          >
            {isClosed
              ? "No openings"
              : `${open} opening${open === 1 ? "" : "s"}`}
          </span>
        ) : undefined
      }
    />
  );
}

function CatalogPagination({
  page,
  totalPages,
  onPageChange,
}: {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}) {
  if (totalPages <= 1) return null;

  const pages = Array.from({ length: totalPages }, (_, i) => i + 1);

  return (
    <nav
      aria-label="Projects pagination"
      className="mt-10 flex flex-col items-center gap-4 border-t border-primary/10 pt-8 sm:flex-row sm:justify-between"
    >
      <Text as="p" size="2" className="!text-muted">
        Page {page} of {totalPages}
      </Text>

      <div className="flex flex-wrap items-center justify-center gap-2">
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          Previous
        </Button>

        {pages.map((n) => (
          <Button
            key={n}
            type="button"
            variant={n === page ? "primary" : "outline"}
            size="sm"
            aria-label={`Page ${n}`}
            aria-current={n === page ? "page" : undefined}
            className="min-w-10"
            onClick={() => onPageChange(n)}
          >
            {n}
          </Button>
        ))}

        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          Next
        </Button>
      </div>
    </nav>
  );
}

function RankedEmptyCta() {
  return (
    <div className="ranked-empty-cta px-6 py-12 text-center sm:px-10 sm:py-16">
      <div className="flex flex-wrap items-center justify-center gap-2">
        {RANK_OPTIONS.map((rank) => (
          <span key={rank} className="rank-badge">
            {rankLabel(rank)}
          </span>
        ))}
      </div>
      <Heading
        as="h2"
        size="6"
        weight="medium"
        mt="5"
        className="!font-[family-name:var(--font-display)] !text-primary"
      >
        Rank your top 3 projects
      </Heading>
      <Text
        as="p"
        size="3"
        mt="3"
        className="mx-auto max-w-lg !leading-relaxed !text-muted"
      >
        You haven’t applied to any projects yet. Browse open opportunities and
        express interest to set your 1st, 2nd, and 3rd choices — matching
        depends on these rankings.
      </Text>
      <div className="mt-8 flex justify-center">
        <Button href={projectsHref()} variant="primary" size="lg">
          Apply to projects
        </Button>
      </div>
    </div>
  );
}

export function ProjectsBrowse({
  variant = "catalog",
}: {
  variant?: ProjectsBrowseVariant;
}) {
  const ranked = variant === "ranked";
  const { status, loading: authLoading } = useAuth();
  const isSignedIn = Boolean(status?.isAuthenticated);
  const studentTopics = useStudentResearchTopics();

  const [projects, setProjects] = useState<CatalogProject[] | null>(null);
  const [rankings, setRankings] = useState<ProjectRankingDto[] | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [rankingsError, setRankingsError] = useState<string | null>(null);
  const [query, setQuery] = useState("");
  const [area, setArea] = useState("All areas");
  const [activity, setActivity] = useState("All activities");
  const [sort, setSort] = useState<SortKey>("newest");
  const [pageNumber, setPageNumber] = useState(1);

  useEffect(() => {
    let cancelled = false;

    void (async () => {
      try {
        const items = await listProjects();
        if (cancelled) return;
        setProjects(items.map(toCatalogProject));
        setLoadError(null);
      } catch (err) {
        if (cancelled) return;
        setProjects([]);
        setLoadError(
          err instanceof ApiError
            ? err.message
            : "Could not load projects.",
        );
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  useEffect(() => {
    if (!ranked) {
      setRankings([]);
      setRankingsError(null);
      return;
    }

    if (authLoading) return;

    if (!isSignedIn) {
      setRankings([]);
      setRankingsError(null);
      return;
    }

    let cancelled = false;

    void (async () => {
      try {
        const mine = await getMyProjectRankings();
        if (cancelled) return;
        setRankings(mine);
        setRankingsError(null);
      } catch (err) {
        if (cancelled) return;
        setRankings([]);
        setRankingsError(
          err instanceof ApiError
            ? err.message
            : "Could not load your rankings.",
        );
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [authLoading, isSignedIn, ranked]);

  const filtered = useMemo(() => {
    if (!projects) return [];

    const q = query.trim().toLowerCase();

    const next = projects.filter((project) => {
      if (!isAvailable(project)) return false;
      return matchesSearchAndFilters(project, q, area, activity);
    });

    next.sort((a, b) => {
      if (sort === "title") return a.title.localeCompare(b.title);
      if (sort === "openings") return openingsLeft(b) - openingsLeft(a);
      return b.postedAtISO.localeCompare(a.postedAtISO);
    });

    return next;
  }, [projects, query, area, activity, sort]);

  const rankedItems = useMemo(() => {
    if (!rankings) return [];

    const byId = new Map((projects ?? []).map((project) => [project.id, project]));

    return rankings
      .slice()
      .sort((a, b) => a.rank - b.rank)
      .map((ranking) => ({
        project: byId.get(ranking.projectId) ?? catalogFromRanking(ranking),
        rank: ranking.rank,
      }));
  }, [projects, rankings]);

  const hasActiveFilters =
    query.trim() !== "" ||
    area !== "All areas" ||
    activity !== "All activities" ||
    sort !== "newest";

  const visibleCount = ranked ? rankedItems.length : filtered.length;
  const rankedEmpty = ranked && rankings != null && rankings.length === 0;
  const loadingList = ranked ? rankings == null : projects == null;

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const currentPage = Math.min(pageNumber, totalPages);
  const paged = useMemo(() => {
    const start = (currentPage - 1) * PAGE_SIZE;
    return filtered.slice(start, start + PAGE_SIZE);
  }, [filtered, currentPage]);

  useEffect(() => {
    setPageNumber(1);
  }, [query, area, activity, sort]);

  function clearFilters() {
    setQuery("");
    setArea("All areas");
    setActivity("All activities");
    setSort("newest");
    setPageNumber(1);
  }

  return (
    <div
      className={
        ranked
          ? undefined
          : "grid gap-10 lg:grid-cols-[minmax(16rem,20rem)_minmax(0,1fr)] xl:grid-cols-[minmax(16rem,22rem)_minmax(0,1fr)] lg:gap-12 xl:gap-14"
      }
    >
      {!ranked ? (
      <aside className="project-filters lg:sticky lg:top-24 lg:self-start">
        <Text
          as="p"
          size="1"
          weight="bold"
          className="!uppercase !tracking-[0.2em] !text-secondary-deep"
        >
          Filter & sort
        </Text>

        <div className="mt-5 space-y-4">
          <div>
            <label htmlFor="project-search" className="field-label">
              Search
            </label>
            <input
              id="project-search"
              type="search"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              className="field-input"
              placeholder="Title, faculty, keyword…"
            />
          </div>

          <div>
            <label htmlFor="project-area" className="field-label">
              Research area
            </label>
            <FieldSelect
              id="project-area"
              name="projectArea"
              placeholder="All areas"
              options={areaFilterOptions}
              value={area}
              onValueChange={setArea}
            />
          </div>

          <div>
            <label htmlFor="project-activity" className="field-label">
              Activity type
            </label>
            <FieldSelect
              id="project-activity"
              name="projectActivity"
              placeholder="All activities"
              options={activityFilterOptions}
              value={activity}
              onValueChange={setActivity}
            />
          </div>

          <div>
            <label htmlFor="project-sort" className="field-label">
              Sort by
            </label>
            <FieldSelect
              id="project-sort"
              name="projectSort"
              placeholder="Newest first"
              options={sortOptions}
              value={sort}
              onValueChange={(value) => setSort(value as SortKey)}
            />
          </div>
        </div>

        {hasActiveFilters ? (
          <button
            type="button"
            onClick={clearFilters}
            className="mt-5 text-sm font-medium text-primary transition hover:text-primary-soft"
          >
            Reset filters
          </button>
        ) : null}
      </aside>
      ) : null}

      <div>
        {!ranked ? (
          <div className="flex flex-col gap-2 border-b border-primary/10 pb-5 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <Heading
                as="h2"
                size="6"
                weight="medium"
                className="!font-[family-name:var(--font-display)] !text-primary"
              >
                Open Opportunities
              </Heading>
              <Text as="p" size="2" mt="1" className="!text-muted">
                {loadingList
                  ? "Loading…"
                  : `${visibleCount} result${visibleCount === 1 ? "" : "s"}`}
                {query.trim() ? ` for “${query.trim()}”` : null}
              </Text>
            </div>
            {!authLoading && !isSignedIn ? (
              <Button href="/sign-in" variant="outline" size="sm">
                Sign in to apply
              </Button>
            ) : null}
          </div>
        ) : null}

        {ranked && rankingsError && (rankings?.length ?? 0) === 0 ? (
          <div className="rounded-lg border border-dashed border-red-200 px-6 py-10 text-center">
            <Text as="p" size="3" className="!text-red-800">
              {rankingsError}
            </Text>
          </div>
        ) : ranked && rankings == null ? (
          <Text as="p" size="3" className="!text-muted">
            Loading your rankings…
          </Text>
        ) : rankedEmpty ? (
          <RankedEmptyCta />
        ) : loadError ? (
          <div className={`${ranked ? "" : "mt-10 "}rounded-lg border border-dashed border-red-200 px-6 py-10 text-center`}>
            <Text as="p" size="3" className="!text-red-800">
              {loadError}
            </Text>
          </div>
        ) : loadingList ? (
          <Text as="p" size="3" className="mt-8 !text-muted">
            Loading projects…
          </Text>
        ) : visibleCount === 0 ? (
          <div className={`${ranked ? "" : "mt-10 "}rounded-lg border border-dashed border-primary/20 px-6 py-16 text-center`}>
            <Heading
              as="h3"
              size="5"
              weight="medium"
              className="!font-[family-name:var(--font-display)] !text-primary"
            >
              No matching projects
            </Heading>
            <Text
              as="p"
              size="3"
              mt="2"
              className="mx-auto max-w-md !text-muted"
            >
              Try a broader search or clear filters to see the full catalog.
            </Text>
            <div className="mt-6 flex justify-center">
              <Button
                type="button"
                variant="secondary"
                size="md"
                onClick={clearFilters}
              >
                Clear filters
              </Button>
            </div>
          </div>
        ) : (
          <>
            <ul className={`grid w-full gap-5${ranked ? "" : " mt-6"}`}>
              {ranked
                ? rankedItems.map(({ project, rank }) => (
                    <CatalogProjectCard
                      key={project.id}
                      project={project}
                      studentTopics={studentTopics}
                      rank={rank}
                    />
                  ))
                : paged.map((project) => (
                    <CatalogProjectCard
                      key={project.id}
                      project={project}
                      studentTopics={studentTopics}
                    />
                  ))}
            </ul>
            {!ranked ? (
              <CatalogPagination
                page={currentPage}
                totalPages={totalPages}
                onPageChange={setPageNumber}
              />
            ) : null}
          </>
        )}
      </div>
    </div>
  );
}
