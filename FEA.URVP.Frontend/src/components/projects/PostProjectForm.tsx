"use client";

import type { ReactNode } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { FieldSelect } from "@/components/ui/FieldSelect";
import {
  irbStages,
  programDescription,
  researchActivityTypes,
  researchAreas,
} from "@/lib/project-form";

function Field({
  id,
  label,
  required,
  hint,
  children,
}: {
  id: string;
  label: string;
  required?: boolean;
  hint?: string;
  children: ReactNode;
}) {
  return (
    <div>
      <label htmlFor={id} className="field-label">
        {label}
        {required ? <span className="req">*</span> : null}
      </label>
      {children}
      {hint ? <p className="field-hint">{hint}</p> : null}
    </div>
  );
}

export function PostProjectForm() {
  return (
    <form
      className="flex flex-col gap-10"
      onSubmit={(e) => e.preventDefault()}
      noValidate
    >
      {/* Program description */}
      <section className="form-section">
        <Text
          as="p"
          size="2"
          weight="medium"
          className="!uppercase !tracking-[0.18em] !text-secondary-deep"
        >
          Program description
        </Text>
        <Text
          as="p"
          size="3"
          mt="3"
          className="max-w-3xl !leading-relaxed !text-muted"
        >
          {programDescription}
        </Text>
      </section>

      {/* General information */}
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
          Your profile details as the principal investigator.
        </Text>

        <div className="mt-6 grid gap-5 sm:grid-cols-2">
          <Field id="fullName" label="Full name" required>
            <input
              id="fullName"
              name="fullName"
              type="text"
              className="field-input"
              placeholder="e.g. Joseph Costantine"
              autoComplete="name"
            />
          </Field>
          <Field id="affiliation" label="Affiliation" required>
            <input
              id="affiliation"
              name="affiliation"
              type="text"
              className="field-input"
              placeholder="Faculty, center, or institute"
              autoComplete="organization"
            />
          </Field>
          <Field id="userName" label="User name" required>
            <input
              id="userName"
              name="userName"
              type="text"
              className="field-input"
              placeholder="AUB username"
              autoComplete="username"
            />
          </Field>
          <Field id="email" label="Email" required>
            <input
              id="email"
              name="email"
              type="email"
              className="field-input"
              placeholder="name@aub.edu.lb"
              autoComplete="email"
            />
          </Field>
        </div>
      </section>

      {/* Research information */}
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
          Describe the project students will join.
        </Text>

        <div className="mt-6 grid gap-5">
          <Field id="projectTitle" label="Project title" required>
            <input
              id="projectTitle"
              name="projectTitle"
              type="text"
              className="field-input"
              placeholder="Concise title for your research project"
            />
          </Field>

          <div className="grid gap-5 sm:grid-cols-2">
            <Field id="researchArea" label="Research area" required>
              <FieldSelect
                id="researchArea"
                name="researchArea"
                placeholder="Select a research area"
                options={researchAreas}
              />
            </Field>
            <Field id="irbStage" label="IRB approval stage" required>
              <FieldSelect
                id="irbStage"
                name="irbStage"
                placeholder="Select IRB stage"
                options={irbStages}
              />
            </Field>
          </div>

          <Field
            id="briefDescription"
            label="Brief description"
            required
            hint="Summarize goals, methods, and what volunteers will do."
          >
            <textarea
              id="briefDescription"
              name="briefDescription"
              className="field-textarea"
              rows={5}
              placeholder="Provide a clear overview of the research project…"
            />
          </Field>
        </div>
      </section>

      {/* Volunteers */}
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
          Help students understand the role and expectations.
        </Text>

        <div className="mt-6 grid gap-5">
          <div className="grid gap-5 sm:grid-cols-2">
            <Field id="activityType" label="Research activity type" required>
              <FieldSelect
                id="activityType"
                name="activityType"
                placeholder="Choose from list"
                options={researchActivityTypes}
              />
            </Field>
            <Field
              id="volunteersRequired"
              label="Number of volunteers required"
              required
            >
              <input
                id="volunteersRequired"
                name="volunteersRequired"
                type="number"
                min={1}
                className="field-input"
                placeholder="e.g. 2"
              />
            </Field>
          </div>

          <Field id="minQualifications" label="Minimum qualifications">
            <textarea
              id="minQualifications"
              name="minQualifications"
              className="field-textarea"
              rows={3}
              placeholder="Courses, skills, language, or experience required…"
            />
          </Field>

          <Field id="additionalComments" label="Additional comments">
            <textarea
              id="additionalComments"
              name="additionalComments"
              className="field-textarea"
              rows={3}
              placeholder="Schedule, location, duration, or other notes…"
            />
          </Field>
        </div>
      </section>

      <div className="flex flex-wrap items-center gap-3 border-t border-primary/10 pt-8">
        <Button type="submit" variant="primary" size="lg">
          Submit project
        </Button>
        <Button href="/my-projects" variant="outline" size="lg">
          Cancel
        </Button>
      </div>
    </form>
  );
}
