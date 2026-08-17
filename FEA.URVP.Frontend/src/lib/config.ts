function envOrigin(value: string | undefined, fallback: string): string {
  const trimmed = value?.trim().replace(/\/$/, "") ?? "";
  return /^https?:\/\//i.test(trimmed) ? trimmed : fallback;
}

/** Backend API origin (no trailing slash). */
export const apiBaseUrl = envOrigin(
  process.env.NEXT_PUBLIC_API_URL,
  "https://localhost:7222",
);

/** Frontend origin used for SSO return URLs (must be HTTPS in local dev). */
export const appBaseUrl = envOrigin(
  process.env.NEXT_PUBLIC_APP_URL,
  "https://localhost:3000",
);
