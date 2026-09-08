import type { Metadata } from "next";
import { AdminEmailSettingsView } from "@/components/admin/AdminEmailSettingsView";

export const metadata: Metadata = {
  title: "Email | Admin",
  description: "Configure the SMTP password used to send URVP email.",
};

export default function AdminEmailSettingsPage() {
  return <AdminEmailSettingsView />;
}
