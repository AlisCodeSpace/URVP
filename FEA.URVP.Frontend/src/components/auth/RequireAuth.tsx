"use client";

import { useEffect, useMemo } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/auth/AuthProvider";
import { PageLoader } from "@/components/ui/PageLoader";
import { portalHref } from "@/lib/auth";

type RequireAuthProps = {
  children: React.ReactNode;
  /** When set, only this user may view the page; others are redirected to their portal. */
  userId?: string;
  /** When set, only these roles may view the page; others are redirected to their portal. */
  roles?: readonly number[];
};

/** Requires a signed-in session. Optionally scopes access by user id and/or role. */
export function RequireAuth({ children, userId, roles }: RequireAuthProps) {
  const router = useRouter();
  const { status, loading } = useAuth();
  const rolesKey = useMemo(
    () => (roles?.length ? roles.join(",") : ""),
    [roles],
  );

  useEffect(() => {
    if (loading) return;

    if (!status?.isAuthenticated || !status.userId) {
      router.replace("/sign-in");
      return;
    }

    if (userId && status.userId !== userId) {
      router.replace(portalHref(status.role, status.userId));
      return;
    }

    if (
      rolesKey &&
      (status.role == null || !rolesKey.split(",").map(Number).includes(status.role))
    ) {
      router.replace(portalHref(status.role, status.userId));
    }
  }, [loading, status, userId, rolesKey, router]);

  const allowedRoles = rolesKey
    ? rolesKey.split(",").map(Number)
    : null;

  const roleDenied =
    Boolean(allowedRoles?.length) &&
    (status?.role == null || !allowedRoles!.includes(status.role));

  const unauthorized =
    !status?.isAuthenticated ||
    !status.userId ||
    (userId != null && status.userId !== userId) ||
    roleDenied;

  if (loading || unauthorized) {
    return (
      <PageLoader label={loading ? "Loading" : "Redirecting"} />
    );
  }

  return children;
}
