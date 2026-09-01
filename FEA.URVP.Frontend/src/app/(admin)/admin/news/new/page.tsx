import type { Metadata } from "next";
import { AdminNewsForm } from "@/components/admin/AdminNewsForm";

export const metadata: Metadata = {
  title: "New news | Admin",
  description: "Publish a news story.",
};

export default function AdminNewNewsPage() {
  return <AdminNewsForm />;
}
