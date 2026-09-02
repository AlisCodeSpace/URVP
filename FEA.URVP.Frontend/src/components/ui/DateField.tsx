"use client";

import { useEffect, useMemo, useState } from "react";
import { Popover } from "@radix-ui/themes";
import { DayPicker } from "@daypicker/react";
import { IconCalendar } from "@/components/ui/Icons";

export type DateFieldProps = {
  id: string;
  value: string;
  onChange: (value: string) => void;
  includeTime?: boolean;
  placeholder?: string;
  disabled?: boolean;
};

function pad(n: number) {
  return String(n).padStart(2, "0");
}

function parseValue(value: string): Date | undefined {
  if (!value) return undefined;
  const parsed = new Date(value);
  return Number.isNaN(parsed.getTime()) ? undefined : parsed;
}

function toValue(date: Date, includeTime: boolean): string {
  const ymd = `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}`;
  if (!includeTime) return ymd;
  return `${ymd}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function timePart(date: Date | undefined): string {
  if (!date) return "00:00";
  return `${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function applyTime(date: Date, time: string): Date {
  const [hoursRaw, minutesRaw] = time.split(":");
  const hours = Number.parseInt(hoursRaw ?? "0", 10);
  const minutes = Number.parseInt(minutesRaw ?? "0", 10);
  const next = new Date(date);
  next.setHours(
    Number.isFinite(hours) ? hours : 0,
    Number.isFinite(minutes) ? minutes : 0,
    0,
    0,
  );
  return next;
}

function formatDisplay(date: Date, includeTime: boolean): string {
  return date.toLocaleString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
    ...(includeTime ? { hour: "numeric", minute: "2-digit" } : {}),
  });
}

export function DateField({
  id,
  value,
  onChange,
  includeTime = false,
  placeholder = "Select date",
  disabled = false,
}: DateFieldProps) {
  const selected = parseValue(value);
  const [open, setOpen] = useState(false);
  const [month, setMonth] = useState<Date>(() => parseValue(value) ?? new Date());

  useEffect(() => {
    const next = parseValue(value);
    if (next) setMonth(next);
  }, [value]);

  const startMonth = useMemo(() => {
    const year = new Date().getFullYear();
    return new Date(year - 5, 0);
  }, []);
  const endMonth = useMemo(() => {
    const year = new Date().getFullYear();
    return new Date(year + 8, 11);
  }, []);

  function selectDay(date: Date) {
    const next = includeTime ? applyTime(date, timePart(selected)) : date;
    onChange(toValue(next, includeTime));
    setMonth(date);
    if (!includeTime) setOpen(false);
  }

  function onTimeChange(time: string) {
    if (!selected) return;
    onChange(toValue(applyTime(selected, time), true));
  }

  function clear() {
    onChange("");
    setOpen(false);
  }

  return (
    <Popover.Root open={open} onOpenChange={setOpen}>
      <Popover.Trigger>
        <button
          type="button"
          id={id}
          disabled={disabled}
          className="field-date-trigger"
          data-placeholder={selected ? undefined : ""}
          data-state={open ? "open" : "closed"}
          aria-haspopup="dialog"
          aria-expanded={open}
        >
          <span className="field-date-trigger-label">
            {selected ? formatDisplay(selected, includeTime) : placeholder}
          </span>
          <IconCalendar className="field-date-trigger-icon" size={18} />
        </button>
      </Popover.Trigger>
      <Popover.Content
        size="1"
        width="auto"
        minWidth="0"
        align="start"
        sideOffset={8}
        className="field-date-popover"
      >
        <DayPicker
          animate
          mode="single"
          required
          month={month}
          onMonthChange={setMonth}
          selected={selected}
          onSelect={selectDay}
          captionLayout="dropdown"
          navLayout="around"
          startMonth={startMonth}
          endMonth={endMonth}
          showOutsideDays
        />
        <div className="field-date-footer">
          {includeTime ? (
            <label className="field-date-time">
              Time
              <input
                type="time"
                className="field-input"
                value={timePart(selected)}
                disabled={!selected}
                onChange={(event) => onTimeChange(event.target.value)}
              />
            </label>
          ) : (
            <span />
          )}
          <button
            type="button"
            className="field-date-clear"
            disabled={!selected}
            onClick={clear}
          >
            Clear
          </button>
        </div>
      </Popover.Content>
    </Popover.Root>
  );
}
