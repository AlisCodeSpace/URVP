export type AdminNavItem = {
  href: string;
  label: string;
  description: string;
};

export type AdminNavGroup = {
  id: string;
  label: string;
  items: AdminNavItem[];
  /** Nested under another group (e.g. Value Lists under Administration). */
  children?: AdminNavGroup[];
};

export const adminNav: AdminNavGroup[] = [
  {
    id: "administration",
    label: "Administration",
    items: [
      {
        href: "/admin/users",
        label: "Users",
        description: "Accounts, roles, and access.",
      },
      {
        href: "/admin/projects",
        label: "Projects",
        description: "All listings and student ranking interest.",
      },
      {
        href: "/admin/faculties",
        label: "Faculties",
        description: "Faculty units across the university.",
      },
      {
        href: "/admin/divisions",
        label: "Divisions",
        description: "Departments and academic divisions.",
      },
      {
        href: "/admin/certificates",
        label: "Certificates",
        description: "Certificate programs and credentials.",
      },
      {
        href: "/admin/associations",
        label: "Associations",
        description: "Student and research associations.",
      },
      {
        href: "/admin/semesters",
        label: "Semesters",
        description: "Academic terms and program windows.",
      },
    ],
    children: [
      {
        id: "value-lists",
        label: "Value Lists",
        items: [
          {
            href: "/admin/research-interests",
            label: "Research Interests",
            description: "Student and faculty research interests.",
          },
          {
            href: "/admin/research-areas",
            label: "Research Areas",
            description: "Canonical research area taxonomy.",
          },
          {
            href: "/admin/courses",
            label: "Courses",
            description: "Course catalog entries used in matching.",
          },
          {
            href: "/admin/majors",
            label: "Majors",
            description: "Degree majors and concentrations.",
          },
        ],
      },
    ],
  },
];

export function flattenAdminNav(): AdminNavItem[] {
  const items: AdminNavItem[] = [];
  for (const group of adminNav) {
    items.push(...group.items);
    for (const child of group.children ?? []) {
      items.push(...child.items);
    }
  }
  return items;
}

export function findAdminNavItem(pathname: string): AdminNavItem | undefined {
  return flattenAdminNav().find(
    (item) => pathname === item.href || pathname.startsWith(`${item.href}/`),
  );
}
