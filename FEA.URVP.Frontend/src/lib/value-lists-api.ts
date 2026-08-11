import { apiFetch } from "@/lib/api";

export type ValueListKindSlug = "research-interests" | "research-areas";

export type ValueListItemDto = {
  id: string;
  kind: "ResearchInterest" | "ResearchArea";
  name: string;
  sortOrder: number;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
};

export type PaginatedValueListItems = {
  items: ValueListItemDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
};

export type ListValueListParams = {
  search?: string;
  pageNumber?: number;
  pageSize?: number;
};

export async function listValueListItems(
  kind: ValueListKindSlug,
  params: ListValueListParams = {},
): Promise<PaginatedValueListItems> {
  const query = new URLSearchParams();
  if (params.search?.trim()) query.set("search", params.search.trim());
  query.set("pageNumber", String(params.pageNumber ?? 1));
  query.set("pageSize", String(params.pageSize ?? 20));

  return apiFetch<PaginatedValueListItems>(
    `/api/value-lists/${kind}?${query.toString()}`,
  );
}

export async function createValueListItem(
  kind: ValueListKindSlug,
  payload: { name: string; isActive?: boolean },
): Promise<ValueListItemDto> {
  return apiFetch<ValueListItemDto>(`/api/value-lists/${kind}`, {
    method: "POST",
    body: JSON.stringify({
      name: payload.name,
      isActive: payload.isActive ?? true,
    }),
  });
}

export async function updateValueListItem(
  kind: ValueListKindSlug,
  id: string,
  payload: { name: string; isActive?: boolean },
): Promise<ValueListItemDto> {
  return apiFetch<ValueListItemDto>(`/api/value-lists/${kind}/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export async function deleteValueListItem(
  kind: ValueListKindSlug,
  id: string,
): Promise<void> {
  await apiFetch<null>(`/api/value-lists/${kind}/${id}`, {
    method: "DELETE",
  });
}
