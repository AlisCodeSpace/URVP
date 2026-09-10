import type { Metadata } from "next";
import { AdminNotificationsView } from "@/components/admin/AdminNotificationsView";

export const metadata: Metadata = {
  title: "Notifications | Admin",
  description: "In-app notifications for your URVP administration account.",
};

export default function AdminNotificationsPage() {
  return <AdminNotificationsView />;
}
