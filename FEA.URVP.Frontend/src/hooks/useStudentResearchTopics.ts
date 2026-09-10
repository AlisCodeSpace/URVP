"use client";

import { useMemo } from "react";
import { useMyStudentProfile } from "@/hooks/useMyStudentProfile";

const EMPTY = new Set<string>();

/** Research topics from the signed-in student's profile (empty when not a student). */
export function useStudentResearchTopics(): ReadonlySet<string> {
  const { profile } = useMyStudentProfile();
  return useMemo(() => {
    const topics = profile?.researchTopics ?? [];
    return topics.length ? new Set(topics) : EMPTY;
  }, [profile]);
}

export function isResearchTopicMatch(
  topic: string,
  studentTopics: ReadonlySet<string>,
): boolean {
  return studentTopics.has(topic);
}
