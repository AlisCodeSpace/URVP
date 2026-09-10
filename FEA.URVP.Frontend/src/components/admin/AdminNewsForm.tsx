"use client";

import { useCallback, useEffect, useId, useState } from "react";
import { useRouter } from "next/navigation";
import { AdminFormField } from "@/components/admin/AdminFormField";
import { AdminNewsImagesField, type NewsImageDraft } from "@/components/admin/AdminNewsImagesField";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { Checkbox } from "@/components/ui/Checkbox";
import { DateField } from "@/components/ui/DateField";
import { AdminFormSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import { toAppDateInput, todayAppDateInput } from "@/lib/datetime";
import {
  MAX_NEWS_IMAGES,
  NEWS_CATEGORIES,
  createNews,
  getNewsById,
  newsImageUrl,
  updateNews,
  uploadNewsImage,
  type NewsArticleDto,
} from "@/lib/news-api";

type NewsFormValues = {
  title: string;
  excerpt: string;
  category: string;
  author: string;
  ticker: string;
  body: string;
  publishedAt: string;
  featured: boolean;
  published: boolean;
};

const emptyValues: NewsFormValues = {
  title: "",
  excerpt: "",
  category: "Announcement",
  author: "URVP Office",
  ticker: "",
  body: "",
  publishedAt: todayAppDateInput(),
  featured: false,
  published: true,
};

function toValues(dto: NewsArticleDto): NewsFormValues {
  return {
    title: dto.title,
    excerpt: dto.excerpt,
    category: dto.category,
    author: dto.author,
    ticker: dto.ticker,
    body: dto.body.join("\n\n"),
    publishedAt: toAppDateInput(dto.publishedAt),
    featured: dto.featured,
    published: dto.published,
  };
}

function draftsFromDto(dto: NewsArticleDto): NewsImageDraft[] {
  return (dto.imageFileIds ?? []).flatMap((id) => {
    const previewUrl = newsImageUrl(id);
    if (!previewUrl) return [];
    return [{ key: id, fileId: id, previewUrl }];
  });
}

function toPayload(values: NewsFormValues) {
  const body = values.body
    .split(/\n\s*\n/)
    .map((p) => p.trim())
    .filter(Boolean);

  return {
    title: values.title.trim(),
    excerpt: values.excerpt.trim(),
    category: values.category.trim(),
    author: values.author.trim(),
    ticker: values.ticker.trim(),
    body,
    publishedAt: values.publishedAt,
    featured: values.featured,
    published: values.published,
  };
}

export function AdminNewsForm({ newsId }: { newsId?: string }) {
  const router = useRouter();
  const isEdit = Boolean(newsId);
  const [values, setValues] = useState<NewsFormValues>(emptyValues);
  const [images, setImages] = useState<NewsImageDraft[]>([]);
  const [loading, setLoading] = useState(isEdit);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const titleId = useId();
  const excerptId = useId();
  const categoryId = useId();
  const authorId = useId();
  const tickerId = useId();
  const bodyId = useId();
  const dateId = useId();

  const load = useCallback(async () => {
    if (!newsId) return;
    setLoading(true);
    setError(null);
    try {
      const item = await getNewsById(newsId);
      setValues(toValues(item));
      setImages(draftsFromDto(item));
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

  function addImageFiles(files: File[]) {
    setImages((prev) => {
      const room = MAX_NEWS_IMAGES - prev.length;
      const accepted = files.slice(0, Math.max(0, room));
      return [
        ...prev,
        ...accepted.map((file) => ({
          key: `${file.name}-${file.size}-${file.lastModified}-${crypto.randomUUID()}`,
          file,
          previewUrl: URL.createObjectURL(file),
        })),
      ];
    });
  }

  function removeImage(key: string) {
    setImages((prev) => {
      const next = prev.filter((image) => image.key !== key);
      const removed = prev.find((image) => image.key === key);
      if (removed?.file) URL.revokeObjectURL(removed.previewUrl);
      return next;
    });
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
      const keptIds = images
        .filter((image) => image.fileId && !image.file)
        .map((image) => image.fileId as string);

      const saved =
        isEdit && newsId
          ? await updateNews(newsId, { ...payload, imageFileIds: keptIds })
          : await createNews(payload);

      const imageFileIds: string[] = [];
      for (const image of images) {
        if (image.file) {
          const uploaded = await uploadNewsImage(saved.id, image.file);
          imageFileIds.push(uploaded.id);
        } else if (image.fileId) {
          imageFileIds.push(image.fileId);
        }
      }

      await updateNews(saved.id, { ...payload, imageFileIds });

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
    return (
      <div className="admin-panel admin-panel--wide">
        <AdminPageHeader
          title={isEdit ? "Edit news" : "New news"}
          description="Same fields as the public News page: title, excerpt, category, date, author, ticker, and body."
          backHref="/admin/news"
          backLabel="Back to news"
        />
        <AdminFormSkeleton fields={8} />
      </div>
    );
  }

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title={isEdit ? "Edit news" : "New news"}
        description="Same fields as the public News page: title, excerpt, category, date, author, ticker, and body."
        backHref="/admin/news"
        backLabel="Back to news"
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

        <div className="grid gap-5 sm:grid-cols-2">
          <AdminFormField id={dateId} label="Date" required>
            <DateField
              id={dateId}
              placeholder="Select date"
              value={values.publishedAt}
              onChange={(next) => setField("publishedAt", next)}
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

        <AdminNewsImagesField
          images={images}
          onAddFiles={addImageFiles}
          onRemove={removeImage}
        />

        <Checkbox
          checked={values.published}
          onCheckedChange={(checked) => setField("published", checked)}
        >
          Published
        </Checkbox>

        <Checkbox
          checked={values.featured}
          onCheckedChange={(checked) => setField("featured", checked)}
        >
          Featured story
        </Checkbox>

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
