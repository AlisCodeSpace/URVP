import Link from "next/link";
import type { ReactNode } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
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

export function ProjectDetail({ project }: { project: CatalogProject }) {
  const open = openingsLeft(project);
  const isClosed = project.status === "Closed" || open === 0;

  return (
    <article>
      <Link
        href="/projects"
        className="inline-flex items-center gap-2 text-sm font-medium text-muted transition hover:text-primary"
      >
        <span aria-hidden>←</span>
        Back to projects
      </Link>

      <div className="mt-8 grid gap-10 lg:grid-cols-[minmax(0,1fr)_18.5rem] lg:items-start lg:gap-14">
        <div className="min-w-0">
          <header>
            <div className="flex flex-wrap items-center gap-x-3 gap-y-2">
              <Text
                as="p"
                size="1"
                weight="bold"
                className={`!uppercase !tracking-[0.18em] ${
                  isClosed ? "!text-muted" : "!text-secondary-deep"
                }`}
              >
                {project.status}
              </Text>
              <span className="text-primary/25" aria-hidden>
                ·
              </span>
              <Text as="p" size="1" className="!uppercase !tracking-[0.14em] !text-muted">
                Posted {project.postedAt}
              </Text>
            </div>

            <Heading
              as="h1"
              size="8"
              weight="medium"
              mt="4"
              className="max-w-3xl !font-[family-name:var(--font-display)] !leading-[1.08] !text-primary"
            >
              {project.title}
            </Heading>
          </header>

          <div className="project-detail-mentor mt-8">
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
            <Text as="p" size="3" mt="1" className="!leading-relaxed !text-muted">
              {project.affiliation}
            </Text>
          </div>

          <dl className="project-detail-facts mt-8">
            <DetailFact label="Research area" value={project.researchArea} />
            <DetailFact label="Activity type" value={project.activityType} />
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
          <Text as="p" size="2" mt="3" className="!leading-relaxed !text-muted">
            {isClosed
              ? "This listing is not accepting new volunteers right now."
              : "Sign in with your AUB account to express interest. Matching is managed by the program team."}
          </Text>

          <div className="mt-6 flex flex-col gap-2">
            {isClosed ? (
              <Button type="button" variant="outline" size="md" disabled>
                Applications closed
              </Button>
            ) : (
              <Button href="/sign-in" variant="primary" size="md">
                Express interest
              </Button>
            )}
            <Button href="/projects" variant="ghost" size="md">
              Browse more
            </Button>
          </div>
        </aside>
      </div>
    </article>
  );
}
