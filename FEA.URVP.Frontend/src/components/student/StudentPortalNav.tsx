"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import {
  studentProfileHref,
  studentProjectsHref,
  studentRankingsHref,
} from "@/lib/auth";

const links = [
  { href: studentProfileHref(), label: "My Profile" },
  { href: studentProjectsHref(), label: "Projects" },
  { href: studentRankingsHref(), label: "My Rankings" },
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
