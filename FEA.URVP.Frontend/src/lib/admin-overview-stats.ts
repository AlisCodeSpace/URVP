/** Mock admin overview metrics — replace with API later. */

export type AdminKpi = {
  id: string;
  label: string;
  value: string;
  hint: string;
  delta?: string;
  href?: string;
};

export type AdminFillMeter = {
  label: string;
  filled: number;
  total: number;
};

export type AdminBreakdownItem = {
  label: string;
  value: number;
  tone: "primary" | "secondary" | "muted";
};

export type AdminPipelineStep = {
  label: string;
  count: number;
  note: string;
};

export type AdminActivityItem = {
  id: string;
  text: string;
  meta: string;
};

export const adminOverviewMeta = {
  semester: "Fall 2025–26",
  cycle: "AY 2025–26 matching cycle",
  cycleWindow: "Oct 13, 2025 – Aug 21, 2026",
  profileWindow: "Aug 25 – Sep 30, 2025",
} as const;

export const adminKpis: AdminKpi[] = [
  {
    id: "students",
    label: "Student profiles",
    value: "642",
    hint: "Active undergraduates",
    delta: "+48 this week",
    href: "/admin/users",
  },
  {
    id: "faculty",
    label: "Faculty mentors",
    value: "118",
    hint: "Posting or mentoring",
    delta: "+6 this week",
    href: "/admin/users",
  },
  {
    id: "projects",
    label: "Open projects",
    value: "87",
    hint: "Listed for matching",
    delta: "12 draft",
    href: "/admin/projects",
  },
  {
    id: "rankings",
    label: "Interest rankings",
    value: "1,204",
    hint: "Student project ranks",
    delta: "+93 today",
  },
  {
    id: "seats",
    label: "Volunteer seats",
    value: "214 / 310",
    hint: "Filled of capacity",
    delta: "69% filled",
  },
  {
    id: "workshops",
    label: "Workshop seats",
    value: "156",
    hint: "Registrations this term",
    delta: "3 sessions",
  },
];

export const adminSeatMeters: AdminFillMeter[] = [
  { label: "Engineering & architecture", filled: 72, total: 96 },
  { label: "Arts & sciences", filled: 58, total: 84 },
  { label: "Health sciences", filled: 41, total: 60 },
  { label: "Centers & institutes", filled: 43, total: 70 },
];

export const adminRoleBreakdown: AdminBreakdownItem[] = [
  { label: "Students", value: 642, tone: "primary" },
  { label: "Faculty", value: 118, tone: "secondary" },
  { label: "Admins", value: 4, tone: "muted" },
];

export const adminPipeline: AdminPipelineStep[] = [
  {
    label: "Profiles complete",
    count: 518,
    note: "Ready for matching",
  },
  {
    label: "Projects live",
    count: 87,
    note: "Visible in catalog",
  },
  {
    label: "Rankings submitted",
    count: 391,
    note: "Students with ≥1 rank",
  },
  {
    label: "Matched placements",
    count: 214,
    note: "Seats confirmed",
  },
];

export const adminCatalogCounts = [
  { label: "Faculties", value: 7, href: "/admin/faculties" },
  { label: "Divisions", value: 24, href: "/admin/divisions" },
  { label: "Majors", value: 61, href: "/admin/majors" },
  { label: "Courses", value: 412, href: "/admin/courses" },
  { label: "Research areas", value: 38, href: "/admin/research-areas" },
  { label: "Interests", value: 96, href: "/admin/research-interests" },
] as const;

export const adminRecentActivity: AdminActivityItem[] = [
  {
    id: "1",
    text: "New project posted — Photonic sensing for structural health",
    meta: "Faculty · 12 min ago",
  },
  {
    id: "2",
    text: "48 student profiles updated in the profile window",
    meta: "Students · 1 hr ago",
  },
  {
    id: "3",
    text: "Workshop registration opened — Meeting Your PI",
    meta: "Workshops · 3 hr ago",
  },
  {
    id: "4",
    text: "Semester Fall 2025–26 marked active",
    meta: "Semesters · Yesterday",
  },
];
