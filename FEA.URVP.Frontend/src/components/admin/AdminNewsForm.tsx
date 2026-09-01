"use client";

import { useCallback, useEffect, useId, useState } from "react";
import { useRouter } from "next/navigation";
import { AdminFormField } from "@/components/admin/AdminFormField";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { ApiError } from "@/lib/api";
import {
  NEWS_CATEGORIES,
  createNews,
  getNewsById,
  updateNews,
  type NewsArticleDto,
} from "@/lib/news-api";

type NewsFormValues = {
  title: string;
  slug: string;
  excerpt: string;
  category: string;
  author: string;
  ticker: string;
  body: string;
  publishedAt: string;
  featured: boolean;
};

const emptyValues: NewsFormValues = {
  title: "",
  slug: "",
  excerpt: "",
  category: "Announcement",
  author: "URVP Office",
  ticker: "",
  body: "",
  publishedAt: new Date().toISOString().slice(0, 10),
  featured: false,
};

function toValues(dto: NewsArticleDto): NewsFormValues {
  return {
    title: dto.title,
    slug: dto.slug,
    excerpt: dto.excerpt,
    category: dto.category,
    author: dto.author,
    ticker: dto.ticker,
    body: dto.body.join("\n\n"),
    publishedAt: dto.publishedAt.slice(0, 10),
    featured: dto.featured,
  };
}

function toPayload(values: NewsFormValues) {
  const body = values.body
    .split(/\n\s*\n/)
    .map((p) => p.trim())
    .filter(Boolean);

  return {
    title: values.title.trim(),
    slug: values.slug.trim() || undefined,
    excerpt: values.excerpt.trim(),
    category: values.category.trim(),
    author: values.author.trim(),
    ticker: values.ticker.trim(),
    body,
    publishedAt: values.publishedAt,
    featured: values.featured,
  };
}

export function AdminNewsForm({ newsId }: { newsId?: string }) {
  const router = useRouter();
  const isEdit = Boolean(newsId);
  const [values, setValues] = useState<NewsFormValues>(emptyValues);
  const [loading, setLoading] = useState(isEdit);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const titleId = useId();
  const slugId = useId();
  const excerptId = useId();
  const categoryId = useId();
  const authorId = useId();
  const tickerId = useId();
  const bodyId = useId();
  const dateId = useId();
  const featuredId = useId();

  const load = useCallback(async () => {
    if (!newsId) return;
    setLoading(true);
    setError(null);
    try {
      const item = await getNewsById(newsId);
      setValues(toValues(item));
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to load article.");
    } finally {
      setLoading(false);
    }
  }, [newsId]);

  useEffect(() => {
    void load();
  }, [load]);

  function setField<K extends keyof NewsFormValues>(
    key: K,
    value: NewsFormValues[K],
  ) {
    setValues((prev) => ({ ...prev, [key]: value }));
  }

  async function onSubmit(event: React.FormEvent) {
    event.preventDefault();
    const payload = toPayload(values);
    if (
      !payload.title ||
      !payload.excerpt ||
      !payload.category ||
      !payload.author ||
      !payload.ticker ||
      payload.body.length === 0 ||
      !payload.publishedAt
    ) {
      setError("Please fill in all required fields.");
      return;
    }

    setSaving(true);
    setError(null);
    try {
      if (isEdit && newsId) {
        await updateNews(newsId, payload);
      } else {
        await createNews(payload);
      }
      router.push("/admin/news");
      router.refresh();
    } catch (err) {
      setError(
        err instanceof ApiError
          ? (err.errors[0] ?? err.message)
          : "Could not save this article.",
      );
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return <p className="admin-users-status">Loading article…</p>;
  }

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title={isEdit ? "Edit news" : "New news"}
        description="Same fields as the public News page: title, excerpt, category, date, author, ticker, and body."
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
            id={slugId}
            label="Slug"
            hint="Leave blank to generate from the title."
          >
            <input
              id={slugId}
              className="field-input"
              value={values.slug}
              onChange={(e) => setField("slug", e.target.value)}
              placeholder="student-profile-window"
            />
          </AdminFormField>
          <AdminFormField id={categoryId} label="Category" required>
            <input
              id={categoryId}
              className="field-input"
              list={`${categoryId}-options`}
              value={values.category}
              onChange={(e) => setField("category", e.target.value)}
              required
            />
            <datalist id={`${categoryId}-options`}>
              {NEWS_CATEGORIES.map((category) => (
                <option key={category} value={category} />
              ))}
            </datalist>
          </AdminFormField>
        </div>

        <div className="grid gap-5 sm:grid-cols-2">
          <AdminFormField id={dateId} label="Date" required>
            <input
              id={dateId}
              type="date"
              className="field-input"
              value={values.publishedAt}
              onChange={(e) => setField("publishedAt", e.target.value)}
              required
            />
          </AdminFormField>
          <AdminFormField id={authorId} label="Author" required>
            <input
              id={authorId}
              className="field-input"
              value={values.author}
              onChange={(e) => setField("author", e.target.value)}
              required
            />
          </AdminFormField>
        </div>

        <AdminFormField
          id={excerptId}
          label="Excerpt"
          required
          hint="Shown on the news list and article header."
        >
          <textarea
            id={excerptId}
            className="field-textarea"
            rows={3}
            value={values.excerpt}
            onChange={(e) => setField("excerpt", e.target.value)}
            required
          />
        </AdminFormField>

        <AdminFormField
          id={tickerId}
          label="Ticker"
          required
          hint="Short line for the home-page updates marquee."
        >
          <input
            id={tickerId}
            className="field-input"
            value={values.ticker}
            onChange={(e) => setField("ticker", e.target.value)}
            required
          />
        </AdminFormField>

        <AdminFormField
          id={bodyId}
          label="Body"
          required
          hint="Separate paragraphs with a blank line."
        >
          <textarea
            id={bodyId}
            className="field-textarea"
            rows={10}
            value={values.body}
            onChange={(e) => setField("body", e.target.value)}
            required
          />
        </AdminFormField>

        <label className="flex items-center gap-2 text-sm" htmlFor={featuredId}>
          <input
            id={featuredId}
            type="checkbox"
            checked={values.featured}
            onChange={(e) => setField("featured", e.target.checked)}
          />
          Featured story
        </label>

        <div className="flex flex-wrap gap-3">
          <Button type="submit" variant="primary" size="md" disabled={saving}>
            {saving ? "Saving…" : isEdit ? "Save article" : "Publish article"}
          </Button>
          <Button href="/admin/news" variant="outline" size="md">
            Cancel
          </Button>
        </div>
      </form>
    </div>
  );
}
