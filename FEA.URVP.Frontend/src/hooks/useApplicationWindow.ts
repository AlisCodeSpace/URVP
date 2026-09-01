"use client";

import { useEffect, useState } from "react";
import { getActiveSemester } from "@/lib/semesters-api";

type WindowStatus =
  | { loading: true; isOpen: false; semesterName: null }
  | { loading: false; isOpen: boolean; semesterName: string | null };

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
    getActiveSemester()
      .then((sem) => {
        if (cancelled) return;
        setStatus({
          loading: false,
          isOpen: sem?.isApplicationWindowOpen ?? false,
          semesterName: sem?.name ?? null,
        });
      })
      .catch(() => {
        if (cancelled) return;
        // If the API is unreachable, default to closed (safe fallback).
        setStatus({ loading: false, isOpen: false, semesterName: null });
      });
    return () => {
      cancelled = true;
    };
  }, []);

  return status;
}
