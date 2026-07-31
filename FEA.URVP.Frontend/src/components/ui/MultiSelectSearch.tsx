"use client";

import { useEffect, useId, useMemo, useRef, useState } from "react";

type MultiSelectSearchProps = {
  id: string;
  options: readonly string[];
  values: string[];
  onChange: (values: string[]) => void;
  placeholder?: string;
  max: number;
  disabled?: boolean;
  hint?: string;
};

export function MultiSelectSearch({
  id,
  options: catalog,
  values,
  onChange,
  placeholder = "Choose from list",
  max,
  disabled = false,
  hint,
}: MultiSelectSearchProps) {
  const listId = useId();
  const rootRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState("");

  const allowSet = useMemo(() => new Set(catalog), [catalog]);

  const selected = useMemo(
    () => values.filter((v) => allowSet.has(v)),
    [values, allowSet],
  );

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    return catalog.filter(
      (item) =>
        !selected.includes(item) &&
        (q.length === 0 || item.toLowerCase().includes(q)),
    );
  }, [catalog, query, selected]);

  const atMax = selected.length >= max;

  useEffect(() => {
    function onPointerDown(event: MouseEvent) {
      if (!rootRef.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    }
    document.addEventListener("mousedown", onPointerDown);
    return () => document.removeEventListener("mousedown", onPointerDown);
  }, []);

  function add(item: string) {
    if (disabled || atMax || selected.includes(item) || !allowSet.has(item)) {
      return;
    }
    onChange([...selected, item]);
    setQuery("");
    inputRef.current?.focus();
  }

  function remove(item: string) {
    if (disabled) return;
    onChange(selected.filter((v) => v !== item));
  }

  return (
    <div ref={rootRef} className="multi-select">
      <div
        className={`multi-select-control field-input ${open ? "is-open" : ""}`}
        onClick={() => {
          if (disabled) return;
          setOpen(true);
          inputRef.current?.focus();
        }}
      >
        <div className="multi-select-chips">
          {selected.map((item) => (
            <span key={item} className="multi-select-chip">
              <span className="multi-select-chip-label">{item}</span>
              <button
                type="button"
                className="multi-select-chip-remove"
                aria-label={`Remove ${item}`}
                disabled={disabled}
                onClick={(e) => {
                  e.stopPropagation();
                  remove(item);
                }}
              >
                ×
              </button>
            </span>
          ))}
          <input
            ref={inputRef}
            id={id}
            type="text"
            role="combobox"
            aria-expanded={open}
            aria-controls={listId}
            aria-autocomplete="list"
            autoComplete="off"
            className="multi-select-input"
            placeholder={
              selected.length === 0 ? placeholder : atMax ? "" : "Search…"
            }
            value={query}
            disabled={disabled || atMax}
            onChange={(e) => {
              setQuery(e.target.value);
              setOpen(true);
            }}
            onFocus={() => setOpen(true)}
            onKeyDown={(e) => {
              if (e.key === "Backspace" && !query && selected.length > 0) {
                remove(selected[selected.length - 1]!);
              }
              if (e.key === "Escape") setOpen(false);
              if (e.key === "Enter") {
                e.preventDefault();
                const first = filtered[0];
                if (first) add(first);
              }
            }}
          />
        </div>
      </div>

      <p className="field-hint">
        {hint ??
          `Select up to ${max} from the list${
            selected.length > 0 ? ` (${selected.length}/${max})` : ""
          }.`}
      </p>

      {open && !atMax ? (
        <ul id={listId} role="listbox" className="multi-select-menu">
          {filtered.length === 0 ? (
            <li className="multi-select-empty">No matching options</li>
          ) : (
            filtered.map((item) => (
              <li key={item}>
                <button
                  type="button"
                  role="option"
                  className="multi-select-option"
                  onClick={() => add(item)}
                >
                  {item}
                </button>
              </li>
            ))
          )}
        </ul>
      ) : null}
    </div>
  );
}
