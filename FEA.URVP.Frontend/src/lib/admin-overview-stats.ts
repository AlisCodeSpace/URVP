import type { AdminOverviewDto } from "@/lib/admin-overview-api";
import { formatScheduleRange } from "@/lib/semesters-api";

export type AdminKpi = {
  id: string;
  label: string;
  value: string;
  hint: string;
  delta?: string;
  href?: string;
};

export type AdminChartTone = "primary" | "secondary" | "muted" | "soft";

export type AdminChartSlice = {
  label: string;
  value: number;
  tone: AdminChartTone;
  note?: string;
};

export type AdminBreakdownItem = {
  label: string;
  value: number;
  tone: AdminChartTone;
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
      label: "Assigned students",
      value: formatCount(matching.confirmedPlacements),
      hint: "Confirmed onto projects this cycle",
      delta: run
        ? `Latest test run ${run.status}${run.warningCount > 0 ? ` · ${run.warningCount} warning${run.warningCount === 1 ? "" : "s"}` : ""}`
        : undefined,
      href: "/admin/projects",
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

const PIPELINE_TONES: AdminChartTone[] = [
  "primary",
  "secondary",
  "soft",
  "muted",
];

export function buildPipelineChart(overview: AdminOverviewDto): AdminChartSlice[] {
  return overview.pipeline.map((step, index) => ({
    label: step.label,
    value: step.count,
    tone: PIPELINE_TONES[index % PIPELINE_TONES.length],
    note: step.note,
  }));
}

export function buildProjectStatusChart(
  overview: AdminOverviewDto,
): AdminChartSlice[] {
  return [
    { label: "Open", value: overview.projects.open, tone: "primary" },
    { label: "Matching", value: overview.projects.matching, tone: "secondary" },
    { label: "Closed", value: overview.projects.closed, tone: "muted" },
  ];
}

export function buildSeatChart(overview: AdminOverviewDto): AdminChartSlice[] {
  return [
    { label: "Filled", value: overview.projects.seatsFilled, tone: "primary" },
    { label: "Remaining", value: overview.projects.seatsRemaining, tone: "muted" },
  ];
}

export function buildPlacementChart(
  overview: AdminOverviewDto,
): AdminChartSlice[] {
  return [
    {
      label: "Confirmed",
      value: overview.matching.confirmedPlacements,
      tone: "primary",
    },
    {
      label: "Declined",
      value: overview.matching.declinedPlacements,
      tone: "secondary",
    },
    {
      label: "Cancelled",
      value: overview.matching.cancelledPlacements,
      tone: "muted",
    },
  ];
}

export function buildRankingChart(overview: AdminOverviewDto): AdminChartSlice[] {
  return [
    {
      label: "With ≥1 rank",
      value: overview.rankings.studentsWithRank,
      tone: "primary",
    },
    {
      label: "Full slate of 3",
      value: overview.rankings.studentsWithFullSlate,
      tone: "secondary",
    },
    {
      label: "Unreachable",
      value: overview.rankings.unreachableStudents,
      tone: "muted",
    },
  ];
}

export function chartTotal(slices: AdminChartSlice[]): number {
  return slices.reduce((sum, item) => sum + item.value, 0);
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
  if (!semester) return "No URVP cycle is running";
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
