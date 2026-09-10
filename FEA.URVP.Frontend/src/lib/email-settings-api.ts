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
  password?: string | null;
  clearPassword?: boolean;
};

export type EmailLogDto = {
  id: string;
  from: string;
  to: string;
  cc: string | null;
  bcc: string | null;
  exception: string | null;
  success: boolean;
  createdOn: string;
};

export type PaginatedEmailLogs = {
  items: EmailLogDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
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

export async function listEmailLogs(params: {
  pageNumber?: number;
  pageSize?: number;
} = {}): Promise<PaginatedEmailLogs> {
  const query = new URLSearchParams();
  query.set("pageNumber", String(params.pageNumber ?? 1));
  query.set("pageSize", String(params.pageSize ?? 50));
  return apiFetch<PaginatedEmailLogs>(
    `/api/admin/email-settings/logs?${query.toString()}`,
  );
}
