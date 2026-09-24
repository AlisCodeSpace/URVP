"use client";

import { useAuth } from "@/components/auth/AuthProvider";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { isStudent } from "@/lib/auth";
import { getActiveSemester } from "@/lib/semesters-api";

export type WindowStatus =
  | { loading: true; isOpen: false; semesterName: null }
  | { loading: false; isOpen: boolean; semesterName: string | null };

let inflight: Promise<WindowStatus> | null = null;

function fetchWindowStatus(): Promise<WindowStatus> {
  if (inflight) return inflight;

  const request = getActiveSemester()
    .then((sem) => {
      const next: WindowStatus = {
        loading: false,
        isOpen: sem?.isApplicationWindowOpen ?? false,
        semesterName: sem?.name?.trim() || null,
      };
      return next;
    })
    .catch(() => {
      // If the API is unreachable, default to closed (safe fallback).
      const next: WindowStatus = {
        loading: false,
        isOpen: false,
        semesterName: null,
      };
      return next;
    })
    .finally(() => {
      inflight = null;
    });

  inflight = request;
  return request;
}

/** Home/hero copy for the active cycle’s student application window. */
export function formatApplicationAnnouncement(
  semesterName: string | null,
): string {
  if (!semesterName) {
    return "URVP is not currently active.";
  }
  return `Applications for the URVP ${semesterName} are open.`;
}

/**
 * Returns whether the student application window is currently open.
 * Uses the active semester returned by the API and the `isApplicationWindowOpen`
 * computed flag.
 */
export function useApplicationWindow(): WindowStatus {
  const query = useCancellableQuery(() => fetchWindowStatus(), [], {
    initialData: { loading: true, isOpen: false, semesterName: null },
    initialLoading: true,
  });

  return query.data ?? { loading: query.loading, isOpen: false, semesterName: null };
}

/**
 * Students may not browse or open project listings while the application window
 * is closed. Faculty and admin keep access. `pending` is true until role and
 * window status are known, so the catalog is not flashed before the lock.
 */
export function useStudentProjectsLocked(): {
  pending: boolean;
  locked: boolean;
  semesterName: string | null;
} {
  const { status, loading: authLoading } = useAuth();
  const appWindow = useApplicationWindow();
  const studentUser = isStudent(status?.role);

  return {
    pending: authLoading || (studentUser && appWindow.loading),
    locked:
      !authLoading && studentUser && !appWindow.loading && !appWindow.isOpen,
    semesterName: appWindow.semesterName,
  };
}
