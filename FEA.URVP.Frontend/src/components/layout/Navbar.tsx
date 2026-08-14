"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import { useAuth } from "@/components/auth/AuthProvider";
import { UserMenu } from "@/components/auth/UserMenu";
import { Button } from "@/components/ui/Button";
import { Logo } from "@/components/ui/Logo";
import { navLinks } from "@/lib/site";

export function Navbar() {
  const pathname = usePathname();
  const [open, setOpen] = useState(false);
  const { status, loading } = useAuth();
  const isSignedIn = Boolean(status?.isAuthenticated);

  const overlayNav =
    pathname === "/" ||
    pathname === "/workshops" ||
    pathname === "/research-day" ||
    pathname === "/news" ||
    pathname === "/contact" ||
    pathname === "/projects" ||
    pathname.startsWith("/projects/") ||
    pathname.startsWith("/student") ||
    pathname.startsWith("/my-projects") ||
    pathname.startsWith("/news/");

  return (
    <header
      className={
        overlayNav
          ? "absolute inset-x-0 top-0 z-50"
          : "sticky top-0 z-50 border-b border-primary/10 bg-primary-deep/95 backdrop-blur-md"
      }
    >
      <div className="site-container flex items-center justify-between gap-4 py-4">
        <Logo
          href="/"
          size={48}
          className="text-white"
          onClick={() => setOpen(false)}
        />

        <nav
          aria-label="Primary"
          className="hidden items-center gap-1 lg:flex"
        >
          <Link
            href="/"
            className={`rounded-md px-3 py-2 text-[0.95rem] font-semibold transition ${
              pathname === "/"
                ? "text-secondary"
                : "text-white/85 hover:text-white"
            }`}
          >
            Home
          </Link>
          {navLinks.map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className={`rounded-md px-3 py-2 text-[0.95rem] font-semibold transition ${
                pathname === link.href
                  ? "text-secondary"
                  : "text-white/85 hover:text-white"
              }`}
            >
              {link.label}
            </Link>
          ))}
        </nav>

        <div className="hidden items-center gap-3 lg:flex">
          {loading ? null : isSignedIn && status ? (
            <UserMenu status={status} />
          ) : (
            <Button href="/sign-in" variant="secondary" size="sm">
              Log In
            </Button>
          )}
        </div>

        <button
          type="button"
          className="inline-flex h-10 w-10 items-center justify-center rounded-md border border-white/25 text-white lg:hidden"
          aria-expanded={open}
          aria-controls="mobile-nav"
          aria-label={open ? "Close menu" : "Open menu"}
          onClick={() => setOpen((v) => !v)}
        >
          <span className="sr-only">Menu</span>
          <span aria-hidden className="flex flex-col gap-1.5">
            <span
              className={`block h-0.5 w-5 bg-current transition ${open ? "translate-y-2 rotate-45" : ""}`}
            />
            <span
              className={`block h-0.5 w-5 bg-current transition ${open ? "opacity-0" : ""}`}
            />
            <span
              className={`block h-0.5 w-5 bg-current transition ${open ? "-translate-y-2 -rotate-45" : ""}`}
            />
          </span>
        </button>
      </div>

      {open ? (
        <div
          id="mobile-nav"
          className="border-t border-white/10 bg-primary-deep px-6 py-5 lg:hidden"
        >
          <nav aria-label="Mobile" className="flex flex-col gap-1">
            <Link
              href="/"
              className="py-2.5 text-[0.95rem] font-semibold text-white/90"
              onClick={() => setOpen(false)}
            >
              Home
            </Link>
            {navLinks.map((link) => (
              <Link
                key={link.href}
                href={link.href}
                className="py-2.5 text-[0.95rem] font-semibold text-white/90"
                onClick={() => setOpen(false)}
              >
                {link.label}
              </Link>
            ))}
          </nav>
          <div className="mt-4">
            {loading ? null : isSignedIn && status ? (
              <UserMenu
                status={status}
                variant="mobile"
                onNavigate={() => setOpen(false)}
              />
            ) : (
              <Button
                href="/sign-in"
                variant="secondary"
                size="md"
                className="w-full"
                onClick={() => setOpen(false)}
              >
                Log In
              </Button>
            )}
          </div>
        </div>
      ) : null}
    </header>
  );
}
