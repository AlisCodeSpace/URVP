"use client";

import Link from "next/link";
import { useEffect, useId, useRef, useState } from "react";
import { useAuth } from "@/components/auth/AuthProvider";
import { useTour } from "@/components/tour/TourProvider";
import { portalHref, roleLabel, type AuthStatus } from "@/lib/auth";
import { canTakeTour } from "@/lib/tour";

type UserMenuProps = {
  status: AuthStatus;
  /** Compact trigger for desktop nav; full-width actions for mobile drawer. */
  variant?: "desktop" | "mobile";
  onNavigate?: () => void;
};

export function UserMenu({ status, variant = "desktop", onNavigate }: UserMenuProps) {
  const { signOut } = useAuth();
  const tour = useTour();
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);
  const menuId = useId();

  const name = status.name?.trim() || "Account";
  const email = status.email?.trim() || "—";
  const role = roleLabel(status.role);
  const portal = portalHref(status.role, status.userId);
  const initial = (name.charAt(0) || "U").toUpperCase();
  const showTour = Boolean(tour && canTakeTour(status.role));

  function startTour() {
    setOpen(false);
    onNavigate?.();
    tour?.start();
  }

  useEffect(() => {
    if (!open) return;

    const onPointerDown = (event: MouseEvent) => {
      if (!rootRef.current?.contains(event.target as Node)) {
        setOpen(false);
      }
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

  if (variant === "mobile") {
    return (
      <div className="space-y-3 rounded-lg border border-white/25 bg-white/10 p-4 backdrop-blur-md">
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold text-white">{name}</p>
          <p className="truncate text-xs text-white/65">{email}</p>
          <p className="mt-1 text-xs font-medium uppercase tracking-wide text-secondary">
            {role}
          </p>
        </div>
        <Link
          href={portal}
          className="btn btn-secondary btn-md w-full"
          onClick={onNavigate}
        >
          Portal
        </Link>
        {showTour ? (
          <button
            type="button"
            className="btn btn-md w-full border border-white/25 bg-white/10 text-white hover:bg-white/20"
            onClick={startTour}
          >
            Take a tour
          </button>
        ) : null}
        <div className="border-t border-white/20 pt-3">
          <button
            type="button"
            className="btn btn-danger btn-md w-full"
            onClick={() => {
              onNavigate?.();
              signOut();
            }}
          >
            Sign out
          </button>
        </div>
      </div>
    );
  }

  return (
    <div ref={rootRef} className="relative inline-flex self-stretch overflow-visible">
      <button
        type="button"
        className="inline-flex h-full items-center gap-2 rounded-md border border-white/25 bg-white/5 px-2.5 py-1.5 text-sm font-semibold text-white transition hover:bg-white/10"
        aria-expanded={open}
        aria-haspopup="menu"
        aria-controls={menuId}
        onClick={() => setOpen((v) => !v)}
      >
        <span
          className="inline-flex h-8 w-8 items-center justify-center rounded-full bg-secondary text-sm font-bold text-primary-deep"
          aria-hidden
        >
          {initial}
        </span>
        Profile
        <span aria-hidden className="text-white/70">
          {open ? "▴" : "▾"}
        </span>
      </button>

      {open ? (
        <div
          id={menuId}
          role="menu"
          className="absolute top-full right-0 left-auto z-50 mt-2 w-64 overflow-hidden rounded-lg border border-white/25 bg-white/10 shadow-lg backdrop-blur-md"
        >
          <div className="border-b border-white/15 px-4 py-3">
            <p className="truncate text-sm font-semibold text-white">{name}</p>
            <p className="truncate text-xs text-white/65">{email}</p>
            <p className="mt-1 text-xs font-medium uppercase tracking-wide text-secondary">
              {role}
            </p>
          </div>
          <div className="flex flex-col gap-1 p-2">
            <Link
              href={portal}
              role="menuitem"
              className="btn btn-secondary btn-sm w-full"
              onClick={() => {
                setOpen(false);
                onNavigate?.();
              }}
            >
              Portal
            </Link>
            {showTour ? (
              <button
                type="button"
                role="menuitem"
                className="btn btn-sm w-full border border-white/25 bg-white/10 text-white hover:bg-white/20"
                onClick={startTour}
              >
                Take a tour
              </button>
            ) : null}
            <div className="mt-1 border-t border-white/20 pt-2">
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
        </div>
      ) : null}
    </div>
  );
}
