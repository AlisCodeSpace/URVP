"use client";

import {
  useEffect,
  useRef,
  useState,
  type ChangeEvent,
  type FormEvent,
  type ReactNode,
} from "react";
import { Heading, Text } from "@radix-ui/themes";
import { useAuth } from "@/components/auth/AuthProvider";
import { Button } from "@/components/ui/Button";
import { IconPencil } from "@/components/ui/Icons";
import { FieldSelect } from "@/components/ui/FieldSelect";
import { MultiSelectSearch } from "@/components/ui/MultiSelectSearch";
import { ApiError } from "@/lib/api";
import { MAX_RESEARCH_AREAS, RESEARCH_AREAS } from "@/lib/research-areas";
import {
  getMyStudentProfile,
  toStudentProfileValues,
  toUpsertPayload,
  uploadStudentDocument,
  upsertMyStudentProfile,
} from "@/lib/student-profile-api";
import {
  cloneStudentProfile,
  DEGREE_OPTIONS,
  emptyStudentProfile,
  GENDER_OPTIONS,
  GRADUATION_YEAR_OPTIONS,
  LANGUAGE_OPTIONS,
  MAX_PROFILE_LANGUAGES,
  MAX_PROFILE_RESEARCH_TOPICS,
  TIME_SLOTS,
  toggleAvailabilitySlot,
  WEEKDAYS,
  type StudentProfileValues,
  type TimeSlot,
  type Weekday,
} from "@/lib/student-profile";

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

function Section({
  title,
  children,
}: {
  title: string;
  children: ReactNode;
}) {
  return (
    <section className="rounded-[var(--radius-lg)] border border-primary/12 bg-surface p-5 sm:p-7">
      <Heading
        as="h2"
        size="5"
        weight="medium"
        className="!font-[family-name:var(--font-display)] !text-primary"
      >
        {title}
      </Heading>
      <div className="mt-5 space-y-5">{children}</div>
    </section>
  );
}

function FileUploadField({
  id,
  label,
  required,
  fileName,
  accept,
  hint,
  disabled,
  onChange,
}: {
  id: string;
  label: string;
  required?: boolean;
  fileName: string | null;
  accept: string;
  hint?: string;
  disabled?: boolean;
  onChange: (file: File | null) => void;
}) {
  const inputRef = useRef<HTMLInputElement>(null);

  return (
    <div>
      <p className="field-label">
        {label}
        {required ? <span className="req">*</span> : null}
      </p>
      <div className="flex flex-wrap items-center gap-3">
        <input
          ref={inputRef}
          id={id}
          type="file"
          accept={accept}
          className="sr-only"
          disabled={disabled}
          onChange={(e: ChangeEvent<HTMLInputElement>) => {
            const file = e.target.files?.[0] ?? null;
            onChange(file);
          }}
        />
        {!disabled ? (
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={() => inputRef.current?.click()}
          >
            Upload
          </Button>
        ) : null}
        <Text as="span" size="2" className="!text-muted">
          {fileName ?? "No file selected"}
        </Text>
        {!disabled && fileName ? (
          <button
            type="button"
            className="text-sm font-medium text-primary underline-offset-2 hover:underline"
            onClick={() => {
              onChange(null);
              if (inputRef.current) inputRef.current.value = "";
            }}
          >
            Remove
          </button>
        ) : null}
      </div>
      {hint && !disabled ? <p className="field-hint mt-2">{hint}</p> : null}
    </div>
  );
}

