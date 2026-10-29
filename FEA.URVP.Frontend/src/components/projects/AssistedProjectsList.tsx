"use client";

import { Button } from "@/components/ui/Button";
import { RankingsListSkeleton } from "@/components/ui/SectionSkeletons";
import { Text } from "@/components/ui/Typography";
import { useAssistedProjects } from "@/hooks/useAssistedProjects";
import { assistedProjectHref } from "@/lib/auth";

export function AssistedProjectsList() {
  const { data, error } = useAssistedProjects(true);

  if (error) {
    return (
      <Text as="p" size="3" role="alert" className="!text-red-800">
        {error}
      </Text>
    );
  }

  if (data == null) return <RankingsListSkeleton />;

  if (data.length === 0) {
    return (
      <Text as="p" size="3" className="!text-muted">
        You are not listed as a research assistant on any project.
      </Text>
    );
  }

  return (
    <ul className="ranked-students-list">
      {data.map((project) => (
        <li key={project.projectId} className="ranked-students-row">
          <div className="ranked-students-badges">
            <span className="rank-badge is-matched">Research assistant</span>
            {project.status === "Inactive" ? (
              <span className="rank-badge is-muted">Previous cycle</span>
            ) : null}
          </div>
          <p className="ranked-students-name">{project.title}</p>
          <p className="ranked-students-meta">
            {project.facultyName}
            {project.semesterName ? ` · ${project.semesterName}` : ""}
          </p>
          <p className="ranked-students-meta">
            <a className="underline" href={`mailto:${project.facultyEmail}`}>
              {project.facultyEmail}
            </a>
          </p>
          <div className="ranked-students-actions">
            <Button
              href={assistedProjectHref(project.projectId)}
              variant="primary"
              size="sm"
            >
              View
            </Button>
          </div>
        </li>
      ))}
    </ul>
  );
}
