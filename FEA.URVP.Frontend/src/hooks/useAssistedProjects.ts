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

/**
 * Whether the signed-in user assists `projectId`. Only a friendlier message than the backend's
 * 403; the backend enforces access on every request. When `enabled` is false, access is granted.
 */
export function useAssistantAccess(projectId: string, enabled: boolean) {
  const assisted = useAssistedProjects(enabled);
  const isListed =
    assisted.data?.some(
      (item) => item.projectId.toLowerCase() === projectId.toLowerCase(),
    ) ?? false;

  if (!enabled) return { allowed: true, checking: false, error: null };

  return {
    allowed: isListed,
    checking: assisted.data == null && assisted.error == null,
    error:
      assisted.error ??
      (assisted.data != null && !isListed
        ? "You can only review projects where you are a research assistant."
        : null),
  };
}
