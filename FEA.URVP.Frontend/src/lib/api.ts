import { apiBaseUrl } from "@/lib/config";

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
  if (init.body && !headers.has("Content-Type")) {
    headers.set("Content-Type", "application/json");
  }

  const res = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers,
    credentials: "include",
    mode: "cors",
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
