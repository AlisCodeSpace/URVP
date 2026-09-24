"use client";

import { useCallback, useMemo } from "react";
import { useAuth } from "@/components/auth/AuthProvider";
import { useNotificationSettings } from "@/hooks/useNotificationSettings";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
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

type NotificationPage = {
  items: Notification[];
  unreadCount: number;
  totalCount: number;
};

const emptyPage: NotificationPage = {
  items: [],
  unreadCount: 0,
  totalCount: 0,
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
  const page = filters.page ?? 1;
  const pageSize = filters.pageSize ?? 20;
  const unreadOnly = filters.unreadOnly ?? false;
  const enabled = canAccess && !settingsLoading && inAppEnabled;

  const query = useCancellableQuery(
    async () => {
      const [pageResult, unread] = await Promise.all([
        listNotifications({ page, pageSize, unreadOnly }),
        getUnreadCount(),
      ]);
      return {
        items: pageResult.items,
        totalCount: pageResult.totalCount,
        unreadCount: unread.count,
      };
    },
    [page, pageSize, unreadOnly, enabled],
    {
      enabled,
      initialData: emptyPage,
      initialLoading: false,
      fallbackError: "Failed to load notifications.",
    },
  );
  const { setData } = query;
  const pageData = query.data ?? emptyPage;

  const setItems = useCallback(
    (next: Notification[] | ((prev: Notification[]) => Notification[])) => {
      setData((current) => {
        const base = current ?? emptyPage;
        const items = typeof next === "function" ? next(base.items) : next;
        return { ...base, items };
      });
    },
    [setData],
  );

  const setUnreadCount = useCallback(
    (next: number | ((prev: number) => number)) => {
      setData((current) => {
        const base = current ?? emptyPage;
        const unreadCount = typeof next === "function" ? next(base.unreadCount) : next;
        return { ...base, unreadCount };
      });
    },
    [setData],
  );

  const setTotalCount = useCallback(
    (next: number | ((prev: number) => number)) => {
      setData((current) => {
        const base = current ?? emptyPage;
        const totalCount = typeof next === "function" ? next(base.totalCount) : next;
        return { ...base, totalCount };
      });
    },
    [setData],
  );

  const visible = canAccess && inAppEnabled;

  return useMemo(
    () => ({
      items: visible ? pageData.items : [],
      unreadCount: visible ? pageData.unreadCount : 0,
      totalCount: visible ? pageData.totalCount : 0,
      loading: settingsLoading || (visible && query.loading),
      error: query.error,
      refresh: query.reload,
      canAccess,
      inAppEnabled,
      setItems,
      setUnreadCount,
      setTotalCount,
    }),
    [
      canAccess,
      inAppEnabled,
      pageData.items,
      pageData.totalCount,
      pageData.unreadCount,
      query.error,
      query.loading,
      query.reload,
      setItems,
      setTotalCount,
      setUnreadCount,
      settingsLoading,
      visible,
    ],
  );
}
