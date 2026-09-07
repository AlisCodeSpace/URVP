"use client";

import { useCallback, useEffect, useMemo, useRef, useState } from "react";
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
  const explicitInApp = filters.inAppNotifications;
  const ownSettings = useNotificationSettings({
    enabled: explicitInApp === undefined,
  });
  const inAppEnabled =
    explicitInApp !== undefined
      ? explicitInApp
      : ownSettings.settings?.inAppNotifications !== false;
  const settingsLoading = explicitInApp === undefined && ownSettings.loading;

  const [items, setItems] = useState<Notification[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const page = filters.page ?? 1;
  const pageSize = filters.pageSize ?? 20;
  const unreadOnly = filters.unreadOnly ?? false;
  const requestIdRef = useRef(0);

  const refresh = useCallback(async () => {
    if (!canAccess) {
      setItems([]);
      setUnreadCount(0);
      setTotalCount(0);
      return;
    }

    if (!inAppEnabled) {
      setItems([]);
      setUnreadCount(0);
      setTotalCount(0);
      return;
    }

    const requestId = ++requestIdRef.current;
    setLoading(true);
    setError(null);
    try {
      const [pageResult, unread] = await Promise.all([
        listNotifications({ page, pageSize, unreadOnly }),
        getUnreadCount(),
      ]);
      if (requestId !== requestIdRef.current) return;
      setItems(pageResult.items);
      setTotalCount(pageResult.totalCount);
      setUnreadCount(unread.count);
    } catch (err) {
      if (requestId !== requestIdRef.current) return;
      setError(
        err instanceof Error ? err.message : "Failed to load notifications.",
      );
    } finally {
      if (requestId === requestIdRef.current) setLoading(false);
    }
  }, [canAccess, inAppEnabled, page, pageSize, unreadOnly]);

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
