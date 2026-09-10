export type AdminNavItem = {
  href: string;
  label: string;
  description: string;
};

export type AdminNavGroup = {
  id: string;
  label: string;
  items: AdminNavItem[];
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
        description: "Listings, rankings, and student assignments.",
      },
      {
        href: "/admin/semesters",
        label: "URVP Cycles",
        description: "Program cycles and application windows.",
      },
      {
        href: "/admin/email",
        label: "Email",
        description: "SMTP credentials for outgoing mail.",
      },
      {
        href: "/admin/notifications",
        label: "Notifications",
        description: "In-app updates for this admin account.",
      },
    ],
  },
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
        label: "Activity Types",
        description: "Activity types faculty choose when posting projects.",
      },
    ],
  },
  {
    id: "miscellaneous",
    label: "Miscellaneous",
    items: [
      {
        href: "/admin/home-intro",
        label: "Home",
        description: "Homepage headline, description, and key points.",
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
    ],
  },
];

export function flattenAdminNav(): AdminNavItem[] {
  return adminNav.flatMap((group) => group.items);
}

export function findAdminNavItem(pathname: string): AdminNavItem | undefined {
  return flattenAdminNav().find(
    (item) => pathname === item.href || pathname.startsWith(`${item.href}/`),
  );
}
