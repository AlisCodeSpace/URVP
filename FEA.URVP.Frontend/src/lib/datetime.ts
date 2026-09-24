/** Display and edit every timestamp in Beirut, never the viewer's local zone. */
export const APP_TIME_ZONE = "Asia/Beirut";

const DATE_TIME_OPTIONS: Intl.DateTimeFormatOptions = {
  timeZone: APP_TIME_ZONE,
  year: "numeric",
  month: "short",
  day: "numeric",
  hour: "numeric",
  minute: "2-digit",
};

const DATE_OPTIONS: Intl.DateTimeFormatOptions = {
  timeZone: APP_TIME_ZONE,
  year: "numeric",
  month: "short",
  day: "numeric",
};

function formatToParts(
  date: Date,
  options: Intl.DateTimeFormatOptions,
): Record<string, string> {
  const parts = new Intl.DateTimeFormat("en-US", options).formatToParts(date);
  const map: Record<string, string> = {};
  for (const part of parts) {
    if (part.type !== "literal") map[part.type] = part.value;
  }
  return map;
}

function pad(n: number) {
  return String(n).padStart(2, "0");
}

/** Treat API datetimes as UTC when the payload omits a timezone. */
export function parseApiDate(iso: string): Date {
  const trimmed = iso.trim();
  if (/[zZ]$/.test(trimmed) || /[+-]\d{2}:\d{2}$/.test(trimmed)) {
    return new Date(trimmed);
  }
  return new Date(`${trimmed}Z`);
}

function asDate(value: string | Date): Date | null {
  const date = value instanceof Date ? value : parseApiDate(value);
  return Number.isNaN(date.getTime()) ? null : date;
}

export function formatAppDateTime(
  value: string | Date | null | undefined,
): string {
  if (value == null || value === "") return "—";
  const date = asDate(value);
  if (!date) return typeof value === "string" ? value : "—";
  return date.toLocaleString("en-US", DATE_TIME_OPTIONS);
}

export function formatAppDate(value: string | Date | null | undefined): string {
  if (value == null || value === "") return "—";
  const date = asDate(value);
  if (!date) return typeof value === "string" ? value : "—";
  return date.toLocaleDateString("en-US", DATE_OPTIONS);
}

/** Convert a UTC instant to a datetime-local string in Beirut: `YYYY-MM-DDTHH:mm`. */
export function toAppDatetimeInput(iso: string | null | undefined): string {
  if (!iso) return "";
  const date = parseApiDate(iso);
  if (Number.isNaN(date.getTime())) return "";
  const parts = formatToParts(date, {
    timeZone: APP_TIME_ZONE,
    hourCycle: "h23",
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
  const hour = Number(parts.hour) === 24 ? 0 : Number(parts.hour);
  return `${parts.year}-${pad(Number(parts.month))}-${pad(Number(parts.day))}T${pad(hour)}:${pad(Number(parts.minute))}`;
}

function getTimeZoneOffsetMs(date: Date, timeZone: string): number {
  const parts = formatToParts(date, {
    timeZone,
    hourCycle: "h23",
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
    second: "2-digit",
  });
  const hour = Number(parts.hour) === 24 ? 0 : Number(parts.hour);
  const asUtc = Date.UTC(
    Number(parts.year),
    Number(parts.month) - 1,
    Number(parts.day),
    hour,
    Number(parts.minute),
    Number(parts.second ?? 0),
  );
  return asUtc - date.getTime();
}

/** Interpret a datetime-local string as Beirut wall clock and return UTC ISO. */
export function fromAppDatetimeInput(local: string): string | null {
  if (!local) return null;
  const match =
    /^(\d{4})-(\d{2})-(\d{2})(?:T(\d{2}):(\d{2})(?::(\d{2}))?)?/.exec(local);
  if (!match) {
    const fallback = parseApiDate(local);
    return Number.isNaN(fallback.getTime()) ? null : fallback.toISOString();
  }

  const year = Number(match[1]);
  const month = Number(match[2]);
  const day = Number(match[3]);
  const hour = Number(match[4] ?? "0");
  const minute = Number(match[5] ?? "0");
  const second = Number(match[6] ?? "0");
  const utcGuess = new Date(Date.UTC(year, month - 1, day, hour, minute, second));
  const offset1 = getTimeZoneOffsetMs(utcGuess, APP_TIME_ZONE);
  let instant = new Date(utcGuess.getTime() - offset1);
  const offset2 = getTimeZoneOffsetMs(instant, APP_TIME_ZONE);
  if (offset1 !== offset2) {
    instant = new Date(utcGuess.getTime() - offset2);
  }
  return instant.toISOString();
}

export function toAppDateInput(iso: string | null | undefined): string {
  const value = toAppDatetimeInput(iso);
  return value ? value.slice(0, 10) : "";
}

export function todayAppDateInput(): string {
  return toAppDateInput(new Date().toISOString());
}
