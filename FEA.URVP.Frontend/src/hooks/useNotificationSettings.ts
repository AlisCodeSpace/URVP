"use client";

import { useCallback, useEffect, useState } from "react";
import { useAuth } from "@/components/auth/AuthProvider";
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
  const [settings, setSettings] = useState<NotificationSettings | null>(null);
  const [loading, setLoading] = useState(enabled);
  const [error, setError] = useState<string | null>(null);

  const refresh = useCallback(async () => {
    if (!enabled) {
      setSettings(null);
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);
    try {
      setSettings(await getNotificationSettings());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load settings.");
    } finally {
      setLoading(false);
    }
  }, [enabled]);

  useEffect(() => {
    void refresh();
  }, [refresh]);

  const save = useCallback(async (next: NotificationSettings) => {
    const updated = await updateNotificationSettings(next);
    setSettings(updated);
    return updated;
  }, []);

  return { settings, loading, error, refresh, save, enabled };
}
