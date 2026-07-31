"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/auth/AuthProvider";
import { myProjectsHref } from "@/lib/auth";

/** `/my-projects` → `/my-projects/{userId}` (or sign-in). */
export default function MyProjectsIndexPage() {
  const router = useRouter();
  const { status, loading } = useAuth();

  useEffect(() => {
    if (loading) return;

    if (!status?.isAuthenticated || !status.userId) {
      router.replace("/sign-in");
      return;
    }

    router.replace(myProjectsHref(status.userId));
  }, [loading, status, router]);

  return (
    <main className="flex min-h-[50vh] flex-1 items-center justify-center px-6">
      <p className="text-muted">Redirecting…</p>
    </main>
  );
}
