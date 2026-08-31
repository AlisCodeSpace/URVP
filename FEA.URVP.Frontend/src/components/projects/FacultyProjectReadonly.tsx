import type { ReactNode } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { IconPencil } from "@/components/ui/Icons";
import {
  ReadOnlyChips,
  ReadOnlyField,
  ReadOnlyValue,
} from "@/components/ui/ReadOnlyField";
import { editProjectHref, myProjectsHref } from "@/lib/auth";
import { programDescription } from "@/lib/project-form";
import type { ProjectDto } from "@/lib/projects-api";

export function FacultyProjectReadonly({
  userId,
  project,
  children,
}: {
  userId: string;
  project: ProjectDto;
  children?: ReactNode;
}) {
  return (
    <div className="flex flex-col gap-10">
      <section className="form-section">
        <Text
          as="p"
          size="2"
          weight="medium"
          className="!uppercase !tracking-[0.18em] !text-secondary-deep"
        >
          Program description
        </Text>
        <Text as="p" size="3" mt="3" className="!leading-relaxed !text-muted">
          {programDescription}
        </Text>
      </section>

      <section className="form-section">
        <Heading
          as="h2"
          size="5"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          General information
        </Heading>
        <Text as="p" size="2" mt="2" className="!text-muted">
          Principal investigator details for this listing.
        </Text>

        <div className="mt-6 grid gap-5 sm:grid-cols-2">
          <ReadOnlyField label="Full name">
            <ReadOnlyValue>{project.facultyName}</ReadOnlyValue>
          </ReadOnlyField>
          <ReadOnlyField label="Affiliation">
            <ReadOnlyValue>{project.affiliation}</ReadOnlyValue>
          </ReadOnlyField>
          <ReadOnlyField label="User name">
            <ReadOnlyValue>{project.userName || "—"}</ReadOnlyValue>
          </ReadOnlyField>
          <ReadOnlyField label="Email">
            <ReadOnlyValue>{project.email}</ReadOnlyValue>
          </ReadOnlyField>
        </div>
      </section>

      <section className="form-section">
        <Heading
          as="h2"
          size="5"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          Research information
        </Heading>
        <Text as="p" size="2" mt="2" className="!text-muted">
          Project details shared with students.
        </Text>

        <div className="mt-6 grid gap-5">
          <ReadOnlyField label="Project title">
            <ReadOnlyValue>{project.title}</ReadOnlyValue>
          </ReadOnlyField>

          <div className="grid gap-5 sm:grid-cols-2">
            <ReadOnlyField label="Research area">
              <ReadOnlyChips items={project.researchAreas} />
            </ReadOnlyField>
            <ReadOnlyField label="IRB approval stage">
              <ReadOnlyValue>{project.irbStageLabel}</ReadOnlyValue>
            </ReadOnlyField>
          </div>

          <ReadOnlyField label="Brief description">
            <ReadOnlyValue>
              <p className="whitespace-pre-wrap">{project.briefDescription}</p>
            </ReadOnlyValue>
          </ReadOnlyField>
        </div>
      </section>

      <section className="form-section">
        <Heading
          as="h2"
          size="5"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          Information about volunteers
        </Heading>
        <Text as="p" size="2" mt="2" className="!text-muted">
          Role and expectations for student volunteers.
        </Text>

        <div className="mt-6 grid gap-5">
          <div className="grid gap-5 sm:grid-cols-2">
            <ReadOnlyField label="Research activity type">
              <ReadOnlyChips items={project.activityTypes} />
            </ReadOnlyField>
            <ReadOnlyField label="Number of volunteers required">
              <ReadOnlyValue>{project.volunteersRequired}</ReadOnlyValue>
            </ReadOnlyField>
          </div>

          <ReadOnlyField label="Status">
            <ReadOnlyValue>{project.status}</ReadOnlyValue>
          </ReadOnlyField>

          <ReadOnlyField label="Minimum qualifications">
            <ReadOnlyValue>
              {project.minQualifications?.trim() ? (
                <p className="whitespace-pre-wrap">{project.minQualifications}</p>
              ) : (
                "—"
              )}
            </ReadOnlyValue>
          </ReadOnlyField>

          <ReadOnlyField label="Additional comments">
            <ReadOnlyValue>
              {project.additionalComments?.trim() ? (
                <p className="whitespace-pre-wrap">
                  {project.additionalComments}
                </p>
              ) : (
                "—"
              )}
            </ReadOnlyValue>
          </ReadOnlyField>
        </div>
      </section>

      {children}

      <div className="flex flex-wrap items-center gap-3 border-t border-primary/10 pt-8">
        <Button
          href={editProjectHref(userId, project.id)}
          variant="primary"
          size="lg"
        >
          <IconPencil />
          Edit project
        </Button>
        <Button
          href={myProjectsHref(userId)}
          variant="outline-secondary"
          size="lg"
        >
          Back to my projects
        </Button>
      </div>
    </div>
  );
}
