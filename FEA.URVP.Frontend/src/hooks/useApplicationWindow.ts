"use client";

import { useEffect, useState } from "react";
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
  isOpen: boolean,
): string {
  if (!semesterName) {
    return "URVP is not currently active.";
  }
  return `Applications for the URVP ${semesterName} are ${isOpen ? "open" : "closed"}.`;
}

/**
 * Returns whether the student application window is currently open.
 * Uses the active semester returned by the API and the `isApplicationWindowOpen`
 * computed flag.
 */
export function useApplicationWindow(): WindowStatus {
  const [status, setStatus] = useState<WindowStatus>({
    loading: true,
    isOpen: false,
    semesterName: null,
  });

  useEffect(() => {
    let cancelled = false;
    fetchWindowStatus().then((next) => {
      if (!cancelled) setStatus(next);
    });
    return () => {
      cancelled = true;
    };
  }, []);

  return status;
}
