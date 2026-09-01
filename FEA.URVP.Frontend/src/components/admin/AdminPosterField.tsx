"use client";

import { useId, useRef, type ChangeEvent } from "react";
import { Button } from "@/components/ui/Button";

type AdminPosterFieldProps = {
  previewUrl?: string | null;
  alt: string;
  onAltChange: (value: string) => void;
  onFileChange: (file: File | null) => void;
  fileName?: string | null;
};

export function AdminPosterField({
  previewUrl,
  alt,
  onAltChange,
  onFileChange,
  fileName,
}: AdminPosterFieldProps) {
  const fileId = useId();
  const altId = useId();
  const inputRef = useRef<HTMLInputElement>(null);

  function onPick(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0] ?? null;
    onFileChange(file);
  }

  return (
    <div className="grid gap-5 sm:grid-cols-[minmax(0,16rem)_1fr] sm:items-start">
      <div>
        <p className="field-label">Card photo</p>
        <div className="workshop-poster relative mt-2 aspect-[3/2] overflow-hidden rounded-md border border-primary/15 bg-primary-deep">
          {previewUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={previewUrl}
              alt={alt || "Workshop poster preview"}
              className="absolute inset-0 h-full w-full object-cover"
            />
          ) : (
            <div className="workshop-poster-fallback flex h-full flex-col justify-between p-4 text-white">
              <p className="text-xs font-medium uppercase tracking-[0.22em] text-secondary">
                URVP Workshop
              </p>
              <p className="text-sm text-white/70">3:2 photo preview</p>
            </div>
          )}
        </div>
        <p className="field-hint mt-2">
          JPG, PNG, or WebP. Cropped to 3:2 to match the workshop card.
        </p>
      </div>

      <div className="grid gap-4">
        <div>
          <input
            ref={inputRef}
            id={fileId}
            type="file"
            accept="image/jpeg,image/png,image/webp,.jpg,.jpeg,.png,.webp"
            className="sr-only"
            onChange={onPick}
          />
          <div className="flex flex-wrap items-center gap-3">
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={() => inputRef.current?.click()}
            >
              {previewUrl ? "Replace photo" : "Upload photo"}
            </Button>
            {previewUrl ? (
              <button
                type="button"
                className="text-sm text-muted underline-offset-2 hover:underline"
                onClick={() => {
                  if (inputRef.current) inputRef.current.value = "";
                  onFileChange(null);
                }}
              >
                Remove
              </button>
            ) : null}
          </div>
          {fileName ? (
            <p className="mt-2 text-sm text-muted">{fileName}</p>
          ) : null}
        </div>

        <div>
          <label htmlFor={altId} className="field-label">
            Photo description
          </label>
          <input
            id={altId}
            type="text"
            className="field-input"
            placeholder="Short alt text for the poster"
            value={alt}
            onChange={(e) => onAltChange(e.target.value)}
          />
        </div>
      </div>
    </div>
  );
}
