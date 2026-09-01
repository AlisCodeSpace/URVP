"use client";

import Link from "next/link";
import { useState, type ReactNode } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { useAuth } from "@/components/auth/AuthProvider";
import { ExpressInterestModal } from "@/components/projects/ExpressInterestModal";
import { PageHeader } from "@/components/layout/PageHeader";
import { Button } from "@/components/ui/Button";
import { useApplicationWindow } from "@/hooks/useApplicationWindow";
import {
  isResearchTopicMatch,
  useStudentResearchTopics,
} from "@/hooks/useStudentResearchTopics";
import { isStudent, projectsHref } from "@/lib/auth";
import { openingsLeft, type CatalogProject } from "@/lib/projects";

function DetailFact({
  label,
  value,
}: {
  label: string;
  value: string;
}) {
  return (
    <div className="project-detail-fact">
      <Text
        as="p"
        size="1"
        weight="bold"
        className="!uppercase !tracking-[0.16em] !text-muted"
      >
        {label}
      </Text>
      <Text as="p" size="3" mt="2" className="!leading-snug !text-primary">
        {value}
      </Text>
    </div>
  );
}

function DetailChips({
  label,
  items,
  studentTopics,
}: {
  label: string;
  items: string[];
  studentTopics?: ReadonlySet<string>;
}) {
  return (
    <div className="project-detail-fact">
      <Text
        as="p"
        size="1"
        weight="bold"
        className="!uppercase !tracking-[0.16em] !text-muted"
      >
        {label}
      </Text>
      <div className="mt-2 flex flex-wrap gap-2">
        {items.map((item) => (
          <span
            key={item}
            className={`project-chip${
              studentTopics && isResearchTopicMatch(item, studentTopics)
                ? " is-match"
                : ""
            }`}
          >
            {item}
          </span>
        ))}
      </div>
    </div>
  );
}

function DetailSection({
  eyebrow,
  title,
  children,
}: {
  eyebrow: string;
  title: string;
  children: ReactNode;
}) {
  return (
    <section className="project-detail-section">
      <Text
        as="p"
        size="1"
        weight="bold"
        className="!uppercase !tracking-[0.18em] !text-secondary-deep"
      >
        {eyebrow}
      </Text>
      <Heading
        as="h2"
        size="5"
        weight="medium"
        mt="2"
        className="!font-[family-name:var(--font-display)] !text-primary"
      >
        {title}
      </Heading>
      <div className="mt-4">{children}</div>
    </section>
  );
}

function splitJoined(value: string): string[] {
  return value
    .split(",")
    .map((part) => part.trim())
    .filter(Boolean);
}

