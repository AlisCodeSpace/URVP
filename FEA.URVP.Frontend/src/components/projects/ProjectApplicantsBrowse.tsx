"use client";

import { useMemo, useState } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { FieldSelect } from "@/components/ui/FieldSelect";
import {
  applicantInterests,
  applicantMajors,
  dummyProjectApplicants,
  type ProjectApplicant,
} from "@/lib/project-applicants";

type SortKey = "newest" | "gpa" | "name";

const sortOptions: { value: SortKey; label: string }[] = [
  { value: "newest", label: "Newest applied" },
  { value: "gpa", label: "GPA high–low" },
  { value: "name", label: "Name A–Z" },
];

const statusClass: Record<ProjectApplicant["status"], string> = {
  Pending: "text-secondary-deep",
  Accepted: "text-primary",
  Declined: "text-muted",
};

function ApplicantCard({ applicant }: { applicant: ProjectApplicant }) {
  return (
    <li>
      <article className="project-card border border-primary/12 bg-surface p-5 sm:p-6">
        <div className="flex flex-wrap items-center gap-x-3 gap-y-2">
          <span
            className={`text-xs font-bold uppercase tracking-[0.18em] ${statusClass[applicant.status]}`}
          >
            {applicant.status}
          </span>
          <span className="text-xs text-muted">
            Applied {applicant.appliedAt}
          </span>
          <span className="ml-auto text-xs font-medium uppercase tracking-[0.14em] text-primary">
            GPA {applicant.gpa}
          </span>
        </div>

        <Heading
          as="h3"
          size="5"
          weight="medium"
          mt="3"
          className="!font-[family-name:var(--font-display)] !leading-snug !text-primary"
        >
          {applicant.name}
        </Heading>

        <Text as="p" size="2" mt="2" className="!text-muted">
          {applicant.major}
          <span className="mx-2 text-primary/25" aria-hidden>
            ·
          </span>
          {applicant.classStanding}
          <span className="mx-2 text-primary/25" aria-hidden>
            ·
          </span>
          {applicant.creditsCompleted} credits
        </Text>

        <Text as="p" size="3" mt="3" className="!leading-relaxed !text-muted">
          <a
            href={`mailto:${applicant.email}`}
            className="font-medium text-primary transition hover:text-secondary-deep"
          >
            {applicant.email}
          </a>
        </Text>

        <div className="mt-4 flex flex-wrap gap-2">
          {applicant.interests.map((interest) => (
            <span key={interest} className="project-chip">
              {interest}
            </span>
          ))}
        </div>
      </article>
    </li>
  );
}

export function ProjectApplicantsBrowse({
  applicants = dummyProjectApplicants,
}: {
  applicants?: ProjectApplicant[];
}) {
  const majors = useMemo(() => applicantMajors(applicants), [applicants]);
  const interests = useMemo(
    () => applicantInterests(applicants),
    [applicants],
  );

  const majorOptions = useMemo(
    () => ["All majors", ...majors] as const,
    [majors],
  );
  const interestOptions = useMemo(
    () => ["All interests", ...interests] as const,
    [interests],
  );

  const [major, setMajor] = useState("All majors");
  const [interest, setInterest] = useState("All interests");
  const [sort, setSort] = useState<SortKey>("newest");

  const filtered = useMemo(() => {
    const next = applicants.filter((applicant) => {
      if (major !== "All majors" && applicant.major !== major) return false;
      if (
        interest !== "All interests" &&
        !applicant.interests.includes(interest)
      ) {
        return false;
      }
      return true;
    });

    next.sort((a, b) => {
      if (sort === "name") return a.name.localeCompare(b.name);
      if (sort === "gpa") return Number(b.gpa) - Number(a.gpa);
      return b.appliedAtISO.localeCompare(a.appliedAtISO);
    });

    return next;
  }, [applicants, major, interest, sort]);

  const hasActiveFilters =
    major !== "All majors" ||
    interest !== "All interests" ||
    sort !== "newest";

  function clearFilters() {
    setMajor("All majors");
    setInterest("All interests");
    setSort("newest");
  }

  if (applicants.length === 0) {
    return (
      <Text as="p" size="3" className="!text-muted">
        No volunteers have applied yet.
      </Text>
    );
  }

  return (
    <div>
      <div className="border-b border-primary/10 pb-5">
        <Heading
          as="h2"
          size="5"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          Applicants
        </Heading>
        <Text as="p" size="2" mt="1" className="!text-muted">
          {filtered.length} result{filtered.length === 1 ? "" : "s"}
        </Text>

        <div className="mt-4">
          <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
            <div className="applicant-filter-select min-w-0">
              <label htmlFor="applicant-major" className="sr-only">
                Major
              </label>
              <FieldSelect
                id="applicant-major"
                name="applicantMajor"
                placeholder="All majors"
                options={majorOptions}
                value={major}
                onValueChange={setMajor}
              />
            </div>

            <div className="applicant-filter-select min-w-0">
              <label htmlFor="applicant-interest" className="sr-only">
                Area of interest
              </label>
              <FieldSelect
                id="applicant-interest"
                name="applicantInterest"
                placeholder="All interests"
                options={interestOptions}
                value={interest}
                onValueChange={setInterest}
              />
            </div>

            <div className="applicant-filter-select min-w-0">
              <label htmlFor="applicant-sort" className="sr-only">
                Sort by
              </label>
              <FieldSelect
                id="applicant-sort"
                name="applicantSort"
                placeholder="Newest applied"
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
              className="mt-3 text-sm font-medium text-primary transition hover:text-primary-soft"
            >
              Reset
            </button>
          ) : null}
        </div>
      </div>

      {filtered.length === 0 ? (
        <div className="mt-10 rounded-lg border border-dashed border-primary/20 px-6 py-16 text-center">
          <Heading
            as="h3"
            size="5"
            weight="medium"
            className="!font-[family-name:var(--font-display)] !text-primary"
          >
            No matching volunteers
          </Heading>
          <Text
            as="p"
            size="3"
            mt="2"
            className="mx-auto max-w-md !text-muted"
          >
            Try a broader filter or clear filters to see all applicants.
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
        <ul className="mt-6 grid gap-5">
          {filtered.map((applicant) => (
            <ApplicantCard key={applicant.id} applicant={applicant} />
          ))}
        </ul>
      )}
    </div>
  );
}
