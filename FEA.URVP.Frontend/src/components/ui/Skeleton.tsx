import type { HTMLAttributes } from "react";

/** Pulse placeholder for content that is still loading from the API. */
export function Skeleton({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      aria-hidden
      className={className ? `skeleton ${className}` : "skeleton"}
      {...props}
    />
  );
}
