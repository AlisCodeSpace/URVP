"use client";

import Link from "next/link";
import { useEffect, useId, useRef, useState } from "react";
import { NotificationItem } from "@/components/notifications/NotificationItem";
import { useNotificationActions } from "@/hooks/useNotificationActions";
import { useNotifications } from "@/hooks/useNotifications";
import { useRealTimeNotifications } from "@/hooks/useRealTimeNotifications";
import { notificationsHref } from "@/lib/auth";

const PREVIEW_SIZE = 5;

type NotificationBellProps = {
  variant?: "site" | "admin";
};

export function NotificationBell({ variant = "site" }: NotificationBellProps) {
  const menuId = useId();
  const rootRef = useRef<HTMLDivElement>(null);
  const [open, setOpen] = useState(false);
  const list = useNotifications({ page: 1, pageSize: PREVIEW_SIZE });
  const actions = useNotificationActions(list);
  useRealTimeNotifications({ enabled: list.canAccess });

  useEffect(() => {
    if (!open) return;

    const onPointerDown = (event: MouseEvent) => {
      if (!rootRef.current?.contains(event.target as Node)) setOpen(false);
    };
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") setOpen(false);
    };

    document.addEventListener("mousedown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("mousedown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open]);

  if (!list.canAccess) return null;

  const badge = list.unreadCount > 9 ? "9+" : String(list.unreadCount);

  return (
    <div ref={rootRef} className={`notification-bell is-${variant}`}>
      <button
        type="button"
        className="notification-bell-btn"
        aria-expanded={open}
        aria-haspopup="true"
        aria-controls={menuId}
        aria-label="Notifications"
        onClick={() => setOpen((value) => !value)}
      >
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" aria-hidden>
          <path
            d="M6 8a6 6 0 1 1 12 0c0 7 3 7 3 9H3c0-2 3-2 3-9"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
            strokeLinejoin="round"
          />
          <path
            d="M10 21a2 2 0 0 0 4 0"
            stroke="currentColor"
            strokeWidth="2"
            strokeLinecap="round"
          />
        </svg>
        {list.unreadCount > 0 ? (
          <span className="notification-bell-badge">{badge}</span>
        ) : null}
      </button>

      {open ? (
        <div id={menuId} className="notification-bell-menu" role="menu">
          <div className="notification-bell-head">
            <p>Notifications</p>
            <div className="notification-bell-actions">
              <button type="button" onClick={() => void list.refresh()}>
                Refresh
              </button>
              <button type="button" onClick={() => void actions.markAllRead()}>
                Mark all read
              </button>
            </div>
          </div>
          {list.loading ? (
            <p className="notification-bell-empty">Loading…</p>
          ) : list.items.length === 0 ? (
            <p className="notification-bell-empty">
              {list.inAppEnabled ? "No notifications yet." : "In-app notifications are off."}
            </p>
          ) : (
            <div className="notification-bell-list">
              {list.items.map((notification) => (
                <NotificationItem
                  key={notification.id}
                  notification={notification}
                  onOpen={(item) => {
                    if (!item.isRead) void actions.markRead(item.id);
                  }}
                />
              ))}
            </div>
          )}
          <Link
            href={notificationsHref()}
            className="notification-bell-all"
            onClick={() => setOpen(false)}
          >
            View all
          </Link>
        </div>
      ) : null}
    </div>
  );
}
