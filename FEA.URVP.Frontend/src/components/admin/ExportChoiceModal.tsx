"use client";

import { useEffect, useId } from "react";
import { Heading, Text } from "@/components/ui/Typography";
import { Button } from "@/components/ui/Button";
import { IconDownload } from "@/components/ui/Icons";
import { useScrollLock } from "@/hooks/useScrollLock";

export type ExportChoice = {
  id: string;
  title: string;
  description: string;
};

type ExportChoiceModalProps = {
  open: boolean;
  title: string;
  description?: string;
  options: readonly ExportChoice[];
  busyId: string | null;
  error?: string | null;
  onClose: () => void;
  onSelect: (id: string) => void;
};

export function ExportChoiceModal({
  open,
  title,
  description,
  options,
  busyId,
  error,
  onClose,
  onSelect,
}: ExportChoiceModalProps) {
  const titleId = useId();
  const descriptionId = useId();
  const busy = busyId !== null;

  useScrollLock(open);

  useEffect(() => {
    if (!open) return;

    function onKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape" && !busy) onClose();
    }

    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [open, busy, onClose]);

  if (!open) return null;

  return (
    <div
      className="dialog-layer fixed inset-0 z-50 flex items-center justify-center"
      role="presentation"
    >
      <button
        type="button"
        className="absolute inset-0 bg-primary/45 backdrop-blur-[2px]"
        aria-label="Close"
        disabled={busy}
        onClick={() => {
          if (!busy) onClose();
        }}
      />

      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        aria-describedby={description ? descriptionId : undefined}
        className="dialog-layer-panel relative z-10 w-full max-w-lg rounded-[var(--radius-lg)] border border-primary/12 bg-surface shadow-[0_24px_60px_-28px_rgba(61,18,72,0.45)]"
      >
        <div className="border-b border-primary/10 px-6 py-5">
          <Heading
            id={titleId}
            as="h2"
            size="5"
            weight="medium"
            className="!font-[family-name:var(--font-display)] !text-primary"
          >
            {title}
          </Heading>
          {description ? (
            <Text
              id={descriptionId}
              as="p"
              size="2"
              mt="2"
              className="!leading-relaxed !text-muted"
            >
              {description}
            </Text>
          ) : null}
        </div>

        <div className="space-y-2.5 px-6 py-5">
          {options.map((option) => {
            const selected = busyId === option.id;
            return (
              <button
                key={option.id}
                type="button"
                disabled={busy}
                onClick={() => onSelect(option.id)}
                className="w-full rounded-md border border-primary/12 bg-background px-4 py-3 text-left transition hover:border-secondary/60 disabled:opacity-60"
              >
                <div className="flex items-center justify-between gap-3">
                  <span className="text-sm font-semibold text-primary">
                    {option.title}
                  </span>
                  <span className="inline-flex items-center gap-1 text-xs text-muted">
                    <IconDownload />
                    {selected ? "Exporting…" : "Download"}
                  </span>
                </div>
                <Text as="p" size="2" mt="1" className="!leading-relaxed !text-muted">
                  {option.description}
                </Text>
              </button>
            );
          })}

          {error ? (
            <Text as="p" size="2" role="alert" className="!text-red-700">
              {error}
            </Text>
          ) : null}
        </div>

        <div className="flex flex-wrap items-center justify-end gap-2 border-t border-primary/10 px-6 py-4">
          <Button
            type="button"
            variant="ghost"
            size="md"
            disabled={busy}
            onClick={onClose}
          >
            Close
          </Button>
        </div>
      </div>
    </div>
  );
}
