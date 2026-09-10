"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/components/auth/AuthProvider";
import { isStudent } from "@/lib/auth";
import {
  getMyStudentProfile,
  type StudentProfileDto,
} from "@/lib/student-profile-api";

export type MyStudentProfileStatus =
  | { loading: true; exists: false; profile: null }
  | { loading: false; exists: boolean; profile: StudentProfileDto | null };

const inflight = new Map<string, Promise<StudentProfileDto | null>>();

function fetchMine(userId: string): Promise<StudentProfileDto | null> {
  const existing = inflight.get(userId);
  if (existing) return existing;

  const request = getMyStudentProfile()
    .then((profile) => profile)
    .catch(() => null)
    .finally(() => {
      inflight.delete(userId);
    });

  inflight.set(userId, request);
  return request;
}

/** Signed-in student's saved profile, or an empty shell when none exists yet. */
export function useMyStudentProfile(): MyStudentProfileStatus {
  const { status, loading: authLoading } = useAuth();
  const [state, setState] = useState<MyStudentProfileStatus>({
    loading: true,
    exists: false,
    profile: null,
  });

  useEffect(() => {
    if (authLoading) return;

    if (!status?.isAuthenticated || !status.userId || !isStudent(status.role)) {
      setState({ loading: false, exists: false, profile: null });
      return;
    }

    const userId = status.userId;
    let cancelled = false;
    setState({ loading: true, exists: false, profile: null });

    void fetchMine(userId).then((profile) => {
      if (cancelled) return;
      setState({
        loading: false,
        exists: Boolean(profile?.exists),
        profile,
      });
    });

    return () => {
      cancelled = true;
    };
  }, [authLoading, status?.isAuthenticated, status?.userId, status?.role]);

  return state;
}
