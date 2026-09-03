"use client";

import { useEffect } from "react";
import { useRouter, useSearchParams } from "next/navigation";
import { fetchAuthStatus, portalHref } from "@/lib/auth";
import { logger } from "@/lib/logger";
import { PageLoader } from "@/components/ui/PageLoader";

export function AuthCallbackView() {
  const router = useRouter();
  const searchParams = useSearchParams();

  useEffect(() => {
    // The backend sends an opaque code here, never identity-provider text. The user-facing message
    // is resolved from it on the sign-in page by authErrorMessage.
    const error = searchParams.get("error");
    if (error) {
      logger.warn("Sign-in callback reported an error code.", { code: error });
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
      } catch {
        logger.warn("Session status could not be read after sign-in.");
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
