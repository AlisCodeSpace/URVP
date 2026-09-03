"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { PageLoader } from "@/components/ui/PageLoader";

/**
 * Replaces the current history entry with `href` from the browser.
 *
 * `redirect()` from `next/navigation` runs on the server and opts a route out of static export, so
 * legacy path aliases redirect on the client instead. These are convenience aliases only; none of
 * them is a security boundary.
 */
export function ClientRedirect({
  href,
  label = "Redirecting",
}: {
  href: string;
  label?: string;
}) {
  const router = useRouter();

  useEffect(() => {
    router.replace(href);
  }, [href, router]);

  return <PageLoader label={label} />;
}
