"use client";

import { useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { fetchAuthStatus, portalHref } from "@/lib/auth";
import { PageLoader } from "@/components/ui/PageLoader";

export function AuthCallbackView() {
  const router = useRouter();
  const searchParams = useSearchParams();

  useEffect(() => {
    const error = searchParams.get("error");
    if (error) {
      console.warn("[auth/callback] OIDC error query:", error);
    }

    let cancelled = false;

    void (async () => {
      try {
        const status = await fetchAuthStatus();
        if (cancelled) return;

        if (status.isAuthenticated) {
          router.replace(portalHref(status.role, status.userId));
          return;
        }
      } catch (err) {
        console.error("[auth/callback] /api/auth/status failed:", err);
        if (cancelled) return;
      }

      router.replace("/sign-in?error=session_missing");
    })();

    return () => {
      cancelled = true;
    };
  }, [router, searchParams]);

  return <PageLoader label="Completing sign-in" />;
}
