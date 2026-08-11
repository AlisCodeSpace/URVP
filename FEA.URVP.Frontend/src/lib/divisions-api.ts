import { apiFetch } from "@/lib/api";

export type DivisionDto = {
  id: string;
  name: string;
  description: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

export type PaginatedDivisions = {
  items: DivisionDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
};

export type ListDivisionsParams = {
  search?: string;
  pageNumber?: number;
  pageSize?: number;
};

export async function listDivisions(
  params: ListDivisionsParams = {},
): Promise<PaginatedDivisions> {
  const query = new URLSearchParams();
  if (params.search?.trim()) query.set("search", params.search.trim());
  query.set("pageNumber", String(params.pageNumber ?? 1));
  query.set("pageSize", String(params.pageSize ?? 20));

  return apiFetch<PaginatedDivisions>(`/api/divisions?${query.toString()}`);
}

export async function createDivision(payload: {
  name: string;
  description?: string;
  isActive?: boolean;
}): Promise<DivisionDto> {
  return apiFetch<DivisionDto>("/api/divisions", {
    method: "POST",
    body: JSON.stringify({
      name: payload.name,
      description: payload.description ?? "",
      isActive: payload.isActive ?? true,
    }),
  });
}

export async function updateDivision(
  id: string,
  payload: { name: string; description?: string; isActive?: boolean },
): Promise<DivisionDto> {
  return apiFetch<DivisionDto>(`/api/divisions/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export async function deleteDivision(id: string): Promise<void> {
  await apiFetch<null>(`/api/divisions/${id}`, {
    method: "DELETE",
  });
}
