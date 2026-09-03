/**
 * Runtime origins.
 *
 * Production is a same-origin deployment: ASP.NET Core serves this static export and the API from
 * one origin, so every API path is relative and the session cookie stays first-party and
 * SameSite=Strict. `NEXT_PUBLIC_API_URL` and `NEXT_PUBLIC_APP_URL` exist only for the local
 * `next dev` topology, where the frontend runs on port 3000 and the backend on port 7222.
 *
 * Neither variable may ever hold a secret: both are inlined into the client bundle at build time.
 */

const DEV_API_ORIGIN = "https://localhost:7222";
const DEV_APP_ORIGIN = "https://localhost:3000";

function envOrigin(value: string | undefined, fallback: string): string {
  const trimmed = value?.trim().replace(/\/$/, "") ?? "";
  return /^https?:\/\//i.test(trimmed) ? trimmed : fallback;
}

/**
 * True when the API shares this page's origin. Decided at build time, because a production export
 * is only ever served by the backend itself.
 */
export const useSameOriginApi = process.env.NODE_ENV === "production";

/**
 * Frontend origin, needed only by the split-origin development topology. Production keeps every
 * URL relative and never consults this.
 */
export const appBaseUrl = envOrigin(process.env.NEXT_PUBLIC_APP_URL, DEV_APP_ORIGIN);

/**
 * Prefix for API calls. Empty in production so paths resolve against the page's own origin, which
 * keeps requests same-origin even when the site is reached through an alternate hostname.
 */
export function getApiBaseUrl(): string {
  return useSameOriginApi ? "" : envOrigin(process.env.NEXT_PUBLIC_API_URL, DEV_API_ORIGIN);
}
