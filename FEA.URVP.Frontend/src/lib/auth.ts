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

export const UserRole = {
  Student: 0,
  Faculty: 1,
  Admin: 2,
} as const;

export type UserRoleValue = (typeof UserRole)[keyof typeof UserRole];

/** Stable role lists for RequireAuth (avoid inline arrays → effect churn). */
export const STUDENT_ROLES = [UserRole.Student] as const;
export const FACULTY_PORTAL_ROLES = [UserRole.Faculty, UserRole.Admin] as const;
export const ADMIN_ROLES = [UserRole.Admin] as const;

/** Temporary: force these emails into the student portal for FE testing. */
const STUDENT_ROLE_OVERRIDES = new Set(["aa624@aub.edu.lb"]);

function applyRoleOverrides(status: AuthStatus): AuthStatus {
  const email = status.email?.trim().toLowerCase();
  if (!email || !STUDENT_ROLE_OVERRIDES.has(email)) return status;
  if (status.role === UserRole.Student) return status;
  console.warn("[auth] Temporary student role override for", email);
  return { ...status, role: UserRole.Student };
}

export function getAuthCallbackUrl(): string {
  const origin =
    typeof window !== "undefined" && window.location?.origin
      ? window.location.origin
      : appBaseUrl;
  return `${origin.replace(/\/$/, "")}/auth/callback`;
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

/** Demo email accounts (must match backend DevAuthAccounts). */
export const DEV_AUTH_ACCOUNTS = [
  { email: "faculty@urvp.com", label: "Faculty", role: UserRole.Faculty },
  { email: "student@urvp.com", label: "Student", role: UserRole.Student },
  { email: "admin@urvp.com", label: "Admin", role: UserRole.Admin },
] as const;

/** Temporary: email sign-in is shown in all environments for demo. */
export const isDevAuthEnabled = true;

export function getDevSignInUrl(
  email: string,
  returnUrl: string = getAuthCallbackUrl(),
): string {
  const url = new URL("/api/auth/dev/signin", apiBaseUrl);
  url.searchParams.set("email", email);
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

  const data = applyRoleOverrides((await res.json()) as AuthStatus);
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
  [UserRole.Student]: "Student",
  [UserRole.Faculty]: "Faculty",
  [UserRole.Admin]: "Admin",
};

export function roleLabel(role: number | null | undefined): string {
  if (role == null) return "User";
  return ROLE_LABELS[role] ?? "User";
}

export function isStudent(role: number | null | undefined): boolean {
  return role === UserRole.Student;
}

export function isFacultyOrAdmin(role: number | null | undefined): boolean {
  return role === UserRole.Faculty || role === UserRole.Admin;
}

export function isAdmin(role: number | null | undefined): boolean {
  return role === UserRole.Admin;
}

/** Admin console, faculty portal, or student profile by role. */
export function portalHref(
  role: number | null | undefined,
  userId?: string | null,
): string {
  if (isAdmin(role)) return "/admin";
  if (isStudent(role)) return "/student/profile";
  if (userId) return `/my-projects/${userId}`;
  return "/sign-in";
}

export function adminHref(): string {
  return "/admin";
}

export function studentProfileHref(): string {
  return "/student/profile";
}

export function studentRankingsHref(): string {
  return "/student/rankings";
}

/** @deprecated Use studentRankingsHref — kept for older links. */
export function studentApplicationsHref(): string {
  return studentRankingsHref();
}

export function studentProjectsHref(): string {
  return "/student/projects";
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
