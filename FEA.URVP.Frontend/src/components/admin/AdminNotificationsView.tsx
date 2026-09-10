"use client";

import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { NotificationsView } from "@/components/notifications/NotificationsView";

export function AdminNotificationsView() {
  return (
    <div className="admin-panel">
      <AdminPageHeader
        title="Notifications"
        description="Updates about rankings, matching, program windows, and announcements."
      />
      <NotificationsView />
    </div>
  );
}
