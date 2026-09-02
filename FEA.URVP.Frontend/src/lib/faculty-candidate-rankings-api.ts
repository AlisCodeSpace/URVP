import { apiFetch } from "@/lib/api";
import type { RankOption } from "@/lib/project-rankings-api";

export type FacultyCandidateRankingDto = {
  id: string;
  projectId: string;
  studentUserId: string;
  rank: number;
  rankedAt: string;
  updatedAt: string;
};

export async function upsertFacultyCandidateRanking(
  projectId: string,
  studentUserId: string,
  rank: RankOption,
): Promise<FacultyCandidateRankingDto> {
  return apiFetch<FacultyCandidateRankingDto>("/api/faculty-candidate-rankings", {
    method: "PUT",
    body: JSON.stringify({ projectId, studentUserId, rank }),
  });
}

export async function removeFacultyCandidateRanking(
  projectId: string,
  studentUserId: string,
): Promise<void> {
  await apiFetch<null>(
    `/api/faculty-candidate-rankings/${projectId}/${studentUserId}`,
    { method: "DELETE" },
  );
}
