"use client";

import { useCallback, useEffect, useId, useState, type FormEvent } from "react";
import { AdminFormField } from "@/components/admin/AdminFormField";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { AdminFormSkeleton } from "@/components/ui/SectionSkeletons";
import { Tag } from "@/components/ui/Tag";
import { ApiError } from "@/lib/api";
import {
  getEmailSettings,
  updateEmailSettings,
  type EmailSettingsDto,
} from "@/lib/email-settings-api";

export function AdminEmailSettingsView() {
  const userNameId = useId();
  const passwordId = useId();

  const [settings, setSettings] = useState<EmailSettingsDto | null>(null);
  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [clearing, setClearing] = useState(false);
  const [confirmClear, setConfirmClear] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);

  const applySettings = useCallback((dto: EmailSettingsDto) => {
    setSettings(dto);
    setUserName(dto.userName ?? "");
    setPassword("");
  }, []);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    setSaved(false);
    try {
      applySettings(await getEmailSettings());
    } catch (err) {
      setSettings(null);
      setError(
        err instanceof ApiError ? err.message : "Failed to load email settings.",
      );
    } finally {
      setLoading(false);
    }
  }, [applySettings]);

  useEffect(() => {
    void load();
  }, [load]);

  async function onSubmit(event: FormEvent) {
    event.preventDefault();
    setSaving(true);
    setError(null);
    setSaved(false);
    try {
      const next = await updateEmailSettings({
        userName: userName.trim() || null,
        password: password.trim() || null,
      });
      applySettings(next);
      setSaved(true);
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Failed to save email settings.",
      );
    } finally {
      setSaving(false);
    }
  }

  async function onClearPassword() {
    setClearing(true);
    setError(null);
    setSaved(false);
    try {
      const next = await updateEmailSettings({
        userName: userName.trim() || null,
        clearPassword: true,
      });
      applySettings(next);
      setConfirmClear(false);
      setSaved(true);
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : "Failed to remove the stored password.",
      );
    } finally {
      setClearing(false);
    }
  }

  if (loading) {
    return (
      <div className="admin-panel">
        <AdminPageHeader
          title="Email"
          description="SMTP credentials used to send program notifications."
        />
        <AdminFormSkeleton fields={5} />
      </div>
    );
  }

  return (
    <div className="admin-panel">
      <AdminPageHeader
        title="Email"
        description="The SMTP host comes from server configuration. Store the mailbox password here — it is encrypted in the database and never shown again."
        tag={
          settings?.enabled
            ? "Sending on"
            : "Sending off"
        }
      />

      {error ? (
        <p className="admin-users-banner is-error" role="alert">
          {error}
        </p>
      ) : null}
      {saved ? (
        <p className="admin-users-banner" role="status">
          Email settings saved.
        </p>
      ) : null}

      {settings ? (
        <dl className="admin-email-meta">
          <div>
            <dt>From</dt>
            <dd>{settings.from}</dd>
          </div>
          <div>
            <dt>SMTP server</dt>
            <dd>
              {settings.smtpHost || "Not configured"}
              {settings.smtpHost ? `:${settings.smtpPort}` : null}
            </dd>
          </div>
          <div>
            <dt>Encryption</dt>
            <dd>{settings.enableSsl ? "SSL/TLS on" : "SSL/TLS off"}</dd>
          </div>
          <div>
            <dt>Password</dt>
            <dd>
              <Tag tone={settings.passwordIsSet ? "secondary" : "muted"}>
                {settings.passwordIsSet ? "Stored" : "Not set"}
              </Tag>
            </dd>
          </div>
        </dl>
      ) : null}

      <form className="mt-6 grid max-w-xl gap-5" onSubmit={onSubmit} autoComplete="off">
        <AdminFormField
          id={userNameId}
          label="SMTP username"
          hint="Usually the From address. Leave blank to use the From address when a password is set."
        >
          <input
            id={userNameId}
            className="field-input"
            value={userName}
            onChange={(e) => setUserName(e.target.value)}
            autoComplete="off"
            spellCheck={false}
          />
        </AdminFormField>

        <AdminFormField
          id={passwordId}
          label="SMTP password"
          hint={
            settings?.passwordIsSet
              ? "Leave blank to keep the stored password. Enter a new value to replace it."
              : "Stored encrypted. It will not be displayed after you save."
          }
        >
          <input
            id={passwordId}
            type="password"
            className="field-input"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="new-password"
            placeholder={settings?.passwordIsSet ? "••••••••" : undefined}
          />
        </AdminFormField>

        <div className="flex flex-wrap gap-3">
          <Button type="submit" variant="primary" size="md" disabled={saving || clearing}>
            {saving ? "Saving…" : "Save credentials"}
          </Button>
          {settings?.passwordIsSet ? (
            <Button
              type="button"
              variant="danger"
              size="md"
              disabled={saving || clearing}
              onClick={() => setConfirmClear(true)}
            >
              Remove password
            </Button>
          ) : null}
        </div>
      </form>

      <ConfirmModal
        open={confirmClear}
        onClose={() => setConfirmClear(false)}
        onConfirm={onClearPassword}
        title="Remove stored SMTP password?"
        description="Outgoing mail will no longer authenticate with the stored password until you enter a new one."
        confirmLabel="Remove password"
        busy={clearing}
      />
    </div>
  );
}
