import type { Metadata } from "next";
import { AdminNewsView } from "@/components/admin/AdminNewsView";

export const metadata: Metadata = {
  title: "News | Admin",
  description: "Publish and edit URVP news stories.",
};

export default function AdminNewsPage() {
  return <AdminNewsView />;
}
