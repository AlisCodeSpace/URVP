import type { Metadata } from "next";
import { AdminHomeIntroView } from "@/components/admin/AdminHomeIntroView";

export const metadata: Metadata = {
  title: "Home intro | Admin",
  description: "Edit the homepage intro headline, description, and key points.",
};

export default function AdminHomeIntroPage() {
  return <AdminHomeIntroView />;
}
