function envOrigin(value: string | undefined, fallback: string): string {
  const trimmed = value?.trim().replace(/\/$/, "") ?? "";
  return /^https?:\/\//i.test(trimmed) ? trimmed : fallback;
}

function isPublicHost(host: string): boolean {
  const hostname = host.split(":")[0]?.replace(/^\[|\]$/g, "").toLowerCase();
  return Boolean(
    hostname &&
      hostname !== "0.0.0.0" &&
      hostname !== "::" &&
      hostname !== "localhost" &&
      hostname !== "127.0.0.1" &&
      hostname !== "::1",
  );
}

/** Public HTTPS origin of the web app (never the container bind address). */
export function publicAppOrigin(headers?: Headers): string {
  const fromEnv = envOrigin(
    process.env.NEXT_PUBLIC_APP_URL ?? process.env.RENDER_EXTERNAL_URL,
    "",
  );
  if (fromEnv && isPublicHost(new URL(fromEnv).host)) {
    return fromEnv;
  }

  if (typeof window !== "undefined" && window.location?.origin) {
    return window.location.origin.replace(/\/$/, "");
  }

  if (headers) {
    const proto = headers.get("x-forwarded-proto")?.split(",")[0]?.trim() || "https";
    const host =
      headers.get("x-forwarded-host")?.split(",")[0]?.trim() ||
      headers.get("host")?.trim() ||
      "";
    if (isPublicHost(host)) {
      return `${proto}://${host}`.replace(/\/$/, "");
    }
  }

  return envOrigin(process.env.NEXT_PUBLIC_APP_URL, "https://localhost:3000");
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

  return publicAppOrigin();
}

/** Backend API origin for the current runtime (no trailing slash). */
export const apiBaseUrl = getApiBaseUrl();