export function ProjectDetail({ project }: { project: CatalogProject }) {
  const { status } = useAuth();
  const isSignedIn = Boolean(status?.isAuthenticated);
  const canRank = isSignedIn && isStudent(status?.role);
  const appWindow = useApplicationWindow();
  const studentTopics = useStudentResearchTopics();
  const open = openingsLeft(project);
  const isClosed = project.status === "Closed" || open === 0;
  const [rankOpen, setRankOpen] = useState(false);
  const researchAreas = splitJoined(project.researchArea);
  const activityTypes = splitJoined(project.activityType);

  return (
    <>
      <PageHeader
        eyebrow={project.status}
        title={project.title}
        description={`${project.facultyName} · ${project.affiliation}. Posted ${project.postedAt}.`}
      >
        <Link
          href={projectsHref()}
          className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
        >
          <span aria-hidden>←</span>
          Back to projects
        </Link>
      </PageHeader>

      <section className="site-container py-14 sm:py-16">
        <article>
          <div className="grid gap-10 lg:grid-cols-[minmax(0,1fr)_minmax(16rem,20rem)] xl:grid-cols-[minmax(0,1fr)_minmax(17rem,22rem)] lg:items-start lg:gap-14 xl:gap-16">
            <div className="min-w-0">
              <div className="project-detail-mentor">
                <Text
                  as="p"
                  size="1"
                  weight="bold"
                  className="!uppercase !tracking-[0.18em] !text-secondary-deep"
                >
                  Principal investigator
                </Text>
                <Heading
                  as="h2"
                  size="5"
                  weight="medium"
                  mt="2"
                  className="!font-[family-name:var(--font-display)] !text-primary"
                >
                  {project.facultyName}
                </Heading>
                <Text
                  as="p"
                  size="3"
                  mt="1"
                  className="!leading-relaxed !text-muted"
                >
                  {project.affiliation}
                </Text>
              </div>

              <dl className="project-detail-facts mt-8">
                <DetailChips
                  label="Research area"
                  items={researchAreas}
                  studentTopics={canRank ? studentTopics : undefined}
                />
                <DetailChips label="Activity type" items={activityTypes} />
                <DetailFact label="IRB stage" value={project.irbStage} />
                <DetailFact
                  label="Volunteer seats"
                  value={`${open} of ${project.volunteersRequired} open`}
                />
              </dl>

              <DetailSection eyebrow="Overview" title="About this project">
                <Text
                  as="p"
                  size="4"
                  className="max-w-3xl !leading-[1.7] !text-foreground/90"
                >
                  {project.description}
                </Text>
              </DetailSection>

              {project.minQualifications ? (
                <DetailSection
                  eyebrow="Preparation"
                  title="Minimum qualifications"
                >
                  <div className="project-detail-callout">
                    <Text
                      as="p"
                      size="3"
                      className="!leading-relaxed !text-foreground/85"
                    >
                      {project.minQualifications}
                    </Text>
                  </div>
                </DetailSection>
              ) : null}

              {project.additionalComments ? (
                <DetailSection eyebrow="Logistics" title="Additional notes">
                  <Text
                    as="p"
                    size="3"
                    className="max-w-3xl !leading-relaxed !text-muted"
                  >
                    {project.additionalComments}
                  </Text>
                </DetailSection>
              ) : null}
            </div>

            <aside className="project-apply-panel h-fit border border-primary/12 bg-surface p-6 lg:sticky lg:top-24">
              <Text
                as="p"
                size="1"
                weight="bold"
                className="!uppercase !tracking-[0.18em] !text-secondary-deep"
              >
                Volunteer seats
              </Text>
              <p className="mt-3 font-[family-name:var(--font-display)] text-4xl font-medium text-primary">
                {open}
                <span className="ml-2 text-lg font-normal text-muted">
                  of {project.volunteersRequired} open
                </span>
              </p>
              <Text
                as="p"
                size="2"
                mt="3"
                className="!leading-relaxed !text-muted"
              >
                {isClosed
                  ? "This listing is not accepting new volunteers right now."
                  : !appWindow.loading && !appWindow.isOpen
                    ? "The student application window is currently closed. Check back during the application period (typically mid-September to end of September)."
                    : canRank
                      ? "Matching is managed by the program team. Rank this project as one of your top 3 choices."
                      : isSignedIn
                        ? "Only student accounts can express interest in projects."
                        : "Sign in with your AUB account to express interest. Matching is managed by the program team."}
              </Text>

              <div className="mt-6 flex flex-col gap-2">
                {isClosed ? (
                  <Button type="button" variant="outline" size="md" disabled>
                    Applications closed
                  </Button>
                ) : !appWindow.loading && !appWindow.isOpen ? (
                  <Button
                    type="button"
                    variant="outline"
                    size="md"
                    disabled
                    title="The application window is not currently open."
                  >
                    Applications not open
                  </Button>
                ) : canRank ? (
                  <Button
                    type="button"
                    variant="primary"
                    size="md"
                    onClick={() => setRankOpen(true)}
                  >
                    Express interest
                  </Button>
                ) : isSignedIn ? (
                  <Button type="button" variant="outline" size="md" disabled>
                    Students only
                  </Button>
                ) : (
                  <Button href="/sign-in" variant="primary" size="md">
                    Sign in to apply
                  </Button>
                )}
                <Button href={projectsHref()} variant="ghost" size="md">
                  Browse more
                </Button>
              </div>
            </aside>
          </div>

          <ExpressInterestModal
            open={rankOpen}
            onClose={() => setRankOpen(false)}
            projectId={project.id}
            projectTitle={project.title}
          />
        </article>
      </section>
    </>
  );
}
