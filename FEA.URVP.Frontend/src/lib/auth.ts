import { apiBaseUrl, appBaseUrl } from "@/lib/config";

export type AuthStatus = {
  isAuthenticated: boolean;
  userId?: string | null;
  email?: string | null;
  name?: string | null;
  userName?: string | null;
  affiliation?: string | null;
  role?: number | null;
  profileImageUrl?: string | null;
  error?: string | null;
};

export function getAuthCallbackUrl(): string {
  return `${appBaseUrl}/auth/callback`;
}

export function getAzureAdSignInUrl(returnUrl: string = getAuthCallbackUrl()): string {
  const url = new URL("/api/auth/azuread-sso/signin", apiBaseUrl);
  url.searchParams.set("returnUrl", returnUrl);
  return url.toString();
}

export function getAzureAdSignOutUrl(returnUrl: string): string {
  const url = new URL("/api/auth/azuread-sso/signout", apiBaseUrl);
  url.searchParams.set("returnUrl", returnUrl);
  return url.toString();
}

export async function fetchAuthStatus(): Promise<AuthStatus> {
  const res = await fetch(`${apiBaseUrl}/api/auth/status`, {
    method: "GET",
    credentials: "include",
    cache: "no-store",
    mode: "cors",
  });

  if (!res.ok) {
    const failed = { isAuthenticated: false, error: "Failed to load session" };
    console.log("[auth] /api/auth/status failed:", res.status, failed);
    return failed;
  }

  const data = (await res.json()) as AuthStatus;
  console.log("[auth] /api/auth/status response:", data);
  return data;
}

export function authErrorMessage(code: string | null | undefined): string | null {
  if (!code) return null;

  switch (code) {
    case "access_denied":
      return "Access denied. Your account is not allowed to use this portal.";
    case "authentication_failed":
      return "Sign-in failed. Please try again.";
    case "state_protection_failed":
      return "Your sign-in session expired. Please try again.";
    case "remote_failure":
      return "Sign-in was interrupted. Please try again.";
    case "session_missing":
      return "Sign-in completed, but no session was found. Please try again.";
    default:
      return "Something went wrong during sign-in. Please try again.";
  }
}

const ROLE_LABELS: Record<number, string> = {
  0: "Student",
  1: "Faculty",
  2: "Admin",
};

export function roleLabel(role: number | null | undefined): string {
  if (role == null) return "User";
  return ROLE_LABELS[role] ?? "User";
}

/** Faculty portal for faculty/admin; student browse for students. */
export function portalHref(
  role: number | null | undefined,
  userId?: string | null,
): string {
  if (role === 0) return "/projects";
  if (userId) return `/my-projects/${userId}`;
  return "/sign-in";
}

export function myProjectsHref(userId: string): string {
  return `/my-projects/${userId}`;
}

export function newProjectHref(userId: string): string {
  return `/my-projects/${userId}/new`;
}

export function viewProjectHref(userId: string, projectId: string): string {
  return `/my-projects/${userId}/${projectId}`;
}

export function editProjectHref(userId: string, projectId: string): string {
  return `/my-projects/${userId}/${projectId}/edit`;
}
