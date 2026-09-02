"use client";

import { useCallback, useEffect, useId, useState } from "react";
import { useRouter } from "next/navigation";
import { AdminFormField } from "@/components/admin/AdminFormField";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { AdminPosterField } from "@/components/admin/AdminPosterField";
import { Button } from "@/components/ui/Button";
import { AdminFormSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import {
  createWorkshop,
  getWorkshop,
  updateWorkshop,
  uploadWorkshopPoster,
  workshopPosterUrl,
  type WorkshopDto,
} from "@/lib/workshops-api";

type WorkshopFormValues = {
  title: string;
  date: string;
  time: string;
  location: string;
  description: string;
  registrationUrl: string;
  posterAlt: string;
  posterFileId: string | null;
};

const emptyValues: WorkshopFormValues = {
  title: "",
  date: "",
  time: "",
  location: "",
  description: "",
  registrationUrl: "",
  posterAlt: "",
  posterFileId: null,
};

function toValues(dto: WorkshopDto): WorkshopFormValues {
  return {
    title: dto.title,
    date: dto.date,
    time: dto.time ?? "",
    location: dto.location ?? "",
    description: dto.description,
    registrationUrl: dto.registrationUrl,
    posterAlt: dto.posterAlt ?? "",
    posterFileId: dto.posterFileId ?? null,
  };
}

export function AdminWorkshopForm({ workshopId }: { workshopId?: string }) {
  const router = useRouter();
  const isEdit = Boolean(workshopId);
  const [values, setValues] = useState<WorkshopFormValues>(emptyValues);
  const [posterFile, setPosterFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [loading, setLoading] = useState(isEdit);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const titleId = useId();
  const dateId = useId();
  const timeId = useId();
  const locationId = useId();
  const descriptionId = useId();
  const urlId = useId();

  const load = useCallback(async () => {
    if (!workshopId) return;
    setLoading(true);
    setError(null);
    try {
      const item = await getWorkshop(workshopId);
      const next = toValues(item);
      setValues(next);
      setPreviewUrl(workshopPosterUrl(next.posterFileId) ?? null);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load workshop.");
    } finally {
      setLoading(false);
    }
  }, [workshopId]);

  useEffect(() => {
    void load();
  }, [load]);

  useEffect(() => {
    if (!posterFile) return;
    const url = URL.createObjectURL(posterFile);
    setPreviewUrl(url);
    return () => URL.revokeObjectURL(url);
  }, [posterFile]);

  function setField<K extends keyof WorkshopFormValues>(
    key: K,
    value: WorkshopFormValues[K],
  ) {
    setValues((prev) => ({ ...prev, [key]: value }));
  }

  function onFileChange(file: File | null) {
    setPosterFile(file);
    if (file) return;
    setField("posterFileId", null);
    setPreviewUrl(null);
  }

  async function onSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!values.title.trim() || !values.date.trim() || !values.description.trim() || !values.registrationUrl.trim()) {
      setError("Please fill in all required fields.");
      return;
    }

    setSaving(true);
    setError(null);
    try {
      const payload = {
        title: values.title.trim(),
        date: values.date.trim(),
        time: values.time.trim() || null,
        location: values.location.trim() || null,
        description: values.description.trim(),
        registrationUrl: values.registrationUrl.trim(),
        posterAlt: values.posterAlt.trim() || null,
        posterFileId: values.posterFileId,
      };

      const saved =
        isEdit && workshopId
          ? await updateWorkshop(workshopId, payload)
          : await createWorkshop(payload);

      if (posterFile) {
        await uploadWorkshopPoster(saved.id, posterFile);
      }

      router.push("/admin/workshops");
      router.refresh();
    } catch (err) {
      setError(
        err instanceof ApiError
          ? (err.errors[0] ?? err.message)
          : "Could not save this workshop.",
      );
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <div className="admin-panel admin-panel--wide">
        <AdminPageHeader
          title={isEdit ? "Edit workshop" : "New workshop"}
          description="Same fields as the Workshops page, plus a 3:2 card photo."
        />
        <AdminFormSkeleton fields={7} />
      </div>
    );
  }

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title={isEdit ? "Edit workshop" : "New workshop"}
        description="Same fields as the Workshops page, plus a 3:2 card photo."
      />

      <form className="mt-6 grid max-w-3xl gap-5" onSubmit={onSubmit} noValidate>
        {error ? (
          <p className="admin-users-banner is-error" role="alert">
            {error}
          </p>
        ) : null}

        <AdminFormField id={titleId} label="Title" required>
          <input
            id={titleId}
            className="field-input"
            value={values.title}
            onChange={(e) => setField("title", e.target.value)}
            required
          />
        </AdminFormField>

        <div className="grid gap-5 sm:grid-cols-2">
          <AdminFormField
            id={dateId}
            label="Date"
            required
            hint='Display date, e.g. "Sep 5, 2025".'
          >
            <input
              id={dateId}
              className="field-input"
              value={values.date}
              onChange={(e) => setField("date", e.target.value)}
              required
            />
          </AdminFormField>
          <AdminFormField id={timeId} label="Time" hint='e.g. "4:00 – 5:00 PM"'>
            <input
              id={timeId}
              className="field-input"
              value={values.time}
              onChange={(e) => setField("time", e.target.value)}
            />
          </AdminFormField>
        </div>

        <AdminFormField id={locationId} label="Location">
          <input
            id={locationId}
            className="field-input"
            value={values.location}
            onChange={(e) => setField("location", e.target.value)}
            placeholder="Online · Zoom"
          />
        </AdminFormField>

        <AdminFormField id={descriptionId} label="Description" required>
          <textarea
            id={descriptionId}
            className="field-textarea"
            rows={4}
            value={values.description}
            onChange={(e) => setField("description", e.target.value)}
            required
          />
        </AdminFormField>

        <AdminFormField id={urlId} label="Registration URL" required>
          <input
            id={urlId}
            type="url"
            className="field-input"
            value={values.registrationUrl}
            onChange={(e) => setField("registrationUrl", e.target.value)}
            placeholder="https://forms.gle/…"
            required
          />
        </AdminFormField>

        <AdminPosterField
          previewUrl={previewUrl}
          alt={values.posterAlt}
          onAltChange={(value) => setField("posterAlt", value)}
          onFileChange={onFileChange}
          fileName={posterFile?.name}
        />

        <div className="flex flex-wrap gap-3">
          <Button type="submit" variant="primary" size="md" disabled={saving}>
            {saving ? "Saving…" : isEdit ? "Save workshop" : "Publish workshop"}
          </Button>
          <Button href="/admin/workshops" variant="outline" size="md">
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
}
