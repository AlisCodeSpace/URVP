import Link from "next/link";
import type { ReactNode } from "react";

export function BackLink({
  href,
  children,
}: {
  href: string;
  children: ReactNode;
}) {
  return (
    <Link href={href} className="back-link">
      <span aria-hidden className="text-base leading-none">
        ←
      </span>
      {children}
    </Link>
  );
}
