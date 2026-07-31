import { apiFetch } from "@/lib/api";
import {
  formatProjectDate,
  type MyProject,
  type MyProjectStatus,
  type ProjectFormValues,
} from "@/lib/project-form";
import type { CatalogProject } from "@/lib/projects";

export type ProjectDto = {
  id: string;
  createdByUserId: string;
  title: string;
  researchAreas: string[];
  irbStage: string;
  irbStageLabel: string;
  briefDescription: string;
  activityTypes: string[];
  volunteersRequired: number;
  volunteersFilled: number;
  minQualifications?: string | null;
  additionalComments?: string | null;
  status: MyProjectStatus;
  facultyName: string;
  affiliation: string;
  email: string;
  userName?: string | null;
  createdAt: string;
  updatedAt: string;
};

type PaginatedProjects = {
  items: ProjectDto[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
};

export type ProjectWritePayload = {
  title: string;
  researchAreas: string[];
  irbStage: string;
  briefDescription: string;
  activityTypes: string[];
  volunteersRequired: number;
  minQualifications?: string | null;
  additionalComments?: string | null;
  affiliation: string;
  userName?: string | null;
};

export type ProjectUpdatePayload = ProjectWritePayload & {
  status: MyProjectStatus;
};

function toWritePayload(values: ProjectFormValues): ProjectWritePayload {
  return {
    title: values.title.trim(),
    researchAreas: values.researchAreas,
    irbStage: values.irbStage,
    briefDescription: values.briefDescription.trim(),
    activityTypes: values.activityTypes,
    volunteersRequired: Number(values.volunteersRequired),
    minQualifications: values.minQualifications.trim() || null,
    additionalComments: values.additionalComments.trim() || null,
    affiliation: values.affiliation.trim(),
    userName: values.userName.trim() || null,
  };
}

export function toMyProject(dto: ProjectDto): MyProject {
  return {
    id: dto.id,
    title: dto.title,
    researchArea: dto.researchAreas.join(", "),
    activityType: dto.activityTypes.join(", "),
    volunteersRequired: dto.volunteersRequired,
    status: dto.status,
    updatedAt: formatProjectDate(dto.updatedAt),
  };
}

export function toCatalogProject(dto: ProjectDto): CatalogProject {
  return {
    id: dto.id,
    title: dto.title,
    researchArea: dto.researchAreas.join(", "),
    activityType: dto.activityTypes.join(", "),
    volunteersRequired: dto.volunteersRequired,
    volunteersFilled: dto.volunteersFilled,
    status: dto.status,
    postedAt: formatProjectDate(dto.createdAt),
    postedAtISO: dto.createdAt.slice(0, 10),
    facultyName: dto.facultyName,
    affiliation: dto.affiliation,
    description: dto.briefDescription,
    minQualifications: dto.minQualifications ?? undefined,
    additionalComments: dto.additionalComments ?? undefined,
    irbStage: dto.irbStageLabel,
  };
}

export function toFormValues(dto: ProjectDto): ProjectFormValues {
  return {
    affiliation: dto.affiliation,
    userName: dto.userName ?? "",
    title: dto.title,
    researchAreas: [...dto.researchAreas],
    irbStage: dto.irbStage,
    briefDescription: dto.briefDescription,
    activityTypes: [...dto.activityTypes],
    volunteersRequired: String(dto.volunteersRequired),
    minQualifications: dto.minQualifications ?? "",
    additionalComments: dto.additionalComments ?? "",
    status: dto.status,
  };
}

export async function listProjects(
  options: { mine?: boolean; pageSize?: number } = {},
): Promise<ProjectDto[]> {
  const params = new URLSearchParams({
    pageNumber: "1",
    pageSize: String(options.pageSize ?? 100),
  });
  if (options.mine) {
    params.set("mine", "true");
  }

  const page = await apiFetch<PaginatedProjects>(
    `/api/projects?${params.toString()}`,
  );
  return page.items;
}

export async function listMyProjects(): Promise<ProjectDto[]> {
  return listProjects({ mine: true });
}

export async function getProject(id: string): Promise<ProjectDto> {
  return apiFetch<ProjectDto>(`/api/projects/${id}`);
}

export async function createProject(
  values: ProjectFormValues,
): Promise<ProjectDto> {
  return apiFetch<ProjectDto>("/api/projects", {
    method: "POST",
    body: JSON.stringify(toWritePayload(values)),
  });
}

export async function updateProject(
  id: string,
  values: ProjectFormValues,
): Promise<ProjectDto> {
  const payload: ProjectUpdatePayload = {
    ...toWritePayload(values),
    status: values.status,
  };
  return apiFetch<ProjectDto>(`/api/projects/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export async function deleteProject(id: string): Promise<void> {
  await apiFetch<unknown>(`/api/projects/${id}`, { method: "DELETE" });
}
