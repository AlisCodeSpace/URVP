import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { AdminShell } from "@/components/admin/AdminShell";
import { ADMIN_ROLES } from "@/lib/auth";

export const metadata: Metadata = {
  title: "Admin | URVP",
  description: "URVP administration console.",
};

export default function AdminLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <RequireAuth roles={ADMIN_ROLES}>
      <AdminShell>{children}</AdminShell>
    </RequireAuth>
  );
}
