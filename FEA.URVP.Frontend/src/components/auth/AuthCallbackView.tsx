"use client";

import { useEffect, useState } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { Text } from "@radix-ui/themes";
import { fetchAuthStatus, portalHref } from "@/lib/auth";

export function AuthCallbackView() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [message] = useState("Completing sign-in…");

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

  return (
    <main className="flex min-h-dvh flex-1 items-center justify-center px-6">
      <Text as="p" size="3" className="!text-muted">
        {message}
      </Text>
    </main>
  );
}
