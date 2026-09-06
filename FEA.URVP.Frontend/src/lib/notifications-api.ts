import { apiFetch } from "@/lib/api";

/** Exact backend NotificationType.ToString() names. */
export const NotificationType = {
  ProjectApproved: "ProjectApproved",
  ProjectOpen: "ProjectOpen",
  ProjectClosed: "ProjectClosed",
  ProjectDeleted: "ProjectDeleted",
  PlacementConfirmed: "PlacementConfirmed",
  PlacementDeclined: "PlacementDeclined",
  PlacementCancelled: "PlacementCancelled",
  MatchingConfirmed: "MatchingConfirmed",
  RankingSubmitted: "RankingSubmitted",
  RankingRemoved: "RankingRemoved",
  FacultyRankingSubmitted: "FacultyRankingSubmitted",
  ApplicationWindowOpened: "ApplicationWindowOpened",
  ApplicationWindowClosed: "ApplicationWindowClosed",
  SemesterCycleStarted: "SemesterCycleStarted",
  StudentProfileSubmitted: "StudentProfileSubmitted",
  NewsPublished: "NewsPublished",
  WorkshopAnnounced: "WorkshopAnnounced",
  RoleAssigned: "RoleAssigned",
} as const;

export type NotificationTypeName =
  (typeof NotificationType)[keyof typeof NotificationType];

export type Notification = {
  id: string;
  type: NotificationTypeName | string;
  title: string;
  message: string;
  data?: string | null;
  isRead: boolean;
  createdAt: string;
  readAt?: string | null;
};

export type NotificationSettings = {
  emailNotifications: boolean;
  inAppNotifications: boolean;
};

export type PaginatedNotifications = {
  items: Notification[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
};

export type NotificationCount = {
  count: number;
};

export function notificationTone(
  type: string,
): "approved" | "rejected" | "default" {
  if (
    type === NotificationType.ProjectApproved ||
    type === NotificationType.PlacementConfirmed ||
    type === NotificationType.MatchingConfirmed
  ) {
    return "approved";
  }

  if (
    type === NotificationType.PlacementDeclined ||
    type === NotificationType.PlacementCancelled ||
    type === NotificationType.ProjectClosed ||
    type === NotificationType.ProjectDeleted
  ) {
    return "rejected";
  }

  return "default";
}

export async function listNotifications(params: {
  page?: number;
  pageSize?: number;
  unreadOnly?: boolean;
} = {}): Promise<PaginatedNotifications> {
  const query = new URLSearchParams();
  query.set("page", String(params.page ?? 1));
  query.set("pageSize", String(params.pageSize ?? 20));
  if (params.unreadOnly) query.set("unreadOnly", "true");
  return apiFetch<PaginatedNotifications>(`/api/notification?${query}`);
}

export async function getUnreadCount(): Promise<NotificationCount> {
  return apiFetch<NotificationCount>("/api/notification/unread-count");
}

export async function markNotificationRead(id: string): Promise<boolean> {
  return apiFetch<boolean>(`/api/notification/${id}/mark-read`, {
    method: "POST",
  });
}

export async function markAllNotificationsRead(): Promise<NotificationCount> {
  return apiFetch<NotificationCount>("/api/notification/mark-all-read", {
    method: "POST",
  });
}

export async function deleteAllNotifications(): Promise<NotificationCount> {
  return apiFetch<NotificationCount>("/api/notification", {
    method: "DELETE",
  });
}

export async function getNotificationSettings(): Promise<NotificationSettings> {
  return apiFetch<NotificationSettings>("/api/notificationsettings");
}

export async function updateNotificationSettings(
  settings: NotificationSettings,
): Promise<NotificationSettings> {
  return apiFetch<NotificationSettings>("/api/notificationsettings", {
    method: "PUT",
    body: JSON.stringify(settings),
  });
}
