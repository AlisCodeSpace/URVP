"use client";

import { useState } from "react";
import { NotificationItem } from "@/components/notifications/NotificationItem";
import { NotificationSettingsDialog } from "@/components/notifications/NotificationSettingsDialog";
import { Button } from "@/components/ui/Button";
import { useNotificationActions } from "@/hooks/useNotificationActions";
import { useNotifications } from "@/hooks/useNotifications";
import { useNotificationSettings } from "@/hooks/useNotificationSettings";
import { useRealTimeNotifications } from "@/hooks/useRealTimeNotifications";

export function NotificationsView() {
  const [settingsOpen, setSettingsOpen] = useState(false);
  const settings = useNotificationSettings();
  const list = useNotifications({
    page: 1,
    pageSize: 50,
    inAppNotifications: settings.settings?.inAppNotifications,
  });
  const actions = useNotificationActions(list);
  useRealTimeNotifications({ enabled: list.canAccess });

  return (
    <div className="notification-page">
      <div className="notification-page-toolbar">
        <p className="notification-page-count">
          {list.inAppEnabled
            ? `${list.unreadCount} unread`
            : "In-app notifications are hidden"}
        </p>
        <div className="flex flex-wrap gap-2">
          <Button type="button" variant="outline" size="sm" onClick={() => void list.refresh()}>
            Refresh
          </Button>
          <Button type="button" variant="outline" size="sm" onClick={() => void actions.markAllRead()}>
            Mark all read
          </Button>
          <Button type="button" variant="outline" size="sm" onClick={() => setSettingsOpen(true)}>
            Settings
          </Button>
        </div>
      </div>

      {list.error ? <p className="notification-page-error">{list.error}</p> : null}

      {list.loading ? (
        <p className="text-sm text-muted">Loading notifications…</p>
      ) : !list.inAppEnabled ? (
        <p className="text-sm text-muted">
          Turn on in-app notifications in Settings to see this list. Existing rows are kept.
        </p>
      ) : list.items.length === 0 ? (
        <p className="text-sm text-muted">No notifications yet.</p>
      ) : (
        <div className="notification-page-list">
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

      <NotificationSettingsDialog
        open={settingsOpen}
        settings={settings.settings}
        onClose={() => setSettingsOpen(false)}
        onSave={async (next) => {
          await settings.save(next);
          await list.refresh();
        }}
      />
    </div>
  );
}
