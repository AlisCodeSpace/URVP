"use client";

import { useEffect, useId, useMemo, useRef, useState } from "react";

export type FieldSelectOption = string | { value: string; label: string };

type FieldSelectProps = {
  id: string;
  name: string;
  placeholder: string;
  options: readonly FieldSelectOption[];
  value?: string;
  onValueChange?: (value: string) => void;
  disabled?: boolean;
};

function optionValue(option: FieldSelectOption) {
  return typeof option === "string" ? option : option.value;
}

function optionLabel(option: FieldSelectOption) {
  return typeof option === "string" ? option : option.label;
}

export function FieldSelect({
  id,
  name,
  placeholder,
  options,
  value,
  onValueChange,
  disabled = false,
}: FieldSelectProps) {
  const listId = useId();
  const rootRef = useRef<HTMLDivElement>(null);
  const [open, setOpen] = useState(false);
  const selected = value ?? "";

  const labelByValue = useMemo(() => {
    const map = new Map<string, string>();
    for (const option of options) {
      map.set(optionValue(option), optionLabel(option));
    }
    return map;
  }, [options]);

  const displayLabel = selected ? labelByValue.get(selected) : undefined;

  useEffect(() => {
    function onPointerDown(event: MouseEvent) {
      if (!rootRef.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener("mousedown", onPointerDown);
    return () => document.removeEventListener("mousedown", onPointerDown);
  }, []);

  useEffect(() => {
    if (disabled) setOpen(false);
  }, [disabled]);

  function select(next: string) {
    onValueChange?.(next);
    setOpen(false);
  }

  return (
    <div ref={rootRef} className="field-select">
      <button
        type="button"
        id={id}
        disabled={disabled}
        aria-haspopup="listbox"
        aria-expanded={open}
        aria-controls={listId}
        data-placeholder={displayLabel ? undefined : ""}
        data-state={open ? "open" : "closed"}
        className="field-select-trigger"
        onClick={() => {
          if (disabled) return;
          setOpen((prev) => !prev);
        }}
        onKeyDown={(e) => {
          if (disabled) return;
          if (e.key === "Escape") setOpen(false);
          if (e.key === "Enter" || e.key === " ") {
            e.preventDefault();
            setOpen((prev) => !prev);
          }
        }}
      >
        <span className="field-select-trigger-label">
          {displayLabel ?? placeholder}
        </span>
        <span className="field-select-trigger-icon" aria-hidden>
          ▾
        </span>
      </button>

      <input type="hidden" name={name} value={selected} />

      {open && !disabled ? (
        <ul id={listId} role="listbox" aria-labelledby={id} className="field-select-menu">
          {options.map((option) => {
            const itemValue = optionValue(option);
            const isSelected = itemValue === selected;
            return (
              <li key={itemValue}>
                <button
                  type="button"
                  role="option"
                  aria-selected={isSelected}
                  className={`field-select-item${isSelected ? " is-selected" : ""}`}
                  onClick={() => select(itemValue)}
                >
                  {optionLabel(option)}
                </button>
              </li>
            );
          })}
        </ul>
      ) : null}
    </div>
  );
}
