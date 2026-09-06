import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PageHeader } from "@/components/layout/PageHeader";
import { NotificationsView } from "@/components/notifications/NotificationsView";

export const metadata: Metadata = {
  title: "Notifications | URVP",
  description: "In-app notifications for your Undergraduate Research Volunteer Program account.",
};

export default function NotificationsPage() {
  return (
    <RequireAuth>
      <main className="flex-1 bg-background">
        <PageHeader
          title="Notifications"
          description="Updates about rankings, matching, program windows, and announcements."
        />
        <section className="site-container py-10 sm:py-14">
          <NotificationsView />
        </section>
      </main>
    </RequireAuth>
  );
}
