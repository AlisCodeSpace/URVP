function envOrigin(value: string | undefined, fallback: string): string {
  const trimmed = value?.trim().replace(/\/$/, "") ?? "";
  return /^https?:\/\//i.test(trimmed) ? trimmed : fallback;
}

/** Frontend origin used for SSO return URLs (must be HTTPS in local dev). */
export const appBaseUrl = envOrigin(
  process.env.NEXT_PUBLIC_APP_URL,
  "https://localhost:3000",
);

const directApiBaseUrl = envOrigin(
  process.env.NEXT_PUBLIC_API_URL,
  "https://localhost:7222",
);

/**
 * Production hosts the SPA and API on different Render URLs. Browser calls
 * same-origin `/api/*`, which Next.js proxies to the backend so the auth
 * cookie stays first-party. Local `next dev` still talks to the API directly.
 */
export const useSameOriginApi = process.env.NODE_ENV === "production";

export function getApiBaseUrl(): string {
  if (!useSameOriginApi) {
    return directApiBaseUrl;
  }

  if (typeof window !== "undefined") {
    return window.location.origin;
  }

  return envOrigin(
    process.env.NEXT_PUBLIC_APP_URL ?? process.env.RENDER_EXTERNAL_URL,
    appBaseUrl,
  );
}

/** Backend API origin for the current runtime (no trailing slash). */
export const apiBaseUrl = getApiBaseUrl();
