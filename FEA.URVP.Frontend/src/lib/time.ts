function pad(n: number) {
  return String(n).padStart(2, "0");
}

export function parseClock(raw: string): string | undefined {
  const value = raw.trim();
  if (!value) return undefined;

  const mer = /^(\d{1,2}):(\d{2})\s*([AaPp][Mm])$/.exec(value);
  if (mer) {
    const minutes = Number(mer[2]);
    if (minutes < 0 || minutes > 59) return undefined;
    let hours = Number(mer[1]) % 12;
    if (/p/i.test(mer[3])) hours += 12;
    return `${pad(hours)}:${pad(minutes)}`;
  }

  const day = /^(\d{1,2}):(\d{2})$/.exec(value);
  if (!day) return undefined;
  const hours = Number(day[1]);
  const minutes = Number(day[2]);
  if (hours < 0 || hours > 23 || minutes < 0 || minutes > 59) return undefined;
  return `${pad(hours)}:${pad(minutes)}`;
}

export function parseTimeValue(
  value: string,
): { start: string; end?: string } | undefined {
  const trimmed = value.trim();
  if (!trimmed) return undefined;

  const parts = trimmed.split(/\s*[–—-]\s*/).map((part) => part.trim());
  if (parts.length >= 2) {
    const endMeridiem = /[AaPp][Mm]\s*$/.exec(parts[1])?.[0]?.trim();
    const startRaw =
      /[AaPp][Mm]\s*$/.test(parts[0]) || !endMeridiem
        ? parts[0]
        : `${parts[0]} ${endMeridiem}`;
    const start = parseClock(startRaw);
    const end = parseClock(parts[1]);
    if (start && end) return { start, end };
  }

  const start = parseClock(trimmed);
  return start ? { start } : undefined;
}

export function formatClock(hhmm: string): string {
  const [hoursRaw, minutesRaw] = hhmm.split(":");
  const date = new Date();
  date.setHours(Number(hoursRaw), Number(minutesRaw), 0, 0);
  return date.toLocaleTimeString("en-US", {
    hour: "numeric",
    minute: "2-digit",
  });
}

export function formatTimeDisplay(value: string | null | undefined): string {
  if (!value) return "";
  const parsed = parseTimeValue(value);
  if (!parsed) return value;
  if (parsed.end) return `${formatClock(parsed.start)} – ${formatClock(parsed.end)}`;
  return formatClock(parsed.start);
}

export function serializeTimeValue(
  start: string,
  end: string,
  includeEnd: boolean,
): string {
  if (!start) return "";
  if (includeEnd && end) return `${start}-${end}`;
  return start;
}

const TIME_STEP_MINUTES = 15;

export function listTimeOptions(extra?: string): string[] {
  const values: string[] = [];
  for (let minutes = 0; minutes < 24 * 60; minutes += TIME_STEP_MINUTES) {
    values.push(`${pad(Math.floor(minutes / 60))}:${pad(minutes % 60)}`);
  }
  if (extra && !values.includes(extra)) {
    values.push(extra);
    values.sort();
  }
  return values;
}
