"use client";

import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { listAssistedProjects } from "@/lib/research-assistants-api";

/** Projects the signed-in user is listed on as a research assistant, matched by email. */
export function useAssistedProjects(enabled: boolean) {
  return useCancellableQuery(() => listAssistedProjects(), [], {
    enabled,
    fallbackError: "Could not load your research assistant projects.",
  });
}
