"use client";

import { useRouter } from "next/navigation";
import {
  useState,
  type FormEvent,
  type ReactNode,
} from "react";
import { Heading, Text } from "@radix-ui/themes";
import { useAuth } from "@/components/auth/AuthProvider";
import { Button } from "@/components/ui/Button";
import { FieldSelect } from "@/components/ui/FieldSelect";
import { MultiSelectSearch } from "@/components/ui/MultiSelectSearch";
import { ApiError } from "@/lib/api";
import { myProjectsHref } from "@/lib/auth";
import {
  emptyProjectFormValues,
  irbStageOptions,
  programDescription,
  projectStatusOptions,
  type ProjectFormValues,
} from "@/lib/project-form";
import { createProject, updateProject } from "@/lib/projects-api";
import {
  MAX_RESEARCH_ACTIVITY_TYPES,
  RESEARCH_ACTIVITY_TYPES,
} from "@/lib/research-activity-types";
import { MAX_RESEARCH_AREAS, RESEARCH_AREAS } from "@/lib/research-areas";

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

type PostProjectFormProps = {
  userId: string;
  mode?: "create" | "edit";
  projectId?: string;
  initialValues?: ProjectFormValues;
};

export function PostProjectForm({
  userId,
  mode = "create",
  projectId,
  initialValues,
}: PostProjectFormProps) {
  const router = useRouter();
  const { status } = useAuth();
  const [values, setValues] = useState<ProjectFormValues>(
    initialValues ?? emptyProjectFormValues,
  );
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const isEdit = mode === "edit";

  function setField<K extends keyof ProjectFormValues>(
    key: K,
    value: ProjectFormValues[K],
  ) {
    setValues((prev) => ({ ...prev, [key]: value }));
  }

  async function onSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setError(null);

    if (
      !status?.affiliation?.trim() ||
      !status?.userName?.trim() ||
      !values.title.trim() ||
      values.researchAreas.length === 0 ||
      !values.irbStage ||
      !values.briefDescription.trim() ||
      values.activityTypes.length === 0 ||
      !values.volunteersRequired
    ) {
      setError("Please fill in all required fields.");
      return;
    }

    if (values.researchAreas.length > MAX_RESEARCH_AREAS) {
      setError(`Select at most ${MAX_RESEARCH_AREAS} research areas.`);
      return;
    }

    if (values.activityTypes.length > MAX_RESEARCH_ACTIVITY_TYPES) {
      setError(
        `Select at most ${MAX_RESEARCH_ACTIVITY_TYPES} research activity types.`,
      );
      return;
    }

    const volunteers = Number(values.volunteersRequired);
    if (!Number.isFinite(volunteers) || volunteers < 1) {
      setError("Number of volunteers must be at least 1.");
      return;
    }

    const payload: ProjectFormValues = {
      ...values,
      affiliation: status.affiliation.trim(),
      userName: status.userName.trim(),
    };

    setSubmitting(true);
    try {
      if (isEdit) {
        if (!projectId) throw new Error("Missing project id.");
        await updateProject(projectId, payload);
      } else {
        await createProject(payload);
      }
      router.push(myProjectsHref(userId));
      router.refresh();
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.errors[0] ?? err.message);
      } else {
        setError("Something went wrong. Please try again.");
      }
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <form className="flex flex-col gap-10" onSubmit={onSubmit} noValidate>
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
              value={status?.name ?? ""}
              readOnly
              autoComplete="name"
            />
          </Field>
          <Field id="affiliation" label="Affiliation" required>
            <input
              id="affiliation"
              name="affiliation"
              type="text"
              className="field-input"
              value={status?.affiliation ?? ""}
              readOnly
              autoComplete="organization"
            />
          </Field>
          <Field id="userName" label="User name" required>
            <input
              id="userName"
              name="userName"
              type="text"
              className="field-input"
              value={status?.userName ?? ""}
              readOnly
              autoComplete="username"
            />
          </Field>
          <Field id="email" label="Email" required>
            <input
              id="email"
              name="email"
              type="email"
              className="field-input"
              value={status?.email ?? ""}
              readOnly
              autoComplete="email"
            />
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
              value={values.title}
              onChange={(e) => setField("title", e.target.value)}
              required
            />
          </Field>

          <div className="grid gap-5 sm:grid-cols-2">
            <Field id="researchAreas" label="Research area" required>
              <MultiSelectSearch
                id="researchAreas"
                options={RESEARCH_AREAS}
                values={values.researchAreas}
                onChange={(next) => setField("researchAreas", next)}
                placeholder="Choose from list"
                max={MAX_RESEARCH_AREAS}
              />
            </Field>
            <Field id="irbStage" label="IRB approval stage" required>
              <FieldSelect
                id="irbStage"
                name="irbStage"
                placeholder="Choose from list"
                options={irbStageOptions}
                value={values.irbStage || undefined}
                onValueChange={(v) => setField("irbStage", v)}
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
              value={values.briefDescription}
              onChange={(e) => setField("briefDescription", e.target.value)}
              required
            />
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
          Help students understand the role and expectations.
        </Text>

        <div className="mt-6 grid gap-5">
          <div className="grid gap-5 sm:grid-cols-2">
            <Field id="activityTypes" label="Research activity type" required>
              <MultiSelectSearch
                id="activityTypes"
                options={RESEARCH_ACTIVITY_TYPES}
                values={values.activityTypes}
                onChange={(next) => setField("activityTypes", next)}
                placeholder="Choose from list"
                max={MAX_RESEARCH_ACTIVITY_TYPES}
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
                value={values.volunteersRequired}
                onChange={(e) => setField("volunteersRequired", e.target.value)}
                required
              />
            </Field>
          </div>

          {isEdit ? (
            <Field id="status" label="Status" required>
              <FieldSelect
                id="status"
                name="status"
                placeholder="Select status"
                options={projectStatusOptions}
                value={values.status}
                onValueChange={(v) =>
                  setField("status", v as ProjectFormValues["status"])
                }
              />
            </Field>
          ) : null}

          <Field id="minQualifications" label="Minimum qualifications">
            <textarea
              id="minQualifications"
              name="minQualifications"
              className="field-textarea"
              rows={3}
              placeholder="Courses, skills, language, or experience required…"
              value={values.minQualifications}
              onChange={(e) => setField("minQualifications", e.target.value)}
            />
          </Field>

          <Field id="additionalComments" label="Additional comments">
            <textarea
              id="additionalComments"
              name="additionalComments"
              className="field-textarea"
              rows={3}
              placeholder="Schedule, location, duration, or other notes…"
              value={values.additionalComments}
              onChange={(e) => setField("additionalComments", e.target.value)}
            />
          </Field>
        </div>
      </section>

      {error ? (
        <Text
          as="p"
          size="2"
          role="alert"
          className="rounded-md bg-red-50 px-3 py-2 !text-red-800"
        >
          {error}
        </Text>
      ) : null}

      <div className="flex flex-wrap items-center gap-3 border-t border-primary/10 pt-8">
        <Button type="submit" variant="primary" size="lg" disabled={submitting}>
          {submitting
            ? isEdit
              ? "Saving…"
              : "Submitting…"
            : isEdit
              ? "Save changes"
              : "Submit project"}
        </Button>
        <Button
          href={myProjectsHref(userId)}
          variant="outline-secondary"
          size="lg"
        >
          Cancel
        </Button>
      </div>
    </form>
  );
}
