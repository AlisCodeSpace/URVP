import type { Metadata } from "next";
import { AdminMatchingView } from "@/components/admin/AdminMatchingView";

export const metadata: Metadata = {
  title: "Matching test | Admin",
  description: "Test the automatic student–project matcher. Assign students on each project.",
};

export default function AdminMatchingPage() {
  return <AdminMatchingView />;
}
