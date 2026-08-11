import type { ReactNode } from "react";

export type TagTone = "primary" | "secondary" | "muted";

type TagProps = {
  children: ReactNode;
  tone?: TagTone;
  className?: string;
};

const toneClass: Record<TagTone, string> = {
  primary: "ui-tag-primary",
  secondary: "ui-tag-secondary",
  muted: "ui-tag-muted",
};

export function Tag({ children, tone = "primary", className = "" }: TagProps) {
  return (
    <span className={["ui-tag", toneClass[tone], className].filter(Boolean).join(" ")}>
      {children}
    </span>
  );
}
