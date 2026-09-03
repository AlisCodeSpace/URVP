import { appBaseUrl, getApiBaseUrl, useSameOriginApi } from "@/lib/config";
import { logger } from "@/lib/logger";

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

/**
 * Stable role lists for RequireAuth (avoid inline arrays → effect churn).
 *
 * These describe which UI a role is *shown*, not what it may do. Every API the UI calls
 * re-authorizes the caller server-side; see `docs/SECURITY.md`.
 */
export const STUDENT_ROLES = [UserRole.Student] as const;
export const FACULTY_PORTAL_ROLES = [UserRole.Faculty, UserRole.Admin] as const;
export const ADMIN_ROLES = [UserRole.Admin] as const;

/* -------------------------------------------------------------------------- */
/* SSO endpoints                                                              */
/* -------------------------------------------------------------------------- */

const AUTH_CALLBACK_PATH = "/auth/callback";

/**
 * Return URL handed to the backend.
 *
 * Root-relative in production: the backend serves the frontend, so a relative path is same-origin
 * by construction and there is no origin for a crafted value to smuggle in. The split-origin
 * development topology needs the absolute frontend origin, which the backend accepts only because
 * it is listed in `Cors:AllowedOrigins`.
 */
function returnUrlFor(path: string): string {
  return useSameOriginApi ? path : `${appBaseUrl}${path}`;
}

export function getAuthCallbackUrl(): string {
  return returnUrlFor(AUTH_CALLBACK_PATH);
}

/**
 * Builds an SSO endpoint URL. Relative in production, so the result is correct no matter which
 * hostname the user reached the site through, and safe to bake into prerendered HTML.
 */
function ssoUrl(endpoint: string, params: Record<string, string>): string {
  const search = new URLSearchParams(params);
  return `${getApiBaseUrl()}${endpoint}?${search.toString()}`;
}

export function getAzureAdSignInUrl(
  returnUrl: string = getAuthCallbackUrl(),
): string {
  return ssoUrl("/api/auth/azuread-sso/signin", { returnUrl });
}

export function getAzureAdSignOutUrl(
  returnUrl: string = returnUrlFor("/"),
): string {
  return ssoUrl("/api/auth/azuread-sso/signout", { returnUrl });
}

/* -------------------------------------------------------------------------- */
/* Demo email sign-in (non-production only)                                   */
/* -------------------------------------------------------------------------- */

/** Demo email accounts (must match backend DevAuthAccounts). */
export const DEV_AUTH_ACCOUNTS = [
  { email: "faculty@urvp.com", label: "Faculty", role: UserRole.Faculty },
  { email: "student@urvp.com", label: "Student", role: UserRole.Student },
  { email: "admin@urvp.com", label: "Admin", role: UserRole.Admin },
] as const;

/**
 * Demo sign-in mints a privileged session from an email address alone. Gated on `NODE_ENV` so a
 * production build cannot render the UI, and the backend refuses the endpoint outright in
 * Production (see `DevSignInPolicy`) so hiding the button is not the control.
 */
export const isDevAuthEnabled = process.env.NODE_ENV !== "production";

export function getDevSignInUrl(
  email: string,
  returnUrl: string = getAuthCallbackUrl(),
): string {
  return ssoUrl("/api/auth/dev/signin", { email, returnUrl });
}

/* -------------------------------------------------------------------------- */
/* Session status                                                             */
/* -------------------------------------------------------------------------- */

export async function fetchAuthStatus(): Promise<AuthStatus> {
  try {
    const res = await fetch(`${getApiBaseUrl()}/api/auth/status`, {
      method: "GET",
      credentials: "include",
      cache: "no-store",
    });

    if (!res.ok) {
      logger.warn("Session status request was rejected.", { status: res.status });
      return { isAuthenticated: false, error: "Failed to load session" };
    }

    return (await res.json()) as AuthStatus;
  } catch {
    logger.warn("Session status request failed.");
    return { isAuthenticated: false, error: "Failed to load session" };
  }
}

