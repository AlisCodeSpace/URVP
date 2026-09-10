"use client";

import Link from "next/link";
import { useAuth } from "@/components/auth/AuthProvider";
import { Button } from "@/components/ui/Button";
import { PageHero } from "@/components/layout/PageHero";
import {
  isAdmin,
  isStudent,
  myProjectsHref,
  projectsHref,
  studentProfileHref,
} from "@/lib/auth";
import {
  formatApplicationAnnouncement,
  useApplicationWindow,
} from "@/hooks/useApplicationWindow";

export function Hero() {
  const { status, loading } = useAuth();
  const appWindow = useApplicationWindow();
  const isSignedIn = Boolean(status?.isAuthenticated);
  const role = status?.role;
  const userId = status?.userId;

  const showStudentCtas = !loading && (!isSignedIn || isStudent(role));
  const applyHref = isSignedIn ? studentProfileHref() : "/sign-in";
  const showApplyLink = showStudentCtas && !appWindow.loading && appWindow.isOpen;

  return (
    <PageHero
      title="Undergraduate Research Volunteer Program"
      titleScale="brand"
      headline="Match with faculty research. Shape your academic path."
      announcement={
        appWindow.loading ? undefined : (
          <>
            {formatApplicationAnnouncement(appWindow.semesterName, appWindow.isOpen)}
            {showApplyLink ? (
              <>
                {" "}
                <Link
                  href={applyHref}
                  className="underline decoration-secondary/70 underline-offset-4 transition hover:text-white hover:decoration-white"
                >
                  Apply now!
                </Link>
              </>
            ) : null}
          </>
        )
      }
      actions={
        <>
          {loading ? null : isSignedIn ? (
            <Button href={primaryHref(role, userId)} variant="secondary" size="lg">
              {primaryLabel(role)}
            </Button>
          ) : (
            <Button href="/sign-in" variant="secondary" size="lg">
              Log In
            </Button>
          )}
          {loading ? null : (
            <Button href={projectsHref()} variant="outline-light" size="lg">
              {isStudent(role) ? "Apply to Projects" : "Browse Projects"}
            </Button>
          )}
          {loading || isSignedIn ? null : (
            <Button href="/my-projects" variant="outline-light" size="lg">
              Faculty Portal
            </Button>
          )}
        </>
      }
    />
  );
}

function primaryHref(role: number | null | undefined, userId?: string | null) {
  if (isAdmin(role)) return "/admin";
  if (isStudent(role)) return studentProfileHref();
  if (userId) return myProjectsHref(userId);
  return "/my-projects";
}

function primaryLabel(role: number | null | undefined) {
  if (isAdmin(role)) return "Admin Console";
  if (isStudent(role)) return "Student Portal";
  return "Faculty Portal";
}
