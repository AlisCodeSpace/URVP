import { apiFetch } from "@/lib/api";
import { getApiBaseUrl } from "@/lib/config";
import { workshops as fallbackWorkshops, type Workshop } from "@/lib/workshops";
import type { FileMetadataDto } from "@/lib/student-profile-api";

export type WorkshopDto = {
  id: string;
  title: string;
  date: string;
  time?: string | null;
  location?: string | null;
  description: string;
  registrationUrl: string;
  posterFileId?: string | null;
  posterAlt?: string | null;
  sortOrder: number;
  createdAt: string;
  updatedAt: string;
};

export type PaginatedWorkshops = {
  items: WorkshopDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
};

export type WorkshopWritePayload = {
  title: string;
  date: string;
  time?: string | null;
  location?: string | null;
  description: string;
  registrationUrl: string;
  posterFileId?: string | null;
  posterAlt?: string | null;
};

export function workshopPosterUrl(
  fileId: string | null | undefined,
): string | undefined {
  if (!fileId) return undefined;
  return `${getApiBaseUrl()}/api/files/${fileId}`;
}

export function toWorkshop(dto: WorkshopDto): Workshop {
  return {
    id: dto.id,
    title: dto.title,
    date: dto.date,
    time: dto.time ?? undefined,
    location: dto.location ?? undefined,
    description: dto.description,
    registrationUrl: dto.registrationUrl,
    posterSrc: workshopPosterUrl(dto.posterFileId),
    posterAlt: dto.posterAlt ?? undefined,
  };
}

export async function listWorkshops(params: {
  search?: string;
  pageNumber?: number;
  pageSize?: number;
} = {}): Promise<PaginatedWorkshops> {
  const query = new URLSearchParams();
  if (params.search?.trim()) query.set("search", params.search.trim());
  query.set("pageNumber", String(params.pageNumber ?? 1));
  query.set("pageSize", String(params.pageSize ?? 100));
  return apiFetch<PaginatedWorkshops>(`/api/workshops?${query.toString()}`);
}

export async function getWorkshop(id: string): Promise<WorkshopDto> {
  return apiFetch<WorkshopDto>(`/api/workshops/${id}`);
}

export async function createWorkshop(
  payload: WorkshopWritePayload,
): Promise<WorkshopDto> {
  return apiFetch<WorkshopDto>("/api/workshops", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export async function updateWorkshop(
  id: string,
  payload: WorkshopWritePayload,
): Promise<WorkshopDto> {
  return apiFetch<WorkshopDto>(`/api/workshops/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export async function deleteWorkshop(id: string): Promise<void> {
  await apiFetch<null>(`/api/workshops/${id}`, { method: "DELETE" });
}

export async function uploadWorkshopPoster(
  workshopId: string,
  file: File,
): Promise<FileMetadataDto> {
  const body = new FormData();
  body.append("file", file);
  body.append("entityType", "Workshop");
  body.append("entityId", workshopId);
  body.append("fileCategory", "Poster");
  return apiFetch<FileMetadataDto>("/api/files", {
    method: "POST",
    body,
  });
}

export async function loadPublicWorkshops(): Promise<Workshop[]> {
  try {
    const page = await listWorkshops({ pageNumber: 1, pageSize: 200 });
    return page.items.map(toWorkshop);
  } catch {
    return fallbackWorkshops;
  }
}
