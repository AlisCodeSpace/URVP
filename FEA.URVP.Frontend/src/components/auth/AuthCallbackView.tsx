"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { fetchAuthStatus, portalHref } from "@/lib/auth";
import { logger } from "@/lib/logger";
import { PageLoader } from "@/components/ui/PageLoader";
import { useCancellableEffect } from "@/hooks/useCancellableQuery";

export function AuthCallbackView() {
  const router = useRouter();
  const searchParams = useSearchParams();

  useCancellableEffect((isCurrent) => {
    const error = searchParams.get("error");
    if (error) {
      logger.warn("Sign-in callback reported an error code.", { code: error });
    }

    return (async () => {
      try {
        const status = await fetchAuthStatus();
        if (!isCurrent()) return;

        if (status.isAuthenticated) {
          router.replace(portalHref(status.role, status.userId));
          return;
        }
      } catch {
        logger.warn("Session status could not be read after sign-in.");
        if (!isCurrent()) return;
      }

      if (isCurrent()) router.replace("/sign-in?error=session_missing");
    })();
  }, [router, searchParams]);

  return <PageLoader label="Completing sign-in" />;
}
