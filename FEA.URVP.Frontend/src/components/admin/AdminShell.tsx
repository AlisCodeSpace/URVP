"use client";

import { useEffect, useState } from "react";
import { AdminSidebar } from "@/components/admin/AdminSidebar";

type AdminShellProps = {
  children: React.ReactNode;
};

export function AdminShell({ children }: AdminShellProps) {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  useEffect(() => {
    if (!sidebarOpen) return;
    const onKey = (event: KeyboardEvent) => {
      if (event.key === "Escape") setSidebarOpen(false);
    };
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, [sidebarOpen]);

  useEffect(() => {
    document.body.classList.toggle("admin-drawer-open", sidebarOpen);
    return () => document.body.classList.remove("admin-drawer-open");
  }, [sidebarOpen]);

  return (
    <div className="admin-shell">
      <AdminSidebar
        open={sidebarOpen}
        onNavigate={() => setSidebarOpen(false)}
      />

      {sidebarOpen ? (
        <button
          type="button"
          className="admin-backdrop"
          aria-label="Close navigation"
          onClick={() => setSidebarOpen(false)}
        />
      ) : null}

      <div className="admin-main">
        <button
          type="button"
          className="admin-menu-btn"
          aria-label="Open navigation"
          onClick={() => setSidebarOpen(true)}
        >
          <span className="admin-menu-icon" aria-hidden>
            <span />
            <span />
            <span />
          </span>
        </button>
        <div className="admin-content">{children}</div>
      </div>
    </div>
  );
}
