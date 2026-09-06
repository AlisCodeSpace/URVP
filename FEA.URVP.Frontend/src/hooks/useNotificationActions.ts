"use client";

import { useCallback } from "react";
import {
  deleteAllNotifications,
  markAllNotificationsRead,
  markNotificationRead,
  type Notification,
} from "@/lib/notifications-api";

type NotificationStateSetters = {
  items: Notification[];
  setItems: (next: Notification[] | ((prev: Notification[]) => Notification[])) => void;
  setUnreadCount: (next: number | ((prev: number) => number)) => void;
  setTotalCount?: (next: number | ((prev: number) => number)) => void;
  refresh: () => Promise<void>;
};

export function useNotificationActions(state: NotificationStateSetters) {
  const markRead = useCallback(
    async (id: string) => {
      const target = state.items.find((item) => item.id === id);
      if (!target || target.isRead) return;

      const now = new Date().toISOString();
      state.setItems((prev) =>
        prev.map((item) =>
          item.id === id ? { ...item, isRead: true, readAt: now } : item,
        ),
      );
      state.setUnreadCount((count) => Math.max(0, count - 1));

      try {
        await markNotificationRead(id);
      } catch {
        await state.refresh();
      }
    },
    [state],
  );

  const markAllRead = useCallback(async () => {
    const unread = state.items.filter((item) => !item.isRead).length;
    const now = new Date().toISOString();
    state.setItems((prev) =>
      prev.map((item) => (item.isRead ? item : { ...item, isRead: true, readAt: now })),
    );
    state.setUnreadCount(0);

    try {
      await markAllNotificationsRead();
    } catch {
      await state.refresh();
      return unread;
    }

    return unread;
  }, [state]);

  const deleteAll = useCallback(async () => {
    const previousCount = state.items.length;
    state.setItems([]);
    state.setUnreadCount(0);
    state.setTotalCount?.(0);

    try {
      await deleteAllNotifications();
    } catch {
      await state.refresh();
    }

    return previousCount;
  }, [state]);

  return { markRead, markAllRead, deleteAll };
}
