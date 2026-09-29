"use client";

import { useId, useState, type ReactNode } from "react";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { useRouter } from "next/navigation";
import { AdminFormField } from "@/components/admin/AdminFormField";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { DateField } from "@/components/ui/DateField";
import { AdminFormSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import { fromAppDatetimeInput, toAppDatetimeInput } from "@/lib/datetime";
import {
  createSemester,
  formatScheduleRange,
  getActiveSemester,
  getSemester,
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
  registrationWindowStart: string;
  registrationWindowEnd: string;
};

const emptyValues: FormValues = {
  name: "",
  description: "",
  cycleStart: "",
  cycleEnd: "",
  applicationWindowStart: "",
  applicationWindowEnd: "",
  registrationWindowStart: "",
  registrationWindowEnd: "",
};

function toValues(dto: SemesterDto): FormValues {
  return {
    name: dto.name,
    description: dto.description ?? "",
    cycleStart: toAppDatetimeInput(dto.cycleStart),
    cycleEnd: toAppDatetimeInput(dto.cycleEnd),
    applicationWindowStart: toAppDatetimeInput(dto.applicationWindowStart),
    applicationWindowEnd: toAppDatetimeInput(dto.applicationWindowEnd),
    registrationWindowStart: toAppDatetimeInput(dto.registrationWindowStart),
    registrationWindowEnd: toAppDatetimeInput(dto.registrationWindowEnd),
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
  const cycleQuery = useCancellableQuery(
    async () => {
      if (semesterId) {
        const dto = await getSemester(semesterId);
        return { mode: "edit" as const, dto, blockedBy: null };
      }
      const active = await getActiveSemester();
      return {
        mode: "create" as const,
        dto: null,
        blockedBy: active?.name ?? null,
      };
    },
    [semesterId],
    { fallbackError: "Failed to load URVP cycle." },
  );
  const [values, setValues] = useState<FormValues>(emptyValues);
  const [saving, setSaving] = useState(false);
  const [currentDto, setCurrentDto] = useState<SemesterDto | null>(null);
  const [createBlockedByActive, setCreateBlockedByActive] = useState<string | null>(
    null,
  );
  const [seenCycle, setSeenCycle] = useState(cycleQuery.data);
  const { loading, error, setError } = cycleQuery;

  if (cycleQuery.data !== seenCycle) {
    setSeenCycle(cycleQuery.data);
    setCreateBlockedByActive(
      cycleQuery.data?.mode === "create" ? cycleQuery.data.blockedBy : null,
    );
    if (cycleQuery.data?.mode === "edit" && cycleQuery.data.dto) {
      setCurrentDto(cycleQuery.data.dto);
      setValues(toValues(cycleQuery.data.dto));
    }
  }

  const nameId = useId();
  const descId = useId();
  const cycleStartId = useId();
  const cycleEndId = useId();
  const windowStartId = useId();
  const windowEndId = useId();
  const registrationStartId = useId();
  const registrationEndId = useId();
  const readOnly = Boolean(isEdit && currentDto?.hasEnded);

  function setField<K extends keyof FormValues>(key: K, value: FormValues[K]) {
    setValues((prev) => ({ ...prev, [key]: value }));
  }

  async function onSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (readOnly || createBlockedByActive) return;
    if (!values.name.trim()) {
      setError("Cycle name is required.");
      return;
    }

    const cycleStart = fromAppDatetimeInput(values.cycleStart);
    const cycleEnd = fromAppDatetimeInput(values.cycleEnd);
    const windowStart = fromAppDatetimeInput(values.applicationWindowStart);
    const windowEnd = fromAppDatetimeInput(values.applicationWindowEnd);
    const registrationStart = fromAppDatetimeInput(values.registrationWindowStart);
    const registrationEnd = fromAppDatetimeInput(values.registrationWindowEnd);

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
    if (
      registrationStart &&
      registrationEnd &&
      new Date(registrationEnd) <= new Date(registrationStart)
    ) {
      setError("Registration window end must be after the start date.");
      return;
    }
    if (cycleStart && registrationStart && new Date(registrationStart) < new Date(cycleStart)) {
      setError("The registration window cannot open before the academic cycle starts.");
      return;
    }
    if (cycleEnd && registrationEnd && new Date(registrationEnd) > new Date(cycleEnd)) {
      setError("The registration window cannot close after the academic cycle ends.");
      return;
    }
    if (registrationStart && windowStart) {
      const registrationUntil = registrationEnd
        ? new Date(registrationEnd).getTime()
        : Number.POSITIVE_INFINITY;
      const applicationUntil = windowEnd
        ? new Date(windowEnd).getTime()
        : Number.POSITIVE_INFINITY;
      if (
        new Date(registrationStart).getTime() < applicationUntil &&
        new Date(windowStart).getTime() < registrationUntil
      ) {
        setError("The registration window and the application window cannot overlap.");
        return;
      }
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
        registrationWindowStart: registrationStart,
        registrationWindowEnd: registrationEnd,
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
          : "Could not save this URVP cycle.",
      );
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <div className="admin-panel admin-panel--wide">
        <AdminPageHeader
          title={isEdit ? "Edit URVP cycle" : "New URVP cycle"}
          description="Schedule the URVP cycle, registration window, and application window."
          backHref="/admin/semesters"
          backLabel="Back to URVP cycles"
        />
        <AdminFormSkeleton fields={8} />
      </div>
    );
  }

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title={
          readOnly ? "View URVP cycle" : isEdit ? "Edit URVP cycle" : "New URVP cycle"
        }
        description={
          readOnly
            ? "This cycle has ended. It is kept as history and cannot be changed."
            : "Set start and end dates so each period closes automatically — or leave an end blank and close it instantly from the URVP Cycles list. Registration and applications can be opened only while the cycle is active, and they cannot overlap."
        }
        backHref="/admin/semesters"
        backLabel="Back to URVP cycles"
      />

      <form className="mt-6 grid max-w-3xl gap-5" onSubmit={onSubmit} noValidate>
        {createBlockedByActive ? (
          <p className="admin-users-banner is-error" role="alert">
            “{createBlockedByActive}” is still active. End that cycle before creating a
            new one.
          </p>
        ) : null}
        {error ? (
          <p className="admin-users-banner is-error" role="alert">
            {error}
          </p>
        ) : null}

        {isEdit && currentDto ? (
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fit, minmax(10rem, 1fr))",
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
                Registration
              </p>
              <p style={{ margin: 0, fontSize: "0.9rem", color: "var(--foreground)" }}>
                {currentDto.isRegistrationWindowOpen ? "Open" : "Closed"}
              </p>
              <p
                style={{
                  margin: "0.25rem 0 0",
                  fontSize: "0.8rem",
                  color: "var(--muted)",
                }}
              >
                {formatScheduleRange(
                  currentDto.registrationWindowStart,
                  currentDto.registrationWindowEnd,
                )}
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
            disabled={readOnly || Boolean(createBlockedByActive)}
          />
        </AdminFormField>

        <AdminFormField id={descId} label="Description" hint="Internal notes, visible to admins only.">
          <textarea
            id={descId}
            className="field-textarea"
            rows={2}
            value={values.description}
            onChange={(e) => setField("description", e.target.value)}
            disabled={readOnly || Boolean(createBlockedByActive)}
          />
        </AdminFormField>

        <ScheduleFieldset
          legend="Academic Cycle"
          description="Nothing can be posted, edited, or ranked while this cycle is inactive. It opens at the start date and closes automatically at the end date. Ending it also closes registration and applications."
        >
          <div className="grid gap-5 sm:grid-cols-2">
            <AdminFormField
              id={cycleStartId}
              label="Starts"
              hint={
                isEdit
                  ? "The start date cannot be changed after the cycle is created."
                  : "Leave blank until the cycle is scheduled."
              }
            >
              <DateField
                id={cycleStartId}
                includeTime
                placeholder="Select start date"
                value={values.cycleStart}
                onChange={(next) => setField("cycleStart", next)}
                disabled={readOnly || Boolean(createBlockedByActive) || isEdit}
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
                disabled={readOnly || Boolean(createBlockedByActive)}
              />
            </AdminFormField>
          </div>
        </ScheduleFieldset>

        <ScheduleFieldset
          legend="Registration Window"
          description="Faculty can post and edit projects, and students can create or update profiles. Students cannot view or rank projects. This window can be open only while the cycle is active, and it cannot overlap applications."
        >
          <div className="grid gap-5 sm:grid-cols-2">
            <AdminFormField
              id={registrationStartId}
              label="Opens"
              hint={
                isEdit
                  ? "The opening date cannot be changed after the cycle is created."
                  : "Leave blank if not yet scheduled."
              }
            >
              <DateField
                id={registrationStartId}
                includeTime
                placeholder="Select start date"
                value={values.registrationWindowStart}
                onChange={(next) => setField("registrationWindowStart", next)}
                disabled={readOnly || Boolean(createBlockedByActive) || isEdit}
              />
            </AdminFormField>
            <AdminFormField
              id={registrationEndId}
              label="Closes"
              hint="Required for automatic close. Leave blank to close it manually."
            >
              <DateField
                id={registrationEndId}
                includeTime
                placeholder="Select end date"
                value={values.registrationWindowEnd}
                onChange={(next) => setField("registrationWindowEnd", next)}
                disabled={readOnly || Boolean(createBlockedByActive)}
              />
            </AdminFormField>
          </div>
        </ScheduleFieldset>

        <ScheduleFieldset
          legend="Application Window"
          description="Students rank projects and faculty rank students. Profiles and projects stay locked, and matching stays locked. This window can be open only while the cycle is active, and it cannot overlap registration."
        >
          <div className="grid gap-5 sm:grid-cols-2">
            <AdminFormField
              id={windowStartId}
              label="Opens"
              hint={
                isEdit
                  ? "The opening date cannot be changed after the cycle is created."
                  : "Leave blank if not yet scheduled."
              }
            >
              <DateField
                id={windowStartId}
                includeTime
                placeholder="Select start date"
                value={values.applicationWindowStart}
                onChange={(next) => setField("applicationWindowStart", next)}
                disabled={readOnly || Boolean(createBlockedByActive) || isEdit}
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
                disabled={readOnly || Boolean(createBlockedByActive)}
              />
            </AdminFormField>
          </div>
        </ScheduleFieldset>

        <div className="flex flex-wrap gap-3">
          {readOnly ? null : (
            <Button
              type="submit"
              variant="primary"
              size="md"
              disabled={saving || Boolean(createBlockedByActive)}
            >
              {saving
                ? "Saving…"
                : isEdit
                  ? "Save changes"
                  : "Create cycle"}
            </Button>
          )}
          <Button href="/admin/semesters" variant="outline" size="md">
            {readOnly ? "Back" : "Cancel"}
          </Button>
        </div>
      </form>
    </div>
  );
}
