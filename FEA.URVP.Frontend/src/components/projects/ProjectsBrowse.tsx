"use client";

import { useEffect, useMemo, useState } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { useAuth } from "@/components/auth/AuthProvider";
import { ProjectCard } from "@/components/projects/ProjectCard";
import { Button } from "@/components/ui/Button";
import { FieldSelect } from "@/components/ui/FieldSelect";
import { useStudentResearchTopics } from "@/hooks/useStudentResearchTopics";
import { ApiError } from "@/lib/api";
import {
  openingsLeft,
  researchActivityTypes,
  researchAreas,
  type CatalogProject,
} from "@/lib/projects";
import { listProjects, toCatalogProject } from "@/lib/projects-api";

type SortKey = "newest" | "openings" | "title";
type StatusFilter = "all" | "available" | "open" | "matching" | "closed";

const areaFilterOptions = ["All areas", ...researchAreas] as const;
const activityFilterOptions = [
  "All activities",
  ...researchActivityTypes,
] as const;

const statusFilterOptions: { value: StatusFilter; label: string }[] = [
  { value: "all", label: "All statuses" },
  { value: "available", label: "Has openings" },
  { value: "open", label: "Open" },
  { value: "matching", label: "Matching" },
  { value: "closed", label: "Closed" },
];

const sortOptions: { value: SortKey; label: string }[] = [
  { value: "newest", label: "Newest first" },
  { value: "openings", label: "Most openings" },
  { value: "title", label: "Title A–Z" },
];

function listValues(joined: string): string[] {
  return joined
    .split(",")
    .map((part) => part.trim())
    .filter(Boolean);
}

function matchesStatus(project: CatalogProject, filter: StatusFilter) {
  if (filter === "all") return true;
  if (filter === "available") {
    return project.status !== "Closed" && openingsLeft(project) > 0;
  }
  return project.status.toLowerCase() === filter;
}

export function ProjectsBrowse() {
  const { status, loading: authLoading } = useAuth();
  const isSignedIn = Boolean(status?.isAuthenticated);
  const studentTopics = useStudentResearchTopics();

  const [projects, setProjects] = useState<CatalogProject[] | null>(null);
  const [loadError, setLoadError] = useState<string | null>(null);
  const [query, setQuery] = useState("");
  const [area, setArea] = useState("All areas");
  const [activity, setActivity] = useState("All activities");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("available");
  const [sort, setSort] = useState<SortKey>("newest");

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

  const filtered = useMemo(() => {
    if (!projects) return [];

    const q = query.trim().toLowerCase();

    const next = projects.filter((project) => {
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
      if (!matchesStatus(project, statusFilter)) return false;
      if (!q) return true;

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

      return haystack.includes(q);
    });

    next.sort((a, b) => {
      if (sort === "title") return a.title.localeCompare(b.title);
      if (sort === "openings") return openingsLeft(b) - openingsLeft(a);
      return b.postedAtISO.localeCompare(a.postedAtISO);
    });

    return next;
  }, [projects, query, area, activity, statusFilter, sort]);

  const hasActiveFilters =
    query.trim() !== "" ||
    area !== "All areas" ||
    activity !== "All activities" ||
    statusFilter !== "available" ||
    sort !== "newest";

  function clearFilters() {
    setQuery("");
    setArea("All areas");
    setActivity("All activities");
    setStatusFilter("available");
    setSort("newest");
  }

  return (
    <div className="grid gap-10 lg:grid-cols-[minmax(16rem,20rem)_minmax(0,1fr)] xl:grid-cols-[minmax(16rem,22rem)_minmax(0,1fr)] lg:gap-12 xl:gap-14">
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
            <label htmlFor="project-status" className="field-label">
              Status
            </label>
            <FieldSelect
              id="project-status"
              name="projectStatus"
              placeholder="Has openings"
              options={statusFilterOptions}
              value={statusFilter}
              onValueChange={(value) => setStatusFilter(value as StatusFilter)}
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

      <div>
        <div className="flex flex-col gap-2 border-b border-primary/10 pb-5 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <Heading
              as="h2"
              size="6"
              weight="medium"
              className="!font-[family-name:var(--font-display)] !text-primary"
            >
              Open opportunities
            </Heading>
            <Text as="p" size="2" mt="1" className="!text-muted">
              {projects == null
                ? "Loading…"
                : `${filtered.length} result${filtered.length === 1 ? "" : "s"}`}
              {query.trim() ? ` for “${query.trim()}”` : null}
            </Text>
          </div>
          {!authLoading && !isSignedIn ? (
            <Button href="/sign-in" variant="outline" size="sm">
              Sign in to apply
            </Button>
          ) : null}
        </div>

        {loadError ? (
          <div className="mt-10 rounded-lg border border-dashed border-red-200 px-6 py-10 text-center">
            <Text as="p" size="3" className="!text-red-800">
              {loadError}
            </Text>
          </div>
        ) : projects == null ? (
          <Text as="p" size="3" mt="8" className="!text-muted">
            Loading projects…
          </Text>
        ) : filtered.length === 0 ? (
          <div className="mt-10 rounded-lg border border-dashed border-primary/20 px-6 py-16 text-center">
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
          <ul className="mt-6 grid gap-5 xl:grid-cols-2">
            {filtered.map((project) => {
              const open = openingsLeft(project);
              const isClosed = project.status === "Closed" || open === 0;

              return (
                <ProjectCard
                  key={project.id}
                  project={{
                    id: project.id,
                    title: project.title,
                    facultyName: project.facultyName,
                    affiliation: project.affiliation,
                    description: project.description,
                    researchAreas: listValues(project.researchArea),
                    activityTypes: listValues(project.activityType),
                  }}
                  studentTopics={studentTopics}
                  eyebrow={project.status}
                  eyebrowMuted={isClosed}
                  meta={`Posted ${project.postedAt}`}
                  metaEnd={
                    <span
                      className={`text-xs font-medium uppercase tracking-[0.14em] ${
                        isClosed ? "text-muted" : "text-primary"
                      }`}
                    >
                      {isClosed
                        ? "No openings"
                        : `${open} opening${open === 1 ? "" : "s"}`}
                    </span>
                  }
                />
              );
            })}
          </ul>
        )}
      </div>
    </div>
  );
}
