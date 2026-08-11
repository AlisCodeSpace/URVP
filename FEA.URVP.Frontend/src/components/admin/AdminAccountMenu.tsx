"use client";

import { useEffect, useId, useRef, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/components/auth/AuthProvider";
import { roleLabel } from "@/lib/auth";

type AdminAccountMenuProps = {
  onNavigate?: () => void;
};

export function AdminAccountMenu({ onNavigate }: AdminAccountMenuProps) {
  const { status, signOut } = useAuth();
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);
  const menuId = useId();

  const name = status?.name?.trim() || "Admin";
  const email = status?.email?.trim() || "—";
  const role = roleLabel(status?.role);
  const initial = (name.charAt(0) || "A").toUpperCase();

  useEffect(() => {
    if (!open) return;

    const onPointerDown = (event: MouseEvent) => {
      if (!rootRef.current?.contains(event.target as Node)) setOpen(false);
    };
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") setOpen(false);
    };

    document.addEventListener("mousedown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("mousedown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open]);

  return (
    <div ref={rootRef} className="admin-account">
      <button
        type="button"
        className="admin-account-btn"
        aria-expanded={open}
        aria-haspopup="menu"
        aria-controls={menuId}
        onClick={() => setOpen((v) => !v)}
      >
        <span className="admin-account-avatar" aria-hidden>
          {initial}
        </span>
        <span className="admin-account-meta">
          <span className="admin-account-name">{name}</span>
          <span className="admin-account-role">{role}</span>
        </span>
        <span aria-hidden className="admin-account-caret">
          {open ? "◂" : "▸"}
        </span>
      </button>

      {open ? (
        <div id={menuId} role="menu" className="admin-account-menu">
          <div className="admin-account-menu-head">
            <p className="truncate text-sm font-semibold text-foreground">{name}</p>
            <p className="truncate text-xs text-muted">{email}</p>
          </div>
          <div className="flex flex-col gap-1 p-2">
            <Link
              href="/"
              role="menuitem"
              className="btn btn-outline btn-sm w-full"
              onClick={() => {
                setOpen(false);
                onNavigate?.();
              }}
            >
              View site
            </Link>
            <button
              type="button"
              role="menuitem"
              className="btn btn-danger btn-sm w-full"
              onClick={() => {
                setOpen(false);
                signOut();
              }}
            >
              Sign out
            </button>
          </div>
        </div>
      ) : null}
    </div>
  );
}
