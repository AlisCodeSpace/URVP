"use client";

import { useId, useRef, useState, type ChangeEvent } from "react";
import { Button } from "@/components/ui/Button";
import { IconTrash } from "@/components/ui/Icons";
import {
  MAX_NEWS_IMAGE_BYTES,
  MAX_NEWS_IMAGES,
  MAX_NEWS_IMAGES_TOTAL_BYTES,
} from "@/lib/news-api";

export type NewsImageDraft = {
  key: string;
  fileId?: string;
  file?: File;
  previewUrl: string;
};

type AdminNewsImagesFieldProps = {
  images: NewsImageDraft[];
  onAddFiles: (files: File[]) => void;
  onRemove: (key: string) => void;
};

const IMAGE_SIZE_MB = MAX_NEWS_IMAGE_BYTES / (1024 * 1024);
const IMAGES_TOTAL_MB = MAX_NEWS_IMAGES_TOTAL_BYTES / (1024 * 1024);

export function AdminNewsImagesField({
  images,
  onAddFiles,
  onRemove,
}: AdminNewsImagesFieldProps) {
  const inputId = useId();
  const inputRef = useRef<HTMLInputElement>(null);
  const remaining = MAX_NEWS_IMAGES - images.length;
  const [limitError, setLimitError] = useState<string | null>(null);

  function onPick(event: ChangeEvent<HTMLInputElement>) {
    const picked = Array.from(event.target.files ?? []);
    event.target.value = "";
    if (picked.length === 0) return;

    const oversized = picked.filter((file) => file.size > MAX_NEWS_IMAGE_BYTES);
    const withinEach = picked.filter((file) => file.size <= MAX_NEWS_IMAGE_BYTES);
    const currentTotal = images.reduce(
      (sum, image) => sum + (image.file?.size ?? 0),
      0,
    );
    const accepted: File[] = [];
    let running = currentTotal;
    for (const file of withinEach) {
      if (running + file.size > MAX_NEWS_IMAGES_TOTAL_BYTES) break;
      accepted.push(file);
      running += file.size;
    }

    if (oversized.length > 0 || accepted.length < withinEach.length) {
      setLimitError(
        `Each photo must be ${IMAGE_SIZE_MB} MB or less, and all photos together must stay within ${IMAGES_TOTAL_MB} MB.`,
      );
    } else {
      setLimitError(null);
    }

    if (accepted.length > 0) onAddFiles(accepted);
  }

  return (
    <div>
      <p className="field-label">Photos</p>
      <p className="field-hint mt-1">
        JPG, PNG, or GIF. The first photo appears on the news list. Up to{" "}
        {MAX_NEWS_IMAGES} images. Each photo can be up to {IMAGE_SIZE_MB} MB, and
        all photos together must stay within {IMAGES_TOTAL_MB} MB.
      </p>

      {limitError ? (
        <p className="admin-users-banner is-error mt-2" role="alert">
          {limitError}
        </p>
      ) : null}

      {images.length > 0 ? (
        <ul className="news-admin-images mt-3">
          {images.map((image, index) => (
            <li key={image.key} className="news-admin-image">
              <div className="news-admin-image-preview">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img src={image.previewUrl} alt="" />
                <button
                  type="button"
                  className="news-admin-image-remove"
                  aria-label={
                    index === 0 ? "Remove list photo" : `Remove photo ${index + 1}`
                  }
                  onClick={() => onRemove(image.key)}
                >
                  <IconTrash size={16} />
                </button>
              </div>
              <p className="news-admin-image-meta">
                {index === 0 ? "List photo" : `Photo ${index + 1}`}
              </p>
            </li>
          ))}
        </ul>
      ) : null}

      <input
        ref={inputRef}
        id={inputId}
        type="file"
        accept="image/jpeg,image/png,image/gif,.jpg,.jpeg,.png,.gif"
        className="sr-only"
        multiple
        disabled={remaining <= 0}
        onChange={onPick}
      />
      <div className="mt-3">
        <Button
          type="button"
          variant="outline"
          size="sm"
          disabled={remaining <= 0}
          onClick={() => inputRef.current?.click()}
        >
          {images.length > 0 ? "Add photos" : "Upload photos"}
        </Button>
      </div>
    </div>
  );
}
