"use client";

import { useAuth } from "@/components/auth/AuthProvider";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
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
  const userId = status?.userId ?? null;
  const canLoad = Boolean(
    !authLoading && status?.isAuthenticated && userId && isStudent(status.role),
  );
  const query = useCancellableQuery(
    () => fetchMine(userId ?? ""),
    [userId, canLoad],
    { enabled: canLoad },
  );

  if (authLoading || (canLoad && query.loading)) {
    return { loading: true, exists: false, profile: null };
  }

  if (!canLoad) {
    return { loading: false, exists: false, profile: null };
  }

  return {
    loading: false,
    exists: Boolean(query.data?.exists),
    profile: query.data,
  };
}
