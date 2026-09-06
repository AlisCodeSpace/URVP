"use client";

import { useEffect, useId, useState } from "react";
import { Heading } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { useScrollLock } from "@/hooks/useScrollLock";
import type { NotificationSettings } from "@/lib/notifications-api";

type NotificationSettingsDialogProps = {
  open: boolean;
  onClose: () => void;
  settings: NotificationSettings | null;
  busy?: boolean;
  onSave: (settings: NotificationSettings) => Promise<void>;
};

export function NotificationSettingsDialog({
  open,
  onClose,
  settings,
  busy = false,
  onSave,
}: NotificationSettingsDialogProps) {
  const titleId = useId();
  const [emailNotifications, setEmailNotifications] = useState(true);
  const [inAppNotifications, setInAppNotifications] = useState(true);
  const [saving, setSaving] = useState(false);

  useScrollLock(open);

  useEffect(() => {
    if (!open || !settings) return;
    setEmailNotifications(settings.emailNotifications);
    setInAppNotifications(settings.inAppNotifications);
  }, [open, settings]);

  useEffect(() => {
    if (!open) return;
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape" && !saving) onClose();
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [onClose, open, saving]);

  if (!open) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center px-4 py-6" role="presentation">
      <button
        type="button"
        className="absolute inset-0 bg-primary/45 backdrop-blur-[2px]"
        aria-label="Close"
        disabled={saving || busy}
        onClick={() => {
          if (!saving) onClose();
        }}
      />
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
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
            Notification settings
          </Heading>
          <div className="mt-4 flex flex-col gap-3">
            <label className="flex items-start gap-3 text-sm text-foreground">
              <input
                type="checkbox"
                checked={inAppNotifications}
                disabled={saving || busy}
                onChange={(event) => setInAppNotifications(event.target.checked)}
              />
              <span>
                <strong>In-app notifications</strong>
                <span className="mt-0.5 block text-muted">
                  Hide the list in this browser. Existing notifications stay in the database.
                </span>
              </span>
            </label>
            <label className="flex items-start gap-3 text-sm text-foreground">
              <input
                type="checkbox"
                checked={emailNotifications}
                disabled={saving || busy}
                onChange={(event) => setEmailNotifications(event.target.checked)}
              />
              <span>
                <strong>Email notifications</strong>
                <span className="mt-0.5 block text-muted">
                  Queue an email when a new notification is created.
                </span>
              </span>
            </label>
          </div>
        </div>
        <div className="flex flex-wrap items-center justify-end gap-2 border-t border-primary/10 px-6 py-4">
          <Button type="button" variant="ghost" size="md" disabled={saving} onClick={onClose}>
            Cancel
          </Button>
          <Button
            type="button"
            variant="primary"
            size="md"
            disabled={saving || busy}
            onClick={() => {
              setSaving(true);
              void onSave({ emailNotifications, inAppNotifications }).finally(() => {
                setSaving(false);
                onClose();
              });
            }}
          >
            {saving ? "Saving…" : "Save"}
          </Button>
        </div>
      </div>
    </div>
  );
}
