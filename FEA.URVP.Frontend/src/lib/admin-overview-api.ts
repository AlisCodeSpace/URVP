import { apiFetch } from "@/lib/api";
import type { MatchingRunDto } from "@/lib/matching-api";
import type { SemesterDto } from "@/lib/semesters-api";

export type AdminOverviewAccounts = {
  students: number;
  studentProfiles: number;
  studentsWithoutProfile: number;
  profilesReady: number;
  faculty: number;
  facultyWithProjects: number;
  admins: number;
};

export type AdminOverviewProjects = {
  open: number;
  matching: number;
  closed: number;
  seatsRequired: number;
  seatsFilled: number;
  seatsRemaining: number;
  fullOpenProjects: number;
  openWithoutStudentRanks: number;
  applicantsWithoutFacultyRanks: number;
};

export type AdminOverviewRankings = {
  studentRankingRows: number;
  studentsWithRank: number;
  studentsWithFullSlate: number;
  unreachableStudents: number;
};

export type AdminOverviewMatching = {
  latestRun: MatchingRunDto | null;
  confirmedPlacements: number;
  declinedPlacements: number;
  cancelledPlacements: number;
};

export type AdminOverviewCatalog = {
  researchInterests: number;
  researchActivityTypes: number;
  workshops: number;
  news: number;
};

export type AdminOverviewPipelineStep = {
  id: string;
  label: string;
  count: number;
  note: string;
};

export type AdminOverviewAttentionItem = {
  id: string;
  text: string;
  href: string;
  severity: "warning" | "info" | string;
};

export type AdminOverviewActivityItem = {
  id: string;
  text: string;
  meta: string;
  at: string;
};

export type AdminOverviewDto = {
  semester: SemesterDto | null;
  accounts: AdminOverviewAccounts;
  projects: AdminOverviewProjects;
  rankings: AdminOverviewRankings;
  matching: AdminOverviewMatching;
  catalog: AdminOverviewCatalog;
  pipeline: AdminOverviewPipelineStep[];
  attention: AdminOverviewAttentionItem[];
  recentActivity: AdminOverviewActivityItem[];
};

export async function getAdminOverview(): Promise<AdminOverviewDto> {
  return apiFetch<AdminOverviewDto>("/api/admin/overview");
}
