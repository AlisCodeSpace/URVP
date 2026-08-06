"use client";

import Link from "next/link";
import type { ReactNode } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { isResearchTopicMatch } from "@/hooks/useStudentResearchTopics";

export type ProjectCardData = {
  id: string;
  title: string;
  facultyName: string;
  affiliation: string;
  description?: string;
  researchAreas: string[];
  activityTypes?: string[];
};

type ProjectCardProps = {
  project: ProjectCardData;
  studentTopics?: ReadonlySet<string>;
  href?: string;
  eyebrow: string;
  eyebrowMuted?: boolean;
  meta: string;
  metaEnd?: ReactNode;
  actions?: ReactNode;
};

export function ProjectCard({
  project,
  studentTopics,
  href = `/projects/${project.id}`,
  eyebrow,
  eyebrowMuted = false,
  meta,
  metaEnd,
  actions,
}: ProjectCardProps) {
  const areaChips = project.researchAreas.slice(0, 2);
  const activityChips = (project.activityTypes ?? []).slice(0, 2);
  const topics = studentTopics ?? new Set<string>();

  const body = (
    <>
      <div className="flex flex-wrap items-center gap-x-3 gap-y-2">
        <span
          className={`text-xs font-bold uppercase tracking-[0.18em] ${
            eyebrowMuted ? "text-muted" : "text-secondary-deep"
          }`}
        >
          {eyebrow}
        </span>
        <span className="text-xs text-muted">{meta}</span>
        {metaEnd ? <span className="ml-auto">{metaEnd}</span> : null}
      </div>

      <Heading
        as="h2"
        size="5"
        weight="medium"
        mt="3"
        className={`!font-[family-name:var(--font-display)] !leading-snug !text-primary${
          actions
            ? ""
            : " transition group-hover:!text-primary-soft"
        }`}
      >
        {project.title}
      </Heading>

      <Text as="p" size="2" mt="2" className="!text-muted">
        {project.facultyName}
        <span className="mx-2 text-primary/25" aria-hidden>
          ·
        </span>
        {project.affiliation}
      </Text>

      {project.description ? (
        <Text
          as="p"
          size="3"
          mt="3"
          className="line-clamp-2 !leading-relaxed !text-muted"
        >
          {project.description}
        </Text>
      ) : null}

      {(areaChips.length > 0 || activityChips.length > 0) && (
        <div className="mt-4 flex flex-wrap items-center gap-2">
          {areaChips.map((chip) => (
            <span
              key={`area-${chip}`}
              className={`project-chip${
                isResearchTopicMatch(chip, topics) ? " is-match" : ""
              }`}
            >
              {chip}
            </span>
          ))}
          {areaChips.length > 0 && activityChips.length > 0 ? (
            <span className="text-primary/25" aria-hidden>
              ·
            </span>
          ) : null}
          {activityChips.map((chip) => (
            <span key={`activity-${chip}`} className="project-chip">
              {chip}
            </span>
          ))}
        </div>
      )}
    </>
  );

  if (actions) {
    return (
      <li>
        <article className="project-card border border-primary/12 bg-surface p-5 sm:p-6">
          {body}
          <div className="mt-5 flex flex-wrap gap-2">{actions}</div>
        </article>
      </li>
    );
  }

  return (
    <li>
      <Link
        href={href}
        className="project-card group block border border-primary/12 bg-surface p-5 transition sm:p-6"
      >
        {body}
        <span className="mt-5 inline-flex items-center gap-2 text-sm font-medium text-secondary-deep transition group-hover:gap-3">
          View project
          <span aria-hidden>→</span>
        </span>
      </Link>
    </li>
  );
}
