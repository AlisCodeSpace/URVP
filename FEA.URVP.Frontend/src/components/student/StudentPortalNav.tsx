"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { studentProfileHref, studentRankingsHref } from "@/lib/auth";

const links = [
  { href: studentProfileHref(), label: "My Profile" },
  { href: studentRankingsHref(), label: "Ranked Projects" },
] as const;

export function StudentPortalNav() {
  const pathname = usePathname();

  return (
    <nav aria-label="Student portal" className="student-portal-tabs">
      <div className="student-portal-tabs-track" role="tablist">
        {links.map((link) => {
          const active =
            pathname === link.href || pathname.startsWith(`${link.href}/`);
          return (
            <Link
              key={link.href}
              href={link.href}
              role="tab"
              aria-selected={active}
              className={`student-portal-tab${active ? " is-active" : ""}`}
            >
              {link.label}
            </Link>
          );
        })}
      </div>
    </nav>
  );
}
