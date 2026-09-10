"use client";

import { useCallback, useEffect, useId, useState, type FormEvent } from "react";
import { AdminFormField } from "@/components/admin/AdminFormField";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { IconPlus, IconTrash } from "@/components/ui/Icons";
import { AdminFormSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import {
  HOME_INTRO_DESCRIPTION_MAX_LENGTH,
  HOME_INTRO_HEADLINE_MAX_LENGTH,
  HOME_INTRO_KEY_POINT_MAX_LENGTH,
  HOME_INTRO_MAX_KEY_POINTS,
  getHomeIntro,
  updateHomeIntro,
  type HomeIntroDto,
} from "@/lib/home-intro-api";

export function AdminHomeIntroView() {
  const headlineId = useId();
  const descriptionId = useId();

  const [headline, setHeadline] = useState("");
  const [description, setDescription] = useState("");
  const [keyPoints, setKeyPoints] = useState<string[]>([""]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);

  const apply = useCallback((dto: HomeIntroDto) => {
    setHeadline(dto.headline);
    setDescription(dto.description);
    setKeyPoints(dto.keyPoints.length > 0 ? dto.keyPoints : [""]);
  }, []);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    setSaved(false);
    try {
      apply(await getHomeIntro());
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Failed to load home intro.",
      );
    } finally {
      setLoading(false);
    }
  }, [apply]);

  useEffect(() => {
    void load();
  }, [load]);

  function setKeyPoint(index: number, value: string) {
    setKeyPoints((prev) => prev.map((point, i) => (i === index ? value : point)));
  }

  function addKeyPoint() {
    setKeyPoints((prev) =>
      prev.length >= HOME_INTRO_MAX_KEY_POINTS ? prev : [...prev, ""],
    );
  }

  function removeKeyPoint(index: number) {
    setKeyPoints((prev) => {
      const next = prev.filter((_, i) => i !== index);
      return next.length > 0 ? next : [""];
    });
  }

  async function onSubmit(event: FormEvent) {
    event.preventDefault();
    const trimmedPoints = keyPoints.map((point) => point.trim()).filter(Boolean);
    if (trimmedPoints.length > HOME_INTRO_MAX_KEY_POINTS) {
      setError(`No more than ${HOME_INTRO_MAX_KEY_POINTS} key points are allowed.`);
      return;
    }

    setSaving(true);
    setError(null);
    setSaved(false);
    try {
      const next = await updateHomeIntro({
        headline: headline.trim(),
        description: description.trim(),
        keyPoints: trimmedPoints,
      });
      apply(next);
      setSaved(true);
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Failed to save home intro.",
      );
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return (
      <div className="admin-panel admin-panel--wide">
        <AdminPageHeader
          title="Home intro"
          description="Headline, description, and key points on the public homepage."
        />
        <AdminFormSkeleton fields={7} />
      </div>
    );
  }

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title="Home intro"
        description="Edit the welcome section below the homepage hero — headline, description, and key information."
        tag={`${keyPoints.filter((point) => point.trim()).length} / ${HOME_INTRO_MAX_KEY_POINTS} key points`}
      />

      {error ? (
        <p className="admin-users-banner is-error" role="alert">
          {error}
        </p>
      ) : null}
      {saved ? (
        <p className="admin-users-banner" role="status">
          Home intro saved.
        </p>
      ) : null}

      <form className="mt-6 grid max-w-3xl gap-6" onSubmit={onSubmit}>
        <AdminFormField
          id={headlineId}
          label="Headline"
          required
          hint="Main heading in the homepage welcome section."
        >
          <input
            id={headlineId}
            className="field-input"
            value={headline}
            maxLength={HOME_INTRO_HEADLINE_MAX_LENGTH}
            onChange={(e) => setHeadline(e.target.value)}
            required
          />
        </AdminFormField>

        <AdminFormField
          id={descriptionId}
          label="Description"
          required
          hint="Shown beside Key information. Separate paragraphs with a blank line."
        >
          <textarea
            id={descriptionId}
            className="field-textarea"
            rows={10}
            value={description}
            maxLength={HOME_INTRO_DESCRIPTION_MAX_LENGTH}
            onChange={(e) => setDescription(e.target.value)}
            required
          />
        </AdminFormField>

        <div>
          <div className="mb-3 flex flex-wrap items-center justify-between gap-3">
            <p className="field-label mb-0">Key points</p>
            <Button
              type="button"
              variant="outline"
              size="sm"
              disabled={keyPoints.length >= HOME_INTRO_MAX_KEY_POINTS || saving}
              onClick={addKeyPoint}
            >
              <IconPlus />
              Add key point
            </Button>
          </div>
          <p className="field-hint mb-4">
            Up to {HOME_INTRO_MAX_KEY_POINTS} points. Empty lines are ignored when
            you save.
          </p>
          <ul className="grid gap-4">
            {keyPoints.map((point, index) => {
              const fieldId = `${descriptionId}-point-${index}`;
              return (
                <li key={fieldId} className="flex items-start gap-3">
                  <div className="min-w-0 flex-1">
                    <AdminFormField
                      id={fieldId}
                      label={`Point ${index + 1}`}
                    >
                      <textarea
                        id={fieldId}
                        className="field-textarea"
                        rows={2}
                        value={point}
                        maxLength={HOME_INTRO_KEY_POINT_MAX_LENGTH}
                        onChange={(e) => setKeyPoint(index, e.target.value)}
                      />
                    </AdminFormField>
                  </div>
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="mt-7 shrink-0"
                    disabled={saving || (keyPoints.length === 1 && !point.trim())}
                    onClick={() => removeKeyPoint(index)}
                    aria-label={`Remove key point ${index + 1}`}
                  >
                    <IconTrash />
                  </Button>
                </li>
              );
            })}
          </ul>
        </div>

        <div>
          <Button type="submit" variant="primary" size="md" disabled={saving}>
            {saving ? "Saving…" : "Save home intro"}
          </Button>
        </div>
      </form>
    </div>
  );
}
