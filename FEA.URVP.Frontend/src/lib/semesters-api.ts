import { apiFetch } from "@/lib/api";

export type SemesterDto = {
  id: string;
  name: string;
  description?: string | null;
  isActive: boolean;
  cycleStart?: string | null;
  cycleEnd?: string | null;
  applicationWindowStart?: string | null;
  applicationWindowEnd?: string | null;
  isApplicationWindowOpen: boolean;
  createdAt: string;
  updatedAt: string;
};

export type SemesterWritePayload = {
  name: string;
  description?: string | null;
  cycleStart?: string | null;
  cycleEnd?: string | null;
  applicationWindowStart?: string | null;
  applicationWindowEnd?: string | null;
};

export type SetApplicationWindowPayload = {
  applicationWindowStart?: string | null;
  applicationWindowEnd?: string | null;
};

export async function listSemesters(): Promise<SemesterDto[]> {
  return apiFetch<SemesterDto[]>("/api/semesters");
}

export async function getActiveSemester(): Promise<SemesterDto | null> {
  return apiFetch<SemesterDto | null>("/api/semesters/active");
}

export async function getSemester(id: string): Promise<SemesterDto> {
  return apiFetch<SemesterDto>(`/api/semesters/${id}`);
}

export async function createSemester(
  payload: SemesterWritePayload,
): Promise<SemesterDto> {
  return apiFetch<SemesterDto>("/api/semesters", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export async function updateSemester(
  id: string,
  payload: SemesterWritePayload,
): Promise<SemesterDto> {
  return apiFetch<SemesterDto>(`/api/semesters/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export async function deleteSemester(id: string): Promise<void> {
  await apiFetch<null>(`/api/semesters/${id}`, { method: "DELETE" });
}

export async function setSemesterActive(
  id: string,
  isActive: boolean,
): Promise<SemesterDto> {
  return apiFetch<SemesterDto>(`/api/semesters/${id}/set-active`, {
    method: "POST",
    body: JSON.stringify({ isActive }),
  });
}

export async function setApplicationWindow(
  id: string,
  payload: SetApplicationWindowPayload,
): Promise<SemesterDto> {
  return apiFetch<SemesterDto>(`/api/semesters/${id}/set-application-window`, {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

/** Treat API datetimes as UTC when the payload omits a timezone. */
export function parseApiDate(iso: string): Date {
  const trimmed = iso.trim();
  if (/[zZ]$/.test(trimmed) || /[+-]\d{2}:\d{2}$/.test(trimmed)) {
    return new Date(trimmed);
  }
  return new Date(`${trimmed}Z`);
}

export function formatWindowDate(iso: string | null | undefined): string {
  if (!iso) return "—";
  return parseApiDate(iso).toLocaleString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
    hour: "2-digit",
    minute: "2-digit",
    timeZoneName: "short",
  });
}

export function formatScheduleRange(
  start: string | null | undefined,
  end: string | null | undefined,
): string {
  if (!start) return "Not scheduled";
  const from = formatWindowDate(start);
  if (!end) return `${from} → open`;
  return `${from} → ${formatWindowDate(end)}`;
}
