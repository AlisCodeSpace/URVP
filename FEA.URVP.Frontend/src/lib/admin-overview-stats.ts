import type { AdminOverviewDto } from "@/lib/admin-overview-api";
import { matchRate } from "@/lib/matching-api";
import { formatScheduleRange } from "@/lib/semesters-api";

export type AdminKpi = {
  id: string;
  label: string;
  value: string;
  hint: string;
  delta?: string;
  href?: string;
};

export type AdminBreakdownItem = {
  label: string;
  value: number;
  tone: "primary" | "secondary" | "muted";
};

export function seatFillPercent(filled: number, required: number): number {
  if (required <= 0) return 0;
  return Math.round((filled / required) * 100);
}

export function formatCount(value: number): string {
  return value.toLocaleString("en-US");
}

export function buildAdminKpis(overview: AdminOverviewDto): AdminKpi[] {
  const { accounts, projects, rankings, matching } = overview;
  const fill = seatFillPercent(projects.seatsFilled, projects.seatsRequired);
  const run = matching.latestRun;

  return [
    {
      id: "students",
      label: "Student profiles",
      value: formatCount(accounts.studentProfiles),
      hint: `${formatCount(accounts.students)} student accounts`,
      delta:
        accounts.studentsWithoutProfile > 0
          ? `${formatCount(accounts.studentsWithoutProfile)} without a profile`
          : accounts.profilesReady > 0
            ? `${formatCount(accounts.profilesReady)} meet eligibility`
            : undefined,
      href: "/admin/users",
    },
    {
      id: "faculty",
      label: "Faculty posting",
      value: formatCount(accounts.facultyWithProjects),
      hint: `${formatCount(accounts.faculty)} faculty accounts`,
      href: "/admin/users",
    },
    {
      id: "projects",
      label: "Open projects",
      value: formatCount(projects.open),
      hint: `${formatCount(projects.matching)} matching · ${formatCount(projects.closed)} closed`,
      delta:
        projects.fullOpenProjects > 0
          ? `${formatCount(projects.fullOpenProjects)} at capacity`
          : undefined,
      href: "/admin/projects",
    },
    {
      id: "rankings",
      label: "Interest rankings",
      value: formatCount(rankings.studentRankingRows),
      hint: `${formatCount(rankings.studentsWithRank)} students with ≥1 rank`,
      delta:
        rankings.studentsWithFullSlate > 0
          ? `${formatCount(rankings.studentsWithFullSlate)} completed a slate of 3`
          : undefined,
      href: "/admin/projects",
    },
    {
      id: "seats",
      label: "Volunteer seats",
      value: `${formatCount(projects.seatsFilled)} / ${formatCount(projects.seatsRequired)}`,
      hint: "Filled of open-project capacity",
      delta:
        projects.seatsRequired > 0
          ? `${fill}% filled · ${formatCount(projects.seatsRemaining)} remaining`
          : "No open seats posted",
    },
    {
      id: "matching",
      label: "Latest matching",
      value: run
        ? `${formatCount(run.studentsMatched)} / ${formatCount(run.studentsConsidered)}`
        : "No run yet",
      hint: run
        ? `${matchRate(run)}% of students considered`
        : "Run matching when rankings are ready",
      delta: run
        ? `${run.status}${run.warningCount > 0 ? ` · ${run.warningCount} warning${run.warningCount === 1 ? "" : "s"}` : ""}`
        : undefined,
      href: "/admin/matching",
    },
  ];
}

export function buildRoleBreakdown(
  overview: AdminOverviewDto,
): AdminBreakdownItem[] {
  return [
    { label: "Students", value: overview.accounts.students, tone: "primary" },
    { label: "Faculty", value: overview.accounts.faculty, tone: "secondary" },
    { label: "Admins", value: overview.accounts.admins, tone: "muted" },
  ];
}

export function catalogTiles(overview: AdminOverviewDto) {
  return [
    {
      label: "Activity types",
      value: overview.catalog.researchActivityTypes,
      href: "/admin/research-activity-types",
    },
    {
      label: "Interests",
      value: overview.catalog.researchInterests,
      href: "/admin/research-interests",
    },
    {
      label: "Workshops",
      value: overview.catalog.workshops,
      href: "/admin/workshops",
    },
    {
      label: "News",
      value: overview.catalog.news,
      href: "/admin/news",
    },
  ] as const;
}

export function semesterChipTitle(overview: AdminOverviewDto): string {
  const semester = overview.semester;
  if (!semester) return "No academic cycle is running";
  return formatScheduleRange(semester.cycleStart, semester.cycleEnd);
}

export function profileWindowLabel(overview: AdminOverviewDto): string {
  const semester = overview.semester;
  if (!semester) return "No application window";
  if (semester.isApplicationWindowOpen) {
    return `Window open · ${formatScheduleRange(semester.applicationWindowStart, semester.applicationWindowEnd)}`;
  }
  return `Window closed · ${formatScheduleRange(semester.applicationWindowStart, semester.applicationWindowEnd)}`;
}
