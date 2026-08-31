import { getApiBaseUrl } from "@/lib/config";

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

export async function apiFetch<T>(
  path: string,
  init: RequestInit = {},
): Promise<T> {
  const headers = new Headers(init.headers);
  if (!headers.has("Accept")) {
    headers.set("Accept", "application/json");
  }
  if (
    init.body &&
    !(init.body instanceof FormData) &&
    !headers.has("Content-Type")
  ) {
    headers.set("Content-Type", "application/json");
  }

  const res = await fetch(`${getApiBaseUrl()}${path}`, {
    ...init,
    headers,
    credentials: "include",
    cache: "no-store",
  });

  let envelope: ApiEnvelope<T> | null = null;
  try {
    envelope = (await res.json()) as ApiEnvelope<T>;
  } catch {
    envelope = null;
  }

  if (!res.ok || envelope?.success === false) {
    throw new ApiError(
      envelope?.message || `Request failed (${res.status})`,
      res.status,
      envelope?.errors ?? [],
    );
  }

  return envelope?.data as T;
}

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
