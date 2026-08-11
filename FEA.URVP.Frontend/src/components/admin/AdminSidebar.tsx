"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { AdminAccountMenu } from "@/components/admin/AdminAccountMenu";
import { Logo } from "@/components/ui/Logo";
import { adminNav, type AdminNavGroup } from "@/lib/admin-nav";

type AdminSidebarProps = {
  open: boolean;
  onNavigate?: () => void;
};

function NavLink({
  href,
  label,
  active,
  nested,
  onNavigate,
}: {
  href: string;
  label: string;
  active: boolean;
  nested?: boolean;
  onNavigate?: () => void;
}) {
  return (
    <Link
      href={href}
      onClick={onNavigate}
      className={`admin-nav-link${active ? " is-active" : ""}${nested ? " is-nested" : ""}`}
      aria-current={active ? "page" : undefined}
    >
      <span className="admin-nav-dot" aria-hidden />
      {label}
    </Link>
  );
}

function NavGroup({
  group,
  pathname,
  onNavigate,
  nested,
}: {
  group: AdminNavGroup;
  pathname: string;
  onNavigate?: () => void;
  nested?: boolean;
}) {
  return (
    <div className={`admin-nav-group${nested ? " is-nested-group" : ""}`}>
      <p className="admin-nav-group-label">{group.label}</p>
      <ul className="admin-nav-list">
        {group.items.map((item) => {
          const active =
            pathname === item.href || pathname.startsWith(`${item.href}/`);
          return (
            <li key={item.href}>
              <NavLink
                href={item.href}
                label={item.label}
                active={active}
                nested={nested}
                onNavigate={onNavigate}
              />
            </li>
          );
        })}
      </ul>
      {group.children?.map((child) => (
        <NavGroup
          key={child.id}
          group={child}
          pathname={pathname}
          onNavigate={onNavigate}
          nested
        />
      ))}
    </div>
  );
}

export function AdminSidebar({ open, onNavigate }: AdminSidebarProps) {
  const pathname = usePathname();
  const overviewActive = pathname === "/admin";

  return (
    <aside
      className={`admin-sidebar${open ? " is-open" : ""}`}
      aria-label="Admin navigation"
    >
      <div className="admin-sidebar-brand">
        <Logo
          href="/admin"
          size={36}
          className="admin-sidebar-logo text-white"
          onClick={onNavigate}
        />
        <p className="admin-sidebar-tag">Administration</p>
      </div>

      <nav className="admin-sidebar-nav">
        <Link
          href="/admin"
          onClick={onNavigate}
          className={`admin-nav-link admin-nav-overview${overviewActive ? " is-active" : ""}`}
          aria-current={overviewActive ? "page" : undefined}
        >
          <span className="admin-nav-dot" aria-hidden />
          Overview
        </Link>

        {adminNav.map((group) => (
          <NavGroup
            key={group.id}
            group={group}
            pathname={pathname}
            onNavigate={onNavigate}
          />
        ))}
      </nav>

      <div className="admin-sidebar-foot">
        <AdminAccountMenu onNavigate={onNavigate} />
      </div>
    </aside>
  );
}
