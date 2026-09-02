import type { HTMLAttributes } from "react";

type SkeletonProps = HTMLAttributes<HTMLDivElement> & {
  /** Use on dark backgrounds (home workshops, page headers). */
  tone?: "light" | "dark";
};

function cn(...parts: Array<string | undefined | false>) {
  return parts.filter(Boolean).join(" ");
}

/** Pulse placeholder for content that is still loading from the API. */
export function Skeleton({
  className,
  tone = "light",
  ...props
}: SkeletonProps) {
  return (
    <div
      aria-hidden
      className={cn(
        "skeleton",
        tone === "dark" && "skeleton--dark",
        className,
      )}
      {...props}
    />
  );
}
