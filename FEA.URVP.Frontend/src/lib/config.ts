/** Backend API origin (no trailing slash). */
export const apiBaseUrl = (
  process.env.NEXT_PUBLIC_API_URL ?? "https://localhost:7222"
).replace(/\/$/, "");

/** Frontend origin used for SSO return URLs (must be HTTPS in local dev). */
export const appBaseUrl = (
  process.env.NEXT_PUBLIC_APP_URL ?? "https://localhost:3000"
).replace(/\/$/, "");
