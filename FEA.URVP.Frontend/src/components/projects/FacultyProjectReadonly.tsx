import type { ReactNode } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { IconPencil } from "@/components/ui/Icons";
import { editProjectHref, myProjectsHref } from "@/lib/auth";
import { programDescription } from "@/lib/project-form";
import type { ProjectDto } from "@/lib/projects-api";

function Field({
  label,
  children,
}: {
  label: string;
  children: ReactNode;
}) {
  return (
    <div>
      <p className="field-label">{label}</p>
      {children}
    </div>
  );
}

function Value({ children }: { children: ReactNode }) {
  return <div className="field-display">{children}</div>;
}

function ChipList({ items }: { items: string[] }) {
  if (items.length === 0) {
    return <Value>—</Value>;
  }

  return (
    <div className="field-display-chips">
      {items.map((item) => (
        <span key={item} className="multi-select-chip">
          <span className="multi-select-chip-label">{item}</span>
        </span>
      ))}
    </div>
  );
}

export function FacultyProjectReadonly({
  userId,
  project,
}: {
  userId: string;
  project: ProjectDto;
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
          <Field label="Full name">
            <Value>{project.facultyName}</Value>
          </Field>
          <Field label="Affiliation">
            <Value>{project.affiliation}</Value>
          </Field>
          <Field label="User name">
            <Value>{project.userName || "—"}</Value>
          </Field>
          <Field label="Email">
            <Value>{project.email}</Value>
          </Field>
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
          <Field label="Project title">
            <Value>{project.title}</Value>
          </Field>

          <div className="grid gap-5 sm:grid-cols-2">
            <Field label="Research area">
              <ChipList items={project.researchAreas} />
            </Field>
            <Field label="IRB approval stage">
              <Value>{project.irbStageLabel}</Value>
            </Field>
          </div>

          <Field label="Brief description">
            <Value>
              <p className="whitespace-pre-wrap">{project.briefDescription}</p>
            </Value>
          </Field>
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
            <Field label="Research activity type">
              <ChipList items={project.activityTypes} />
            </Field>
            <Field label="Number of volunteers required">
              <Value>{project.volunteersRequired}</Value>
            </Field>
          </div>

          <Field label="Status">
            <Value>{project.status}</Value>
          </Field>

          <Field label="Minimum qualifications">
            <Value>
              {project.minQualifications?.trim() ? (
                <p className="whitespace-pre-wrap">{project.minQualifications}</p>
              ) : (
                "—"
              )}
            </Value>
          </Field>

          <Field label="Additional comments">
            <Value>
              {project.additionalComments?.trim() ? (
                <p className="whitespace-pre-wrap">
                  {project.additionalComments}
                </p>
              ) : (
                "—"
              )}
            </Value>
          </Field>
        </div>
      </section>

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