/**
 * Maps a backend sign-in error code to a message. The backend deliberately sends only these
 * opaque codes; identity-provider text is never forwarded to the browser.
 */
export function authErrorMessage(
  code: string | null | undefined,
): string | null {
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

/* -------------------------------------------------------------------------- */
/* Roles                                                                      */
/* -------------------------------------------------------------------------- */

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

/* -------------------------------------------------------------------------- */
/* Route builders                                                             */
/* -------------------------------------------------------------------------- */

/**
 * Route identifiers travel in the query string, not in path segments.
 *
 * `next build` with `output: 'export'` must enumerate every page at build time, and these
 * identifiers only exist at runtime. Query parameters keep each route a single exported HTML file
 * that the backend serves for any identifier.
 *
 * The identifiers themselves are not secrets and grant nothing: the backend re-checks ownership
 * and role on every API call these pages make.
 */
export const RouteParam = {
  User: "user",
  Project: "project",
  Student: "student",
  Id: "id",
  Slug: "slug",
} as const;

function withParams(path: string, params: Record<string, string>): string {
  const search = new URLSearchParams();

  for (const [key, value] of Object.entries(params)) {
    if (value) search.set(key, value);
  }

  const query = search.toString();
  return query ? `${path}?${query}` : path;
}

/** Admin console, faculty portal, or student profile by role. */
export function portalHref(
  role: number | null | undefined,
  userId?: string | null,
): string {
  if (isAdmin(role)) return adminHref();
  if (isStudent(role)) return studentProfileHref();
  if (userId) return myProjectsHref(userId);
  return "/sign-in";
}

export function adminHref(): string {
  return "/admin";
}

export function studentProfileHref(): string {
  return "/student/profile";
}

export function projectsHref(): string {
  return "/projects";
}

export function projectDetailHref(projectId: string): string {
  return withParams("/projects/detail", { [RouteParam.Id]: projectId });
}

export function newsHref(): string {
  return "/news";
}

export function newsArticleHref(slug: string): string {
  return withParams("/news/article", { [RouteParam.Slug]: slug });
}

export function studentProjectsHref(): string {
  return "/student/projects";
}

export function studentRankingsHref(): string {
  return studentProjectsHref();
}

/** @deprecated Use studentRankingsHref — kept for older links. */
export function studentApplicationsHref(): string {
  return studentRankingsHref();
}

export function myProjectsHref(userId: string): string {
  return withParams("/my-projects", { [RouteParam.User]: userId });
}

export function newProjectHref(userId: string): string {
  return withParams("/my-projects/new", { [RouteParam.User]: userId });
}

export function viewProjectHref(userId: string, projectId: string): string {
  return withParams("/my-projects/project", {
    [RouteParam.User]: userId,
    [RouteParam.Project]: projectId,
  });
}

export function editProjectHref(userId: string, projectId: string): string {
  return withParams("/my-projects/project/edit", {
    [RouteParam.User]: userId,
    [RouteParam.Project]: projectId,
  });
}

export function viewRankedStudentHref(
  userId: string,
  projectId: string,
  studentUserId: string,
): string {
  return withParams("/my-projects/project/student", {
    [RouteParam.User]: userId,
    [RouteParam.Project]: projectId,
    [RouteParam.Student]: studentUserId,
  });
}

export function adminProjectHref(projectId: string): string {
  return withParams("/admin/projects/detail", { [RouteParam.Id]: projectId });
}

export function adminMatchingRunHref(runId: string): string {
  return withParams("/admin/matching/run", { [RouteParam.Id]: runId });
}

export function adminNewsEditHref(newsId: string): string {
  return withParams("/admin/news/edit", { [RouteParam.Id]: newsId });
}

export function adminSemesterEditHref(semesterId: string): string {
  return withParams("/admin/semesters/edit", { [RouteParam.Id]: semesterId });
}

export function adminWorkshopEditHref(workshopId: string): string {
  return withParams("/admin/workshops/edit", { [RouteParam.Id]: workshopId });
}
