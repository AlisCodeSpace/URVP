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
        href: "/admin/news",
        label: "News",
        description: "Stories on the News page and home ticker.",
      },
      {
        href: "/admin/workshops",
        label: "Workshops",
        description: "Sessions, registration links, and card photos.",
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
            href: "/admin/research-activity-types",
            label: "Research Activity Types",
            description: "Activity types faculty choose when posting projects.",
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
