"use client";

import { useCallback, useEffect, useId, useState, type ReactNode } from "react";
import { useRouter } from "next/navigation";
import { AdminFormField } from "@/components/admin/AdminFormField";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { DateField } from "@/components/ui/DateField";
import { AdminFormSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import {
  createSemester,
  formatScheduleRange,
  getSemester,
  parseApiDate,
  updateSemester,
  type SemesterDto,
} from "@/lib/semesters-api";

type FormValues = {
  name: string;
  description: string;
  cycleStart: string;
  cycleEnd: string;
  applicationWindowStart: string;
  applicationWindowEnd: string;
};

const emptyValues: FormValues = {
  name: "",
  description: "",
  cycleStart: "",
  cycleEnd: "",
  applicationWindowStart: "",
  applicationWindowEnd: "",
};

function toLocalDatetimeInput(iso: string | null | undefined): string {
  if (!iso) return "";
  const d = parseApiDate(iso);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function toIsoOrNull(local: string): string | null {
  return local ? new Date(local).toISOString() : null;
}

function toValues(dto: SemesterDto): FormValues {
  return {
    name: dto.name,
    description: dto.description ?? "",
    cycleStart: toLocalDatetimeInput(dto.cycleStart),
    cycleEnd: toLocalDatetimeInput(dto.cycleEnd),
    applicationWindowStart: toLocalDatetimeInput(dto.applicationWindowStart),
    applicationWindowEnd: toLocalDatetimeInput(dto.applicationWindowEnd),
  };
}

function ScheduleFieldset({
  legend,
  description,
  children,
}: {
  legend: string;
  description: string;
  children: ReactNode;
}) {
  return (
    <fieldset
      style={{
        border: "1.5px solid color-mix(in srgb, var(--primary) 16%, transparent)",
        borderRadius: "0.5rem",
        padding: "1rem 1rem 1.1rem",
        margin: 0,
      }}
    >
      <legend
        style={{
          padding: "0 0.4rem",
          fontSize: "0.85rem",
          fontWeight: 600,
          color: "var(--primary-deep)",
        }}
      >
        {legend}
      </legend>
      <p
        style={{
          margin: "0 0 1rem",
          fontSize: "0.82rem",
          color: "var(--muted)",
        }}
      >
        {description}
      </p>
      {children}
    </fieldset>
  );
}

export function AdminSemesterForm({ semesterId }: { semesterId?: string }) {
  const router = useRouter();
  const isEdit = Boolean(semesterId);
  const [values, setValues] = useState<FormValues>(emptyValues);
  const [loading, setLoading] = useState(isEdit);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [currentDto, setCurrentDto] = useState<SemesterDto | null>(null);

  const nameId = useId();
  const descId = useId();
  const cycleStartId = useId();
  const cycleEndId = useId();
  const windowStartId = useId();
  const windowEndId = useId();

  const load = useCallback(async () => {
    if (!semesterId) return;
    setLoading(true);
    setError(null);
    try {
      const dto = await getSemester(semesterId);
      setCurrentDto(dto);
      setValues(toValues(dto));
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Failed to load semester.",
      );
    } finally {
      setLoading(false);
    }
  }, [semesterId]);

  useEffect(() => {
    void load();
  }, [load]);

  function setField<K extends keyof FormValues>(key: K, value: FormValues[K]) {
    setValues((prev) => ({ ...prev, [key]: value }));
  }

  async function onSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!values.name.trim()) {
      setError("Semester name is required.");
      return;
    }

    const cycleStart = toIsoOrNull(values.cycleStart);
    const cycleEnd = toIsoOrNull(values.cycleEnd);
    const windowStart = toIsoOrNull(values.applicationWindowStart);
    const windowEnd = toIsoOrNull(values.applicationWindowEnd);

    if (cycleStart && cycleEnd && new Date(cycleEnd) <= new Date(cycleStart)) {
      setError("Academic cycle end must be after the start date.");
      return;
    }
    if (windowStart && windowEnd && new Date(windowEnd) <= new Date(windowStart)) {
      setError("Application window end must be after the start date.");
      return;
    }
    if (cycleStart && windowStart && new Date(windowStart) < new Date(cycleStart)) {
      setError("The application window cannot open before the academic cycle starts.");
      return;
    }
    if (cycleEnd && windowEnd && new Date(windowEnd) > new Date(cycleEnd)) {
      setError("The application window cannot close after the academic cycle ends.");
      return;
    }

    setSaving(true);
    setError(null);
    try {
      const payload = {
        name: values.name.trim(),
        description: values.description.trim() || null,
        cycleStart,
        cycleEnd,
        applicationWindowStart: windowStart,
        applicationWindowEnd: windowEnd,
      };

      if (isEdit && semesterId) {
        await updateSemester(semesterId, payload);
      } else {
        await createSemester(payload);
      }

      router.push("/admin/semesters");
      router.refresh();
    } catch (err) {
      setError(
        err instanceof ApiError
          ? (err.errors?.[0] ?? err.message)
          : "Could not save this semester.",
      );
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <div className="admin-panel admin-panel--wide">
        <AdminPageHeader
          title={isEdit ? "Edit semester" : "New semester"}
          description="Schedule the academic cycle and student application window."
        />
        <AdminFormSkeleton fields={6} />
      </div>
    );
  }

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title={isEdit ? "Edit semester" : "New semester"}
        description="Set start and end dates so each period closes automatically — or leave an end blank and close it instantly from the semesters list. You can edit dates at any time to extend or shorten a period."
      />

      <form className="mt-6 grid max-w-3xl gap-5" onSubmit={onSubmit} noValidate>
        {error ? (
          <p className="admin-users-banner is-error" role="alert">
            {error}
          </p>
        ) : null}

        {isEdit && currentDto ? (
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "1fr 1fr",
              gap: "0.75rem",
              padding: "0.9rem 1rem",
              border:
                "1.5px solid color-mix(in srgb, var(--primary) 16%, transparent)",
              borderRadius: "0.5rem",
              background: "color-mix(in srgb, var(--primary) 4%, white)",
            }}
          >
            <div>
              <p
                style={{
                  margin: "0 0 0.2rem",
                  fontSize: "0.78rem",
                  textTransform: "uppercase",
                  letterSpacing: "0.05em",
                  color: "var(--muted)",
                  fontWeight: 600,
                }}
              >
                Cycle
              </p>
              <p style={{ margin: 0, fontSize: "0.9rem", color: "var(--foreground)" }}>
                {currentDto.isActive ? "Active" : "Inactive"}
              </p>
              <p
                style={{
                  margin: "0.25rem 0 0",
                  fontSize: "0.8rem",
                  color: "var(--muted)",
                }}
              >
                {formatScheduleRange(currentDto.cycleStart, currentDto.cycleEnd)}
              </p>
            </div>
            <div>
              <p
                style={{
                  margin: "0 0 0.2rem",
                  fontSize: "0.78rem",
                  textTransform: "uppercase",
                  letterSpacing: "0.05em",
                  color: "var(--muted)",
                  fontWeight: 600,
                }}
              >
                Applications
              </p>
              <p style={{ margin: 0, fontSize: "0.9rem", color: "var(--foreground)" }}>
                {currentDto.isApplicationWindowOpen ? "Open" : "Closed"}
              </p>
              <p
                style={{
                  margin: "0.25rem 0 0",
                  fontSize: "0.8rem",
                  color: "var(--muted)",
                }}
              >
                {formatScheduleRange(
                  currentDto.applicationWindowStart,
                  currentDto.applicationWindowEnd,
                )}
              </p>
            </div>
          </div>
        ) : null}

        <AdminFormField id={nameId} label="Name" required hint='e.g. "Fall 2025–26"'>
          <input
            id={nameId}
            className="field-input"
            value={values.name}
            onChange={(e) => setField("name", e.target.value)}
            placeholder="Fall 2025–26"
            required
          />
        </AdminFormField>

        <AdminFormField id={descId} label="Description" hint="Internal notes, visible to admins only.">
          <textarea
            id={descId}
            className="field-textarea"
            rows={2}
            value={values.description}
            onChange={(e) => setField("description", e.target.value)}
          />
        </AdminFormField>

        <ScheduleFieldset
          legend="Academic Cycle"
          description="Projects are visible while this cycle is running. It opens at the start date and closes automatically at the end date. You can edit these dates later to extend or shorten the cycle, or end it instantly from the semesters list."
        >
          <div className="grid gap-5 sm:grid-cols-2">
            <AdminFormField
              id={cycleStartId}
              label="Starts"
              hint="Leave blank until the cycle is scheduled."
            >
              <DateField
                id={cycleStartId}
                includeTime
                placeholder="Select start date"
                value={values.cycleStart}
                onChange={(next) => setField("cycleStart", next)}
              />
            </AdminFormField>
            <AdminFormField
              id={cycleEndId}
              label="Ends"
              hint="Required for automatic close. Leave blank to close it manually."
            >
              <DateField
                id={cycleEndId}
                includeTime
                placeholder="Select end date"
                value={values.cycleEnd}
                onChange={(next) => setField("cycleEnd", next)}
              />
            </AdminFormField>
          </div>
        </ScheduleFieldset>

        <ScheduleFieldset
          legend="Application Window"
          description="Students may apply only while the cycle is running and the current time is inside this window. The window closes automatically at the end date. Edit the dates to extend or shorten it, or close it instantly from the semesters list."
        >
          <div className="grid gap-5 sm:grid-cols-2">
            <AdminFormField
              id={windowStartId}
              label="Opens"
              hint="Leave blank if not yet scheduled."
            >
              <DateField
                id={windowStartId}
                includeTime
                placeholder="Select start date"
                value={values.applicationWindowStart}
                onChange={(next) => setField("applicationWindowStart", next)}
              />
            </AdminFormField>
            <AdminFormField
              id={windowEndId}
              label="Closes"
              hint="Required for automatic close. Leave blank to close it manually."
            >
              <DateField
                id={windowEndId}
                includeTime
                placeholder="Select end date"
                value={values.applicationWindowEnd}
                onChange={(next) => setField("applicationWindowEnd", next)}
              />
            </AdminFormField>
          </div>
        </ScheduleFieldset>

        <div className="flex flex-wrap gap-3">
          <Button type="submit" variant="primary" size="md" disabled={saving}>
            {saving
              ? "Saving…"
              : isEdit
                ? "Save changes"
                : "Create semester"}
          </Button>
          <Button href="/admin/semesters" variant="outline" size="md">
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
}
