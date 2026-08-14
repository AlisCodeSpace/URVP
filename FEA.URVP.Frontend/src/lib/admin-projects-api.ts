import { apiFetch } from "@/lib/api";
import type { MyProjectStatus } from "@/lib/project-form";
import type { ProjectDto } from "@/lib/projects-api";

export type AdminProjectListItemDto = {
  id: string;
  title: string;
  facultyName: string;
  affiliation: string;
  email: string;
  status: MyProjectStatus;
  volunteersRequired: number;
  volunteersFilled: number;
  rankingCount: number;
  createdAt: string;
  updatedAt: string;
};

export type ProjectRankingStudentDto = {
  rankingId: string;
  studentUserId: string;
  studentName: string;
  studentEmail: string;
  studentUserName?: string | null;
  rank: number;
  rankedAt: string;
  updatedAt: string;
};

export type AdminProjectDetailDto = {
  project: ProjectDto;
  rankings: ProjectRankingStudentDto[];
};

export type PaginatedAdminProjects = {
  items: AdminProjectListItemDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
};

export type ListAdminProjectsParams = {
  search?: string;
  status?: MyProjectStatus | "";
  pageNumber?: number;
  pageSize?: number;
};

export async function listAdminProjects(
  params: ListAdminProjectsParams = {},
): Promise<PaginatedAdminProjects> {
  const query = new URLSearchParams();
  if (params.search?.trim()) query.set("search", params.search.trim());
  if (params.status) query.set("status", params.status);
  query.set("pageNumber", String(params.pageNumber ?? 1));
  query.set("pageSize", String(params.pageSize ?? 20));

  return apiFetch<PaginatedAdminProjects>(
    `/api/projects/admin?${query.toString()}`,
  );
}

export async function getAdminProject(
  id: string,
): Promise<AdminProjectDetailDto> {
  return apiFetch<AdminProjectDetailDto>(`/api/projects/admin/${id}`);
}
