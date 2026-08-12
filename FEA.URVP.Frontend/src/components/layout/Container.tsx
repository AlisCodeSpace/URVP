import type { ElementType, ReactNode } from "react";

type ContainerProps = {
  children: ReactNode;
  className?: string;
  /** Narrow reading / form column (~48rem). Default is the fluid site shell. */
  narrow?: boolean;
  as?: ElementType;
};

/** Fluid page shell — widens at xl / 2xl / ultrawide breakpoints via `.site-container`. */
export function Container({
  children,
  className = "",
  narrow = false,
  as: Tag = "div",
}: ContainerProps) {
  const widthClass = narrow ? "site-container site-container--narrow" : "site-container";

  return (
    <Tag className={[widthClass, className].filter(Boolean).join(" ")}>
      {children}
    </Tag>
  );
}
