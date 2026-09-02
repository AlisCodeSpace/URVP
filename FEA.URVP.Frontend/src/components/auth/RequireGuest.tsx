"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/auth/AuthProvider";
import { PageLoader } from "@/components/ui/PageLoader";
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
    return <PageLoader label="Redirecting" />;
  }

  return children;
}
