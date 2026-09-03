import type { NextConfig } from "next";

/**
 * The ASP.NET Core backend serves this build from its own origin, so the browser sees a single
 * origin for both the app and the API (same-origin BFF). No Next.js server runs in production.
 *
 * Consequences that matter for security:
 * - `headers()` and `middleware.ts` are inert for a static export. Security headers, CSP and all
 *   authorization live in ASP.NET Core / IIS, which is the only authoritative place.
 * - Every file in `out/` is a public asset. Route guards are UX only; protection comes from the
 *   backend refusing unauthorized API calls.
 * - Route parameters cannot be path segments, because static export would need to enumerate them
 *   at build time. Detail routes read their identifiers from the query string instead; see
 *   `src/lib/auth.ts` for the URL builders.
 */
const nextConfig: NextConfig = {
  output: "export",

  // The image optimizer needs a server; without this the export fails on next/image usage.
  images: { unoptimized: true },

  // Source maps would publish readable application source as public static assets.
  productionBrowserSourceMaps: false,

  // Removes the framework fingerprinting header from the dev server too.
  poweredByHeader: false,
};

export default nextConfig;
