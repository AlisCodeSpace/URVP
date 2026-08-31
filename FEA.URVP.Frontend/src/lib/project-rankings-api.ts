import { apiFetch } from "@/lib/api";

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

export type ProjectRankingDto = {
  id: string;
  projectId: string;
  rank: number;
  projectTitle: string;
  facultyName: string;
  facultyAffiliation: string;
  researchAreas: string[];
  projectStatus: number;
  rankedAt: string;
  updatedAt: string;
};

export const RANK_OPTIONS = [1, 2, 3] as const;
export type RankOption = (typeof RANK_OPTIONS)[number];

export async function getMyProjectRankings(): Promise<ProjectRankingDto[]> {
  return apiFetch<ProjectRankingDto[]>("/api/project-rankings/me");
}

export async function getProjectRankings(
  projectId: string,
): Promise<ProjectRankingStudentDto[]> {
  return apiFetch<ProjectRankingStudentDto[]>(
    `/api/projects/${projectId}/rankings`,
  );
}

export async function upsertProjectRanking(
  projectId: string,
  rank: RankOption,
): Promise<ProjectRankingDto> {
  return apiFetch<ProjectRankingDto>("/api/project-rankings", {
    method: "PUT",
    body: JSON.stringify({ projectId, rank }),
  });
}

export async function removeProjectRanking(projectId: string): Promise<void> {
  await apiFetch<null>(`/api/project-rankings/${projectId}`, {
    method: "DELETE",
  });
}

export function formatRankedAt(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return iso;
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}

export function rankLabel(rank: number): string {
  if (rank === 1) return "1st choice";
  if (rank === 2) return "2nd choice";
  if (rank === 3) return "3rd choice";
  return `Rank ${rank}`;
}
