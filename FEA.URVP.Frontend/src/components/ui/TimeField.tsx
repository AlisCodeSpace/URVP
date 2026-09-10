"use client";

import { useState } from "react";
import { Popover } from "@radix-ui/themes";
import { IconClock } from "@/components/ui/Icons";
import {
  formatClock,
  formatTimeDisplay,
  listTimeOptions,
  parseTimeValue,
  serializeTimeValue,
} from "@/lib/time";

export type TimeFieldProps = {
  id: string;
  value: string;
  onChange: (value: string) => void;
  includeEnd?: boolean;
  placeholder?: string;
  disabled?: boolean;
};

export function TimeField({
  id,
  value,
  onChange,
  includeEnd = false,
  placeholder = "Select time",
  disabled = false,
}: TimeFieldProps) {
  const selected = parseTimeValue(value);
  const [open, setOpen] = useState(false);
  const display = formatTimeDisplay(value);

  function onStartChange(next: string) {
    onChange(serializeTimeValue(next, selected?.end ?? "", includeEnd));
  }

  function onEndChange(next: string) {
    onChange(serializeTimeValue(selected?.start ?? next, next, true));
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
          data-placeholder={display ? undefined : ""}
          data-state={open ? "open" : "closed"}
          aria-haspopup="dialog"
          aria-expanded={open}
        >
          <span className="field-date-trigger-label">
            {display || placeholder}
          </span>
          <IconClock className="field-date-trigger-icon" size={18} />
        </button>
      </Popover.Trigger>
      <Popover.Content
        size="1"
        width="auto"
        minWidth="0"
        align="start"
        sideOffset={8}
        collisionPadding={16}
        className="field-date-popover"
      >
        <div className="field-time-fields">
          <label className="field-date-time">
            {includeEnd ? "Start" : "Time"}
            <select
              className="field-input"
              value={selected?.start ?? ""}
              onChange={(event) => onStartChange(event.target.value)}
            >
              <option value="">Select</option>
              {listTimeOptions(selected?.start).map((time) => (
                <option key={`start-${time}`} value={time}>
                  {formatClock(time)}
                </option>
              ))}
            </select>
          </label>
          {includeEnd ? (
            <label className="field-date-time">
              End
              <select
                className="field-input"
                value={selected?.end ?? ""}
                disabled={!selected?.start}
                onChange={(event) => onEndChange(event.target.value)}
              >
                <option value="">Select</option>
                {listTimeOptions(selected?.end).map((time) => (
                  <option key={`end-${time}`} value={time}>
                    {formatClock(time)}
                  </option>
                ))}
              </select>
            </label>
          ) : null}
        </div>
        <div className="field-date-footer">
          <span />
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
