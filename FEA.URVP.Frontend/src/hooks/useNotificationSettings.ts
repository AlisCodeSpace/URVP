"use client";

import { useCallback } from "react";
import { useAuth } from "@/components/auth/AuthProvider";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import {
  getNotificationSettings,
  updateNotificationSettings,
  type NotificationSettings,
} from "@/lib/notifications-api";

type UseNotificationSettingsOptions = {
  enabled?: boolean;
};

export function useNotificationSettings(
  options: UseNotificationSettingsOptions = {},
) {
  const { status } = useAuth();
  const enabled =
    Boolean(status?.isAuthenticated) && (options.enabled ?? true);
  const query = useCancellableQuery(() => getNotificationSettings(), [enabled], {
    enabled,
    clearDataWhenDisabled: true,
    fallbackError: "Failed to load settings.",
  });
  const { setData } = query;

  const save = useCallback(async (next: NotificationSettings) => {
    const updated = await updateNotificationSettings(next);
    setData(updated);
    return updated;
  }, [setData]);

  return {
    settings: query.data,
    loading: query.loading,
    error: query.error,
    refresh: query.reload,
    save,
    enabled,
  };
}