export function StudentProfileForm() {
  const { status } = useAuth();
  const [values, setValues] = useState<StudentProfileValues>(() =>
    emptyStudentProfile(status?.name, status?.email),
  );
  const [baseline, setBaseline] = useState<StudentProfileValues>(() =>
    emptyStudentProfile(status?.name, status?.email),
  );
  const [pendingTranscript, setPendingTranscript] = useState<File | null>(null);
  const [pendingCiti, setPendingCiti] = useState<File | null>(null);
  const [editing, setEditing] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    if (!status?.isAuthenticated || !status.userId) return;

    let cancelled = false;

    void (async () => {
      setLoading(true);
      setError(null);
      try {
        const dto = await getMyStudentProfile();
        if (cancelled) return;
        const next = toStudentProfileValues(dto);
        setValues(next);
        setBaseline(cloneStudentProfile(next));
      } catch (err) {
        if (cancelled) return;
        setError(
          err instanceof ApiError
            ? err.message
            : "Could not load your profile.",
        );
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [status?.isAuthenticated, status?.userId]);

  const readOnly = !editing;
  const transcriptDisplayName =
    pendingTranscript?.name ?? values.transcriptFileName;
  const citiDisplayName = pendingCiti?.name ?? values.citiFileName;

  function setField<K extends keyof StudentProfileValues>(
    key: K,
    value: StudentProfileValues[K],
  ) {
    if (readOnly) return;
    if (key === "firstName" || key === "lastName" || key === "email") return;
    setValues((prev) => ({ ...prev, [key]: value }));
    setSuccess(null);
  }

  function onToggleSlot(day: Weekday, slot: TimeSlot) {
    if (readOnly) return;
    setValues((prev) => ({
      ...prev,
      availability: toggleAvailabilitySlot(prev.availability, day, slot),
    }));
    setSuccess(null);
  }

  function startEditing() {
    setBaseline(cloneStudentProfile(values));
    setPendingTranscript(null);
    setPendingCiti(null);
    setError(null);
    setSuccess(null);
    setEditing(true);
  }

  function cancelEditing() {
    setValues(cloneStudentProfile(baseline));
    setPendingTranscript(null);
    setPendingCiti(null);
    setError(null);
    setSuccess(null);
    setEditing(false);
  }

  function onTranscriptChange(file: File | null) {
    if (readOnly) return;
    setPendingTranscript(file);
    if (!file) {
      setField("transcriptFileId", null);
      setField("transcriptFileName", null);
    } else {
      setField("transcriptFileName", file.name);
    }
    setSuccess(null);
  }

  function onCitiChange(file: File | null) {
    if (readOnly) return;
    setPendingCiti(file);
    if (!file) {
      setField("citiFileId", null);
      setField("citiFileName", null);
    } else {
      setField("citiFileName", file.name);
    }
    setSuccess(null);
  }

  async function onSubmit(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    if (!editing || !status?.userId) return;

    setError(null);
    setSuccess(null);

    const hasTranscript =
      Boolean(pendingTranscript) || Boolean(values.transcriptFileId);

    if (
      !values.gender ||
      !values.mobileNumber.trim() ||
      !values.degree ||
      !values.expectedGraduationYear ||
      !values.completedCredits ||
      !values.cumulativeAverage.trim() ||
      !hasTranscript
    ) {
      setError("Please fill in all required fields.");
      return;
    }

    const gpa = Number(values.cumulativeAverage);
    const validGpa =
      Number.isFinite(gpa) &&
      ((gpa >= 0 && gpa <= 4) || (gpa > 4 && gpa <= 100));
    if (!validGpa) {
      setError("Enter a valid cumulative average (0–4.0 or 0–100).");
      return;
    }

    if (values.researchTopics.length > MAX_PROFILE_RESEARCH_TOPICS) {
      setError(
        `Select at most ${MAX_PROFILE_RESEARCH_TOPICS} research topics.`,
      );
      return;
    }

    setSubmitting(true);
    try {
      let nextValues = cloneStudentProfile(values);

      if (pendingTranscript) {
        const uploaded = await uploadStudentDocument(
          status.userId,
          "Transcript",
          pendingTranscript,
        );
        nextValues = {
          ...nextValues,
          transcriptFileId: uploaded.id,
          transcriptFileName: uploaded.fileName,
        };
      }

      if (pendingCiti) {
        const uploaded = await uploadStudentDocument(
          status.userId,
          "CitiCertification",
          pendingCiti,
        );
        nextValues = {
          ...nextValues,
          citiFileId: uploaded.id,
          citiFileName: uploaded.fileName,
        };
      }

      const savedDto = await upsertMyStudentProfile(toUpsertPayload(nextValues));
      const saved = toStudentProfileValues(savedDto);
      setValues(saved);
      setBaseline(cloneStudentProfile(saved));
      setPendingTranscript(null);
      setPendingCiti(null);
      setEditing(false);
      setSuccess("Profile saved.");
    } catch (err) {
      if (err instanceof ApiError) {
        setError(
          err.errors.length > 0
            ? err.errors.join(" ")
            : err.message,
        );
      } else if (err instanceof Error) {
        setError(err.message);
      } else {
        setError("Could not save your profile.");
      }
    } finally {
      setSubmitting(false);
    }
  }

  const maxTopics = Math.min(MAX_PROFILE_RESEARCH_TOPICS, MAX_RESEARCH_AREAS);
  const showRequired = editing;

  if (loading) {
    return (
      <p className="text-muted" role="status">
        Loading profile…
      </p>
    );
  }

  return (
    <form onSubmit={onSubmit} className="space-y-6" noValidate>
      <div className="flex flex-wrap items-center justify-between gap-3">
        {showRequired ? (
          <p className="text-sm text-secondary-deep">(*) Fields are required</p>
        ) : (
          <p className="text-sm text-muted">
            Viewing your profile. Click Edit to make changes.
          </p>
        )}
        <Button
          type="button"
          variant={editing ? "outline" : "primary"}
          size="md"
          disabled={submitting}
          onClick={editing ? cancelEditing : startEditing}
        >
          {editing ? (
            "Cancel"
          ) : (
            <>
              <IconPencil />
              Edit profile
            </>
          )}
        </Button>
      </div>

      <Section title="General information">
        <div className="grid gap-5 sm:grid-cols-2">
          <Field id="firstName" label="First name" required={showRequired}>
            <input
              id="firstName"
              className="field-input"
              value={values.firstName}
              disabled
              readOnly
              autoComplete="given-name"
            />
          </Field>
          <Field id="lastName" label="Last name" required={showRequired}>
            <input
              id="lastName"
              className="field-input"
              value={values.lastName}
              disabled
              readOnly
              autoComplete="family-name"
            />
          </Field>
          <Field id="email" label="Email">
            <input
              id="email"
              className="field-input"
              value={values.email}
              disabled
              readOnly
              autoComplete="email"
            />
          </Field>
        </div>

        <fieldset disabled={readOnly}>
          <legend className="field-label">
            Gender{showRequired ? <span className="req">*</span> : null}
          </legend>
          <div className="mt-1 flex flex-wrap gap-4">
            {GENDER_OPTIONS.map((option) => (
              <label
                key={option}
                className={`inline-flex items-center gap-2 text-sm font-medium text-foreground ${
                  readOnly ? "cursor-default opacity-80" : "cursor-pointer"
                }`}
              >
                <input
                  type="radio"
                  name="gender"
                  value={option}
                  checked={values.gender === option}
                  onChange={() => setField("gender", option)}
                  disabled={readOnly}
                  className="accent-[var(--primary)]"
                />
                {option}
              </label>
            ))}
          </div>
        </fieldset>

        <div className="grid gap-5 sm:grid-cols-2">
          <Field
            id="mobileNumber"
            label="Mobile number"
            required={showRequired}
          >
            <input
              id="mobileNumber"
              className="field-input"
              placeholder="Mobile number"
              value={values.mobileNumber}
              onChange={(e) => setField("mobileNumber", e.target.value)}
              disabled={readOnly}
              autoComplete="tel"
              inputMode="tel"
            />
          </Field>
          <Field id="degree" label="Degree" required={showRequired}>
            <FieldSelect
              id="degree"
              name="degree"
              placeholder="Select degree"
              options={DEGREE_OPTIONS}
              value={values.degree || undefined}
              onValueChange={(v) => setField("degree", v)}
              disabled={readOnly}
            />
          </Field>
          <Field
            id="expectedGraduationYear"
            label="Expected graduation year"
            required={showRequired}
          >
            <FieldSelect
              id="expectedGraduationYear"
              name="expectedGraduationYear"
              placeholder="Select year"
              options={GRADUATION_YEAR_OPTIONS}
              value={values.expectedGraduationYear || undefined}
              onValueChange={(v) => setField("expectedGraduationYear", v)}
              disabled={readOnly}
            />
          </Field>
          <Field id="languages" label="Languages">
            <MultiSelectSearch
              id="languages"
              options={LANGUAGE_OPTIONS}
              values={values.languages}
              onChange={(v) => setField("languages", v)}
              max={MAX_PROFILE_LANGUAGES}
              placeholder="Choose from list"
              disabled={readOnly}
            />
          </Field>
          <Field id="otherLanguages" label="Other languages">
            <input
              id="otherLanguages"
              className="field-input"
              placeholder="Other languages"
              value={values.otherLanguages}
              onChange={(e) => setField("otherLanguages", e.target.value)}
              disabled={readOnly}
            />
          </Field>
        </div>
      </Section>

      <Section title="Criteria checklist">
        <fieldset disabled={readOnly}>
          <legend className="field-label">
            I have successfully completed at least 24 credits at AUB
            {showRequired ? <span className="req">*</span> : null}
          </legend>
          <div className="mt-1 flex flex-wrap gap-4">
            {(
              [
                { value: "yes", label: "Yes" },
                { value: "no", label: "No" },
              ] as const
            ).map((option) => (
              <label
                key={option.value}
                className={`inline-flex items-center gap-2 text-sm font-medium text-foreground ${
                  readOnly ? "cursor-default opacity-80" : "cursor-pointer"
                }`}
              >
                <input
                  type="radio"
                  name="completedCredits"
                  value={option.value}
                  checked={values.completedCredits === option.value}
                  onChange={() => setField("completedCredits", option.value)}
                  disabled={readOnly}
                  className="accent-[var(--primary)]"
                />
                {option.label}
              </label>
            ))}
          </div>
        </fieldset>

        <Field
          id="cumulativeAverage"
          label="My cumulative average is above 78 (or 3.0) and is currently"
          required={showRequired}
        >
          <input
            id="cumulativeAverage"
            className="field-input"
            placeholder="Cumulative average"
            value={values.cumulativeAverage}
            onChange={(e) => setField("cumulativeAverage", e.target.value)}
            disabled={readOnly}
            inputMode="decimal"
          />
        </Field>

        <FileUploadField
          id="transcript"
          label="Upload your transcript"
          required={showRequired}
          accept=".pdf,application/pdf"
          fileName={transcriptDisplayName}
          disabled={readOnly}
          onChange={onTranscriptChange}
          hint="Upload a PDF of your unofficial or official transcript."
        />

        <FileUploadField
          id="citi"
          label="CITI certification"
          accept=".pdf,application/pdf"
          fileName={citiDisplayName}
          disabled={readOnly}
          onChange={onCitiChange}
          hint="Upload your CITI Certification (.pdf). Required for students who plan to apply to the Medical Research Volunteer Program (MRVP). Obtain it via citiprogram.org (Biomedical Research – Basic/Refresher Curriculum)."
        />
      </Section>

      <Section title="Previous research experience">
        <Field
          id="researchTopics"
          label="Research topic(s)"
          hint={
            editing
              ? `Select up to ${maxTopics} topics from the same catalog used when posting projects.`
              : undefined
          }
        >
          <MultiSelectSearch
            id="researchTopics"
            options={RESEARCH_AREAS}
            values={values.researchTopics}
            onChange={(v) => setField("researchTopics", v)}
            max={maxTopics}
            placeholder="Choose from list"
            disabled={readOnly}
          />
        </Field>

        <Field id="publications" label="Publications">
          <textarea
            id="publications"
            className="field-textarea"
            placeholder="List publications, posters, or related outputs"
            value={values.publications}
            onChange={(e) => setField("publications", e.target.value)}
            disabled={readOnly}
            rows={4}
          />
        </Field>
      </Section>

      <Section title="Time availability">
        <Text as="p" size="2" className="!text-muted">
          {editing
            ? "Select the slots when you are typically available to volunteer (at least 8 hours/week is expected for the program cycle)."
            : "Your typical weekly availability for volunteering."}
        </Text>
        <div className="overflow-x-auto">
          <table className="w-full min-w-[36rem] border-collapse text-left text-sm">
            <thead>
              <tr>
                <th className="pb-3 pr-3 font-semibold text-muted">Day</th>
                {TIME_SLOTS.map((slot) => (
                  <th
                    key={slot}
                    className="px-2 pb-3 text-center text-xs font-semibold uppercase tracking-wide text-muted"
                  >
                    {slot}
                  </th>
                ))}
              </tr>
            </thead>
            <tbody>
              {WEEKDAYS.map((day) => {
                const entry = values.availability.find((a) => a.day === day);
                return (
                  <tr key={day} className="border-t border-primary/8">
                    <td className="py-3 pr-3 font-medium text-foreground">
                      {day}
                    </td>
                    {TIME_SLOTS.map((slot) => {
                      const checked = entry?.slots.includes(slot) ?? false;
                      const slotId = `avail-${day}-${slot}`;
                      return (
                        <td key={slot} className="px-2 py-3 text-center">
                          <input
                            id={slotId}
                            type="checkbox"
                            checked={checked}
                            onChange={() => onToggleSlot(day, slot)}
                            disabled={readOnly}
                            className="h-4 w-4 accent-[var(--primary)]"
                            aria-label={`${day} ${slot}`}
                          />
                        </td>
                      );
                    })}
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </Section>

      {error ? (
        <p className="text-sm font-medium text-secondary-deep" role="alert">
          {error}
        </p>
      ) : null}
      {success ? (
        <p className="text-sm font-medium text-primary" role="status">
          {success}
        </p>
      ) : null}

      {editing ? (
        <div className="flex flex-wrap items-center justify-start gap-3 pt-2">
          <Button
            type="submit"
            variant="primary"
            size="lg"
            disabled={submitting}
          >
            {submitting ? "Saving…" : "Update profile"}
          </Button>
          <Button
            type="button"
            variant="outline"
            size="lg"
            disabled={submitting}
            onClick={cancelEditing}
          >
            Cancel
          </Button>
        </div>
      ) : null}
    </form>
  );
}
