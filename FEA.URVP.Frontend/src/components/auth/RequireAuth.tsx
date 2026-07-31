"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/auth/AuthProvider";
import { portalHref } from "@/lib/auth";

type RequireAuthProps = {
  children: React.ReactNode;
  /** When set, only this user may view the page; others are redirected to their portal. */
  userId?: string;
};

/** Requires a signed-in session. Optionally scopes access to a specific user id. */
export function RequireAuth({ children, userId }: RequireAuthProps) {
  const router = useRouter();
  const { status, loading } = useAuth();

  useEffect(() => {
    if (loading) return;

    if (!status?.isAuthenticated || !status.userId) {
      router.replace("/sign-in");
      return;
    }

    if (userId && status.userId !== userId) {
      router.replace(portalHref(status.role, status.userId));
    }
  }, [loading, status, userId, router]);

  const unauthorized =
    !status?.isAuthenticated ||
    !status.userId ||
    (userId != null && status.userId !== userId);

  if (loading || unauthorized) {
    return (
      <main className="flex min-h-[50vh] flex-1 items-center justify-center px-6">
        <p className="text-muted">
          {loading ? "Loading…" : "Redirecting…"}
        </p>
      </main>
    );
  }

  return children;
}
