/**
 * The application's only logging entry point.
 *
 * Two rules make this safe in production:
 *
 * 1. Verbosity is fixed at build time from `NODE_ENV`. No query parameter, cookie or
 *    browser-storage flag can turn on verbose logging on a production host, so an attacker who
 *    can influence a victim's URL cannot make the page dump session details into the console.
 * 2. Production never emits payloads. `debug` and `info` are no-ops, and `warn`/`error` emit only
 *    a short redacted message. Raw API envelopes, identity-provider errors and stack traces stay
 *    out of the console, where a support screenshot or a shared session recording would leak them.
 */

const isDevelopment = process.env.NODE_ENV !== "production";

/** Context keys whose values are never logged, in any environment. */
const REDACTED_KEY_PATTERN =
  /(token|secret|password|passwd|cookie|authorization|credential|nonce|state|assertion|signature)/i;

const MAX_MESSAGE_LENGTH = 300;

type LogContext = Record<string, unknown>;

function redactValue(key: string, value: unknown): unknown {
  if (REDACTED_KEY_PATTERN.test(key)) {
    return "[redacted]";
  }

  if (value instanceof Error) {
    // The message can carry server detail; the stack can carry bundle paths.
    return isDevelopment ? value : value.name;
  }

  if (typeof value === "string" && value.length > MAX_MESSAGE_LENGTH) {
    return `${value.slice(0, MAX_MESSAGE_LENGTH)}...[truncated]`;
  }

  return value;
}

function redact(context?: LogContext): LogContext | undefined {
  if (!context) {
    return undefined;
  }

  const safe: LogContext = {};
  for (const [key, value] of Object.entries(context)) {
    safe[key] = redactValue(key, value);
  }

  return safe;
}

function emit(
  level: "debug" | "info" | "warn" | "error",
  message: string,
  context?: LogContext,
): void {
  const line = `[urvp] ${message}`;

  if (!isDevelopment) {
    // Message only. Context may describe a user's session or a backend failure.
    console[level](line);
    return;
  }

  const safe = redact(context);
  if (safe) {
    console[level](line, safe);
    return;
  }

  console[level](line);
}

export const logger = {
  /** Development only. Removed entirely from production behaviour. */
  debug(message: string, context?: LogContext): void {
    if (!isDevelopment) return;
    emit("debug", message, context);
  },

  /** Development only. */
  info(message: string, context?: LogContext): void {
    if (!isDevelopment) return;
    emit("info", message, context);
  },

  warn(message: string, context?: LogContext): void {
    emit("warn", message, context);
  },

  error(message: string, context?: LogContext): void {
    emit("error", message, context);
  },
};

/**
 * Whether the UI may display technical failure detail. Gated on `NODE_ENV` rather than a
 * `NEXT_PUBLIC_*` variable, so it cannot be switched on for a production deployment by editing an
 * environment variable.
 */
export const canShowTechnicalErrors = isDevelopment;
