"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/components/auth/AuthProvider";
import { isStudent } from "@/lib/auth";
import { getMyStudentProfile } from "@/lib/student-profile-api";

const EMPTY = new Set<string>();

/** Research topics from the signed-in student's profile (empty when not a student). */
export function useStudentResearchTopics(): ReadonlySet<string> {
  const { status, loading: authLoading } = useAuth();
  const [topics, setTopics] = useState<ReadonlySet<string>>(EMPTY);

  useEffect(() => {
    if (authLoading) return;

    if (!status?.isAuthenticated || !isStudent(status.role)) {
      setTopics(EMPTY);
      return;
    }

    let cancelled = false;

    void (async () => {
      try {
        const profile = await getMyStudentProfile();
        if (cancelled) return;
        setTopics(new Set(profile.researchTopics ?? []));
      } catch {
        if (!cancelled) setTopics(EMPTY);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [authLoading, status?.isAuthenticated, status?.role]);

  return topics;
}

export function isResearchTopicMatch(
  topic: string,
  studentTopics: ReadonlySet<string>,
): boolean {
  return studentTopics.has(topic);
}
