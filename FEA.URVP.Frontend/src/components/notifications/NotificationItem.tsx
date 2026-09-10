"use client";

import { formatAppDateTime } from "@/lib/datetime";
import { notificationTone, type Notification } from "@/lib/notifications-api";

function formatWhen(value: string) {
  return formatAppDateTime(value);
}

type NotificationItemProps = {
  notification: Notification;
  onOpen: (notification: Notification) => void;
};

export function NotificationItem({ notification, onOpen }: NotificationItemProps) {
  const tone = notificationTone(notification.type);

  return (
    <button
      type="button"
      className={`notification-item is-${tone}${notification.isRead ? " is-read" : ""}`}
      onClick={() => onOpen(notification)}
    >
      <span className="notification-item-dot" aria-hidden />
      <span className="notification-item-body">
        <span className="notification-item-title">{notification.title}</span>
        <span className="notification-item-message">{notification.message}</span>
        <span className="notification-item-meta">{formatWhen(notification.createdAt)}</span>
      </span>
    </button>
  );
}
