"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/auth/AuthProvider";
import { PageLoader } from "@/components/ui/PageLoader";
import { portalHref } from "@/lib/auth";

/** `/my-projects` → role portal (faculty list or student profile). */
export default function MyProjectsIndexPage() {
  const router = useRouter();
  const { status, loading } = useAuth();

  useEffect(() => {
    if (loading) return;

    if (!status?.isAuthenticated || !status.userId) {
      router.replace("/sign-in");
      return;
    }

    router.replace(portalHref(status.role, status.userId));
  }, [loading, status, router]);

  return <PageLoader label="Redirecting" />;
}
