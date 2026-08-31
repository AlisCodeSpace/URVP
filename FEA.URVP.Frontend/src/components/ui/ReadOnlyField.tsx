import type { ReactNode } from "react";

export function ReadOnlyField({
  label,
  children,
}: {
  label: string;
  children: ReactNode;
}) {
  return (
    <div>
      <p className="field-label">{label}</p>
      {children}
    </div>
  );
}

export function ReadOnlyValue({ children }: { children: ReactNode }) {
  return <div className="field-display">{children}</div>;
}

export function ReadOnlyChips({ items }: { items: string[] }) {
  if (items.length === 0) {
    return <ReadOnlyValue>—</ReadOnlyValue>;
  }

  return (
    <div className="field-display-chips">
      {items.map((item) => (
        <span key={item} className="multi-select-chip">
          <span className="multi-select-chip-label">{item}</span>
        </span>
      ))}
    </div>
  );
}
