import { apiFetch } from "@/lib/api";

export type EmailSettingsDto = {
  enabled: boolean;
  from: string;
  fromName: string;
  smtpHost: string | null;
  smtpPort: number;
  enableSsl: boolean;
  userName: string | null;
  passwordIsSet: boolean;
};

export type UpdateEmailSettingsPayload = {
  userName?: string | null;
  password?: string | null;
  clearPassword?: boolean;
};

export async function getEmailSettings(): Promise<EmailSettingsDto> {
  return apiFetch<EmailSettingsDto>("/api/admin/email-settings");
}

export async function updateEmailSettings(
  payload: UpdateEmailSettingsPayload,
): Promise<EmailSettingsDto> {
  return apiFetch<EmailSettingsDto>("/api/admin/email-settings", {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}
