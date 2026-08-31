"use client";

import { useState, type ReactNode } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { IconDownload } from "@/components/ui/Icons";
import {
  ReadOnlyChips,
  ReadOnlyField,
  ReadOnlyValue,
} from "@/components/ui/ReadOnlyField";
import { ApiError } from "@/lib/api";
import { downloadStudentDocument } from "@/lib/student-profile-api";
import {
  TIME_SLOTS,
  WEEKDAYS,
  type StudentProfileValues,
} from "@/lib/student-profile";

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

function DisplayText({ value }: { value: string }) {
  const trimmed = value.trim();
  return <ReadOnlyValue>{trimmed ? trimmed : "—"}</ReadOnlyValue>;
}

function DocumentDownload({
  label,
  fileId,
  fileName,
}: {
  label: string;
  fileId: string | null;
  fileName: string | null;
}) {
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function onDownload() {
    if (!fileId || !fileName) return;
    setBusy(true);
    setError(null);
    try {
      await downloadStudentDocument(fileId, fileName);
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Could not download the file.",
      );
    } finally {
      setBusy(false);
    }
  }

  return (
    <ReadOnlyField label={label}>
      {fileId && fileName ? (
        <div className="flex flex-col gap-2">
          <div className="field-display flex flex-wrap items-center justify-between gap-3">
            <p className="min-w-0 flex-1 truncate">{fileName}</p>
            <Button
              type="button"
              variant="outline"
              size="sm"
              className="shrink-0"
              disabled={busy}
              onClick={() => void onDownload()}
            >
              <IconDownload />
              {busy ? "Downloading…" : "Download"}
            </Button>
          </div>
          {error ? (
            <p className="text-sm font-medium text-secondary-deep" role="alert">
              {error}
            </p>
          ) : null}
        </div>
      ) : (
        <ReadOnlyValue>Not uploaded</ReadOnlyValue>
      )}
    </ReadOnlyField>
  );
}

export function StudentProfileReadonly({
  values,
}: {
  values: StudentProfileValues;
}) {
  const languages = [
    ...values.languages,
    ...(values.otherLanguages.trim() ? [values.otherLanguages.trim()] : []),
  ];

  return (
    <div className="space-y-6">
      <Section title="General information">
        <div className="grid gap-5 sm:grid-cols-2">
          <ReadOnlyField label="First name">
            <DisplayText value={values.firstName} />
          </ReadOnlyField>
          <ReadOnlyField label="Last name">
            <DisplayText value={values.lastName} />
          </ReadOnlyField>
          <ReadOnlyField label="Email">
            <DisplayText value={values.email} />
          </ReadOnlyField>
          <ReadOnlyField label="Gender">
            <DisplayText value={values.gender} />
          </ReadOnlyField>
          <ReadOnlyField label="Mobile number">
            <DisplayText value={values.mobileNumber} />
          </ReadOnlyField>
          <ReadOnlyField label="Degree">
            <DisplayText value={values.degree} />
          </ReadOnlyField>
          <ReadOnlyField label="Expected graduation year">
            <DisplayText value={values.expectedGraduationYear} />
          </ReadOnlyField>
          <ReadOnlyField label="Languages">
            <ReadOnlyChips items={languages} />
          </ReadOnlyField>
        </div>
      </Section>

      <Section title="Criteria checklist">
        <ReadOnlyField label="Completed at least 24 credits at AUB">
          <ReadOnlyValue>
            {values.completedCredits === "yes"
              ? "Yes"
              : values.completedCredits === "no"
                ? "No"
                : "—"}
          </ReadOnlyValue>
        </ReadOnlyField>
        <ReadOnlyField label="Cumulative average">
          <DisplayText value={values.cumulativeAverage} />
        </ReadOnlyField>
        <DocumentDownload
          label="Transcript"
          fileId={values.transcriptFileId}
          fileName={values.transcriptFileName}
        />
        <DocumentDownload
          label="CITI certification"
          fileId={values.citiFileId}
          fileName={values.citiFileName}
        />
      </Section>

      <Section title="Research interests">
        <ReadOnlyField label="Research topic(s)">
          <ReadOnlyChips items={values.researchTopics} />
        </ReadOnlyField>
        <ReadOnlyField label="Publications">
          {values.publications.trim() ? (
            <ReadOnlyValue>
              <p className="whitespace-pre-wrap">{values.publications}</p>
            </ReadOnlyValue>
          ) : (
            <ReadOnlyValue>—</ReadOnlyValue>
          )}
        </ReadOnlyField>
      </Section>

      <Section title="Time availability">
        <Text as="p" size="2" className="!text-muted">
          Typical weekly availability for volunteering.
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
                      const on = entry?.slots.includes(slot) ?? false;
                      return (
                        <td key={slot} className="px-2 py-3 text-center">
                          <span
                            className={`availability-slot${on ? " is-on" : ""}`}
                            aria-label={`${day} ${slot}: ${on ? "available" : "unavailable"}`}
                          >
                            {on ? "Available" : "—"}
                          </span>
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
    </div>
  );
}
