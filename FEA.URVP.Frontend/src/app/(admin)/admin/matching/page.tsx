import type { Metadata } from "next";
import { AdminMatchingView } from "@/components/admin/AdminMatchingView";

export const metadata: Metadata = {
  title: "Matching | Admin",
  description: "Run and review automatic student–project matching.",
};

export default function AdminMatchingPage() {
  return <AdminMatchingView />;
}
