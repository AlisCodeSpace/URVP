import { apiFetch } from "@/lib/api";

export type MatchingRunStatus = "Draft" | "Confirmed" | "Discarded";

export type PlacementStatus = "Proposed" | "Confirmed" | "Declined" | "Cancelled";

export type MatchingRunDto = {
  id: string;
  semesterId: string;
  semesterName: string;
  status: MatchingRunStatus;
  algorithmVersion: string;
  seed: number;
  studentsConsidered: number;
  projectsConsidered: number;
  seatsAvailable: number;
  studentsMatched: number;
  tieBreaksUsed: number;
  warningCount: number;
  createdAt: string;
  confirmedAt?: string | null;
};

export type PlacementDto = {
  id: string;
  projectId: string;
  projectTitle: string;
  facultyName: string;
  studentUserId: string;
  studentName: string;
  studentEmail: string;
  studentRank: number;
  facultyRank: number;
  resolvedByTieBreak: boolean;
  status: PlacementStatus;
  updatedAt: string;
};

export type MatchingRunDetailDto = {
  run: MatchingRunDto;
  warnings: string[];
  placements: PlacementDto[];
};

export type RunMatchingPayload = {
  semesterId?: string | null;
  seed?: number | null;
};

export async function listMatchingRuns(
  semesterId?: string,
): Promise<MatchingRunDto[]> {
  const query = semesterId ? `?semesterId=${encodeURIComponent(semesterId)}` : "";
  return apiFetch<MatchingRunDto[]>(`/api/matching/runs${query}`);
}

export async function getMatchingRun(id: string): Promise<MatchingRunDetailDto> {
  return apiFetch<MatchingRunDetailDto>(`/api/matching/runs/${id}`);
}

export async function runMatching(
  payload: RunMatchingPayload = {},
): Promise<MatchingRunDetailDto> {
  return apiFetch<MatchingRunDetailDto>("/api/matching/runs", {
    method: "POST",
    body: JSON.stringify(payload),
  });
}

export async function confirmMatchingRun(id: string): Promise<MatchingRunDetailDto> {
  return apiFetch<MatchingRunDetailDto>(`/api/matching/runs/${id}/confirm`, {
    method: "POST",
  });
}

export async function discardMatchingRun(id: string): Promise<MatchingRunDto> {
  return apiFetch<MatchingRunDto>(`/api/matching/runs/${id}/discard`, {
    method: "POST",
  });
}

export async function updatePlacementStatus(
  id: string,
  status: Extract<PlacementStatus, "Declined" | "Cancelled">,
): Promise<PlacementDto> {
  return apiFetch<PlacementDto>(`/api/matching/placements/${id}/status`, {
    method: "PUT",
    body: JSON.stringify({ status }),
  });
}

export function matchRate(run: MatchingRunDto): number {
  if (run.studentsConsidered <= 0) return 0;
  return Math.round((run.studentsMatched / run.studentsConsidered) * 100);
}
