"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/auth/AuthProvider";
import { portalHref } from "@/lib/auth";

/** Redirects authenticated users away from guest-only pages (e.g. sign-in). */
export function RequireGuest({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { status, loading } = useAuth();

  useEffect(() => {
    if (loading) return;
    if (status?.isAuthenticated) {
      router.replace(portalHref(status.role, status.userId));
    }
  }, [loading, status, router]);

  if (loading || status?.isAuthenticated) {
    return (
      <main className="flex min-h-dvh flex-1 items-center justify-center px-6">
        <p className="text-muted">Redirecting…</p>
      </main>
    );
  }

  return children;
}
