import { getApiBaseUrl } from "@/lib/config";
import { logger } from "@/lib/logger";

export type ApiEnvelope<T> = {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
};

export class ApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly errors: string[] = [],
  ) {
    super(message);
    this.name = "ApiError";
  }
}

const UNSAFE_METHODS = new Set(["POST", "PUT", "PATCH", "DELETE"]);

/** Must match AntiforgeryConfiguration.HeaderName on the backend. */
const CSRF_HEADER = "X-CSRF-TOKEN";

/** Must match AntiforgeryValidationMiddleware.FailureCode on the backend. */
const CSRF_FAILURE_CODE = "antiforgery_validation_failed";

/* -------------------------------------------------------------------------- */
/* Session expiry notification                                                */
/* -------------------------------------------------------------------------- */

type UnauthorizedListener = () => void;

const unauthorizedListeners = new Set<UnauthorizedListener>();

/**
 * Subscribes to backend 401 responses so cached session metadata can be cleared and route guards
 * re-evaluated. The backend is the only authority on session validity; this exists purely to keep
 * the UI from showing a signed-in shell around a dead session.
 */
export function onUnauthorized(listener: UnauthorizedListener): () => void {
  unauthorizedListeners.add(listener);
  return () => unauthorizedListeners.delete(listener);
}

function notifyUnauthorized(): void {
  for (const listener of unauthorizedListeners) {
    try {
      listener();
    } catch {
      // A failing listener must not mask the original request failure.
    }
  }
}

/* -------------------------------------------------------------------------- */
/* Antiforgery token                                                          */
/* -------------------------------------------------------------------------- */

/**
 * Held in memory only. Persisting it to localStorage or sessionStorage would leave a
 * CSRF-defeating value readable by any script that achieves injection, and it would survive the
 * tab that owns the session.
 */
let csrfToken: string | null = null;
let csrfRequest: Promise<string | null> | null = null;

async function requestCsrfToken(): Promise<string | null> {
  try {
    const res = await fetch(`${getApiBaseUrl()}/api/auth/csrf`, {
      method: "GET",
      headers: { Accept: "application/json" },
      credentials: "include",
      cache: "no-store",
    });

    if (!res.ok) {
      logger.warn("Could not obtain an antiforgery token.", { status: res.status });
      return null;
    }

    const envelope = (await res.json()) as ApiEnvelope<{ token?: string }>;
    return envelope?.data?.token ?? null;
  } catch {
    logger.warn("Antiforgery token request failed.");
    return null;
  }
}

async function ensureCsrfToken(forceRefresh = false): Promise<string | null> {
  if (forceRefresh) {
    csrfToken = null;
    csrfRequest = null;
  }

  if (csrfToken) {
    return csrfToken;
  }

  // Coalesced so a burst of parallel mutations issues one token request.
  csrfRequest ??= requestCsrfToken().finally(() => {
    csrfRequest = null;
  });

  csrfToken = await csrfRequest;
  return csrfToken;
}

function clearCsrfToken(): void {
  csrfToken = null;
  csrfRequest = null;
}

/* -------------------------------------------------------------------------- */
/* Request execution                                                          */
/* -------------------------------------------------------------------------- */

type RawResult<T> = {
  status: number;
  ok: boolean;
  envelope: ApiEnvelope<T> | null;
};

async function sendRequest<T>(
  path: string,
  init: RequestInit,
  csrf: string | null,
): Promise<RawResult<T>> {
  const headers = new Headers(init.headers);

  if (!headers.has("Accept")) {
    headers.set("Accept", "application/json");
  }

  // FormData must keep the boundary the browser generates, so its Content-Type is left alone.
  if (init.body && !(init.body instanceof FormData) && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  if (csrf) {
    headers.set(CSRF_HEADER, csrf);
  }

  const res = await fetch(`${getApiBaseUrl()}${path}`, {
    ...init,
    headers,
    // The session cookie is HttpOnly, so it can only travel this way. No bearer token is ever
    // attached: the backend authorizes from the cookie alone.
    credentials: "include",
    cache: "no-store",
  });

  let envelope: ApiEnvelope<T> | null = null;
  try {
    envelope = (await res.json()) as ApiEnvelope<T>;
  } catch {
    envelope = null;
  }

  return { status: res.status, ok: res.ok, envelope };
}

function isAntiforgeryFailure<T>(result: RawResult<T>): boolean {
  return result.status === 403 && result.envelope?.errors?.includes(CSRF_FAILURE_CODE) === true;
}

export async function apiFetch<T>(
  path: string,
  init: RequestInit = {},
): Promise<T> {
  const method = (init.method ?? "GET").toUpperCase();
  const needsCsrf = UNSAFE_METHODS.has(method);

  let result = await sendRequest<T>(path, init, needsCsrf ? await ensureCsrfToken() : null);

  // The paired cookie can outlive the token across a sign-in or a key-ring roll. One silent
  // refresh keeps that invisible to the user; a second failure is a real rejection.
  if (needsCsrf && isAntiforgeryFailure(result)) {
    logger.debug("Antiforgery token rejected; refreshing and retrying once.", { path });
    result = await sendRequest<T>(path, init, await ensureCsrfToken(true));
  }

  if (result.status === 401) {
    clearCsrfToken();
    notifyUnauthorized();
  }

  if (!result.ok || result.envelope?.success === false) {
    throw new ApiError(
      result.envelope?.message || `Request failed (${result.status})`,
      result.status,
      result.envelope?.errors ?? [],
    );
  }

  return result.envelope?.data as T;
}

/* -------------------------------------------------------------------------- */
/* Binary download                                                            */
/* -------------------------------------------------------------------------- */

function fileNameFromDisposition(
  header: string | null,
  fallback: string,
): string {
  if (!header) return fallback;

  const utf = header.match(/filename\*=UTF-8''([^;]+)/i);
  if (utf?.[1]) {
    try {
      return decodeURIComponent(utf[1].trim());
    } catch {
      return utf[1].trim();
    }
  }

  const ascii = header.match(/filename="?([^";]+)"?/i);
  return ascii?.[1]?.trim() || fallback;
}

/** Download a binary file endpoint (not the JSON envelope). */
export async function apiDownloadFile(
  path: string,
  fallbackFileName: string,
): Promise<void> {
  const res = await fetch(`${getApiBaseUrl()}${path}`, {
    method: "GET",
    credentials: "include",
    cache: "no-store",
  });

  if (!res.ok) {
    if (res.status === 401) {
      clearCsrfToken();
      notifyUnauthorized();
    }

    let message = `Download failed (${res.status})`;
    try {
      const envelope = (await res.json()) as ApiEnvelope<unknown>;
      if (envelope?.message) message = envelope.message;
    } catch {
      /* body is not JSON */
    }

    throw new ApiError(message, res.status);
  }

  const blob = await res.blob();
  const fileName = fileNameFromDisposition(
    res.headers.get("Content-Disposition"),
    fallbackFileName,
  );

  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}
