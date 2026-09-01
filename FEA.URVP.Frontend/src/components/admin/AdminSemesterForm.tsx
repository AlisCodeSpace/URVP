"use client";

import { useCallback, useEffect, useId, useState } from "react";
import { useRouter } from "next/navigation";
import { AdminFormField } from "@/components/admin/AdminFormField";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { ApiError } from "@/lib/api";
import {
  createSemester,
  formatWindowDate,
  getSemester,
  setApplicationWindow,
  updateSemester,
  type SemesterDto,
} from "@/lib/semesters-api";

type FormValues = {
  name: string;
  description: string;
  applicationWindowStart: string;
  applicationWindowEnd: string;
};

const emptyValues: FormValues = {
  name: "",
  description: "",
  applicationWindowStart: "",
  applicationWindowEnd: "",
};

function toLocalDatetimeInput(iso: string | null | undefined): string {
  if (!iso) return "";
  const d = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function toValues(dto: SemesterDto): FormValues {
  return {
    name: dto.name,
    description: dto.description ?? "",
    applicationWindowStart: toLocalDatetimeInput(dto.applicationWindowStart),
    applicationWindowEnd: toLocalDatetimeInput(dto.applicationWindowEnd),
  };
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
  const startId = useId();
  const endId = useId();

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

    const start = values.applicationWindowStart
      ? new Date(values.applicationWindowStart).toISOString()
      : null;
    const end = values.applicationWindowEnd
      ? new Date(values.applicationWindowEnd).toISOString()
      : null;

    if (start && end && new Date(end) <= new Date(start)) {
      setError("Application window end must be after the start date.");
      return;
    }

    setSaving(true);
    setError(null);
    try {
      const payload = {
        name: values.name.trim(),
        description: values.description.trim() || null,
      };

      const saved =
        isEdit && semesterId
          ? await updateSemester(semesterId, payload)
          : await createSemester(payload);

      // Persist application window dates if they differ from the saved DTO.
      const savedStart = saved.applicationWindowStart ?? null;
      const savedEnd = saved.applicationWindowEnd ?? null;
      if (start !== savedStart || end !== savedEnd) {
        await setApplicationWindow(saved.id, {
          applicationWindowStart: start,
          applicationWindowEnd: end,
        });
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
    return <p className="admin-users-status">Loading semester…</p>;
  }

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title={isEdit ? "Edit semester" : "New semester"}
        description="Set the semester name and configure the student application window."
      />

      <form className="mt-6 grid max-w-3xl gap-5" onSubmit={onSubmit} noValidate>
        {error ? (
          <p className="admin-users-banner is-error" role="alert">
            {error}
          </p>
        ) : null}

        {/* ── Current status summary (edit only) ── */}
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
            Application Window
          </legend>
          <p
            style={{
              margin: "0 0 1rem",
              fontSize: "0.82rem",
              color: "var(--muted)",
            }}
          >
            Students may submit applications only when the cycle is active and
            the current date falls within this window. Typically mid-September
            to end of September.
          </p>
          <div className="grid gap-5 sm:grid-cols-2">
            <AdminFormField
              id={startId}
              label="Opens"
              hint="Leave blank if not yet scheduled."
            >
              <input
                id={startId}
                type="datetime-local"
                className="field-input"
                value={values.applicationWindowStart}
                onChange={(e) =>
                  setField("applicationWindowStart", e.target.value)
                }
              />
            </AdminFormField>

            <AdminFormField
              id={endId}
              label="Closes"
              hint="Leave blank to keep the window open indefinitely."
            >
              <input
                id={endId}
                type="datetime-local"
                className="field-input"
                value={values.applicationWindowEnd}
                onChange={(e) =>
                  setField("applicationWindowEnd", e.target.value)
                }
              />
            </AdminFormField>
          </div>
          {currentDto?.applicationWindowStart ? (
            <p
              style={{
                margin: "0.85rem 0 0",
                fontSize: "0.8rem",
                color: "var(--muted)",
              }}
            >
              Current window: {formatWindowDate(currentDto.applicationWindowStart)}
              {currentDto.applicationWindowEnd
                ? ` → ${formatWindowDate(currentDto.applicationWindowEnd)}`
                : " → open"}
            </p>
          ) : null}
        </fieldset>

        <div className="flex flex-wrap gap-3">
          <Button
            type="submit"
            variant="primary"
            size="md"
            disabled={saving}
          >
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
