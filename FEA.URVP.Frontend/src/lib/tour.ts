import { myProjectsHref, studentProfileHref, UserRole } from "@/lib/auth";

export function canTakeTour(role: number | null | undefined): boolean {
  return role === UserRole.Student || role === UserRole.Faculty;
}

export function tourStartHref(
  role: number | null | undefined,
  userId: string | null | undefined,
): string | null {
  if (role === UserRole.Student) return studentProfileHref();
  if (role === UserRole.Faculty && userId) return myProjectsHref(userId);
  return null;
}

export function isTourStartPath(
  pathname: string,
  role: number | null | undefined,
): boolean {
  if (role === UserRole.Student) return pathname === "/student/profile";
  if (role === UserRole.Faculty) return pathname === "/my-projects";
  return false;
}

function storageKey(userId: string, role: number): string {
  return `urvp.tour.done.${role}.${userId}`;
}

export function isTourCompleted(userId: string, role: number): boolean {
  try {
    return window.localStorage.getItem(storageKey(userId, role)) === "1";
  } catch {
    return false;
  }
}

export function setTourCompleted(
  userId: string,
  role: number,
  done = true,
): void {
  try {
    const key = storageKey(userId, role);
    if (done) window.localStorage.setItem(key, "1");
    else window.localStorage.removeItem(key);
  } catch {
    /* ignore quota / private mode */
  }
}

export function isCurrentTourPath(href: string): boolean {
  const url = new URL(href, window.location.origin);
  return window.location.pathname === url.pathname;
}

export async function waitForTourPath(
  href: string,
  timeoutMs = 7000,
): Promise<void> {
  const url = new URL(href, window.location.origin);
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    if (window.location.pathname === url.pathname) return;
    await new Promise((resolve) => setTimeout(resolve, 40));
  }
}

export async function waitForTourTarget(
  selector: string,
  timeoutMs = 7000,
): Promise<Element | null> {
  const deadline = Date.now() + timeoutMs;
  while (Date.now() < deadline) {
    const el = document.querySelector(selector);
    if (el) return el;
    await new Promise((resolve) => setTimeout(resolve, 40));
  }
  return null;
}

/** Bring a tour target into view, accounting for the fixed site header. */
export async function scrollTourTargetIntoView(
  selector: string,
): Promise<void> {
  const el = document.querySelector(selector);
  if (!(el instanceof HTMLElement)) return;

  const headerOffset = 96;
  const rect = el.getBoundingClientRect();
  const fullyVisible =
    rect.top >= headerOffset &&
    rect.bottom <= window.innerHeight - 24 &&
    rect.left >= 0 &&
    rect.right <= window.innerWidth;

  if (fullyVisible) return;

  el.scrollIntoView({ behavior: "smooth", block: "center", inline: "nearest" });
  await new Promise((resolve) => setTimeout(resolve, 350));
}
