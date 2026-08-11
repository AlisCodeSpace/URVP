"use client";

import { useEffect, useId, type ReactNode } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { Button, type ButtonVariant } from "@/components/ui/Button";
import { useScrollLock } from "@/hooks/useScrollLock";

export type ConfirmModalProps = {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void | Promise<void>;
  title: string;
  description?: ReactNode;
  confirmLabel?: string;
  cancelLabel?: string;
  busyLabel?: string;
  confirmVariant?: Extract<ButtonVariant, "danger" | "primary">;
  busy?: boolean;
};

export function ConfirmModal({
  open,
  onClose,
  onConfirm,
  title,
  description,
  confirmLabel = "Confirm",
  cancelLabel = "Cancel",
  busyLabel = "Working…",
  confirmVariant = "danger",
  busy = false,
}: ConfirmModalProps) {
  const titleId = useId();
  const descriptionId = useId();

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
      className="fixed inset-0 z-50 flex items-center justify-center px-4 py-6"
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
        className="relative z-10 w-full max-w-md overflow-hidden rounded-[var(--radius-lg)] border border-primary/12 bg-surface shadow-[0_24px_60px_-28px_rgba(61,18,72,0.45)]"
      >
        <div className="px-6 py-5">
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

        <div className="flex flex-wrap items-center justify-end gap-2 border-t border-primary/10 px-6 py-4">
          <Button
            type="button"
            variant="ghost"
            size="md"
            disabled={busy}
            onClick={onClose}
          >
            {cancelLabel}
          </Button>
          <Button
            type="button"
            variant={confirmVariant}
            size="md"
            disabled={busy}
            onClick={() => void onConfirm()}
          >
            {busy ? busyLabel : confirmLabel}
          </Button>
        </div>
      </div>
    </div>
  );
}
