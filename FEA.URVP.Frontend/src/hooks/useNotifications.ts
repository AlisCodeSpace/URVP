"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/auth/AuthProvider";
import { useNotificationSettings } from "@/hooks/useNotificationSettings";
import {
  getUnreadCount,
  listNotifications,
  type Notification,
} from "@/lib/notifications-api";

type UseNotificationsOptions = {
  page?: number;
  pageSize?: number;
  unreadOnly?: boolean;
  inAppNotifications?: boolean;
};

export function useNotifications(filters: UseNotificationsOptions = {}) {
  const { status } = useAuth();
  const canAccess = Boolean(status?.isAuthenticated);
  const ownSettings = useNotificationSettings();
  const settings = filters.inAppNotifications === undefined ? ownSettings.settings : {
    emailNotifications: true,
    inAppNotifications: filters.inAppNotifications,
  };
  const settingsLoading =
    filters.inAppNotifications === undefined && ownSettings.loading;
  const inAppEnabled = settings?.inAppNotifications !== false;

  const [items, setItems] = useState<Notification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const page = filters.page ?? 1;
  const pageSize = filters.pageSize ?? 20;
  const unreadOnly = filters.unreadOnly ?? false;

  const refresh = useCallback(async () => {
    if (!canAccess) {
      setItems([]);
      setUnreadCount(0);
      setTotalCount(0);
      return;
    }

    if (settings && !inAppEnabled) {
      setItems([]);
      setUnreadCount(0);
      setTotalCount(0);
      return;
    }

    setLoading(true);
    setError(null);
    try {
      const [pageResult, unread] = await Promise.all([
        listNotifications({ page, pageSize, unreadOnly }),
        getUnreadCount(),
      ]);
      setItems(pageResult.items);
      setTotalCount(pageResult.totalCount);
      setUnreadCount(unread.count);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load notifications.");
    } finally {
      setLoading(false);
    }
  }, [canAccess, inAppEnabled, page, pageSize, settings, unreadOnly]);

  useEffect(() => {
    if (!canAccess || settingsLoading) return;
    void refresh();
  }, [canAccess, refresh, settingsLoading]);

  return useMemo(
    () => ({
      items: !inAppEnabled ? [] : items,
      unreadCount: !inAppEnabled ? 0 : unreadCount,
      totalCount: !inAppEnabled ? 0 : totalCount,
      loading: settingsLoading || loading,
      error,
      refresh,
      canAccess,
      inAppEnabled,
      setItems,
      setUnreadCount,
      setTotalCount,
    }),
    [
      canAccess,
      error,
      inAppEnabled,
      items,
      loading,
      refresh,
      settingsLoading,
      totalCount,
      unreadCount,
    ],
  );
}
