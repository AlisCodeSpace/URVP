import { apiDownloadFile, apiFetch } from "@/lib/api";
import type { PlacementDto } from "@/lib/matching-api";
import type { ProjectRankingDto } from "@/lib/project-rankings-api";

export type UserRoleName = "Student" | "Faculty" | "Admin";
export type UserSortField = "Name" | "Email" | "Role";
export type SortDirection = "Asc" | "Desc";

export type UserDto = {
  id: string;
  name: string;
  email: string;
  userName: string;
  affiliation: string;
  role: UserRoleName;
  roleLabel: string;
  registeredAt: string;
  updatedAt: string;
};

export type PaginatedUsers = {
  items: UserDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
};

export type ListUsersParams = {
  search?: string;
  role?: UserRoleName | "";
  facultyWithProjects?: boolean;
  sortBy?: UserSortField;
  sortDir?: SortDirection;
  pageNumber?: number;
  pageSize?: number;
};

export const USER_ROLE_OPTIONS: { value: UserRoleName; label: string }[] = [
  { value: "Student", label: "Student" },
  { value: "Faculty", label: "Faculty" },
  { value: "Admin", label: "Admin" },
];

export async function listUsers(
  params: ListUsersParams = {},
): Promise<PaginatedUsers> {
  const query = new URLSearchParams();
  if (params.search?.trim()) query.set("search", params.search.trim());
  if (params.role) query.set("role", params.role);
  if (params.facultyWithProjects) query.set("facultyWithProjects", "true");
  query.set("sortBy", params.sortBy ?? "Name");
  query.set("sortDir", params.sortDir ?? "Asc");
  query.set("pageNumber", String(params.pageNumber ?? 1));
  query.set("pageSize", String(params.pageSize ?? 20));

  return apiFetch<PaginatedUsers>(`/api/users?${query.toString()}`);
}

export type UserExportDetail = "basic" | "full";

export async function exportUsers(
  detail: UserExportDetail,
  params: Pick<
    ListUsersParams,
    "search" | "role" | "facultyWithProjects" | "sortBy" | "sortDir"
  > = {},
): Promise<void> {
  const query = new URLSearchParams();
  query.set("format", "xlsx");
  query.set("detail", detail);
  if (params.search?.trim()) query.set("search", params.search.trim());
  if (params.role) query.set("role", params.role);
  if (params.facultyWithProjects) query.set("facultyWithProjects", "true");
  query.set("sortBy", params.sortBy ?? "Name");
  query.set("sortDir", params.sortDir ?? "Asc");

  await apiDownloadFile(
    `/api/users/export?${query.toString()}`,
    detail === "full" ? "urvp-users-full.xlsx" : "urvp-users-basic.xlsx",
  );
}

export type StudentProjectActivityDto = {
  rankings: ProjectRankingDto[];
  assignments: PlacementDto[];
};

export async function getStudentProjectActivity(
  userId: string,
): Promise<StudentProjectActivityDto> {
  return apiFetch<StudentProjectActivityDto>(
    `/api/users/${userId}/project-activity`,
  );
}

export async function assignUserRole(
  userId: string,
  role: UserRoleName,
): Promise<UserDto> {
  return apiFetch<UserDto>(`/api/users/${userId}/role`, {
    method: "PUT",
    body: JSON.stringify({ role }),
  });
}
