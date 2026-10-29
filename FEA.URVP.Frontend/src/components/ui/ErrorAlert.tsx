import type { ComponentProps } from "react";
import { Text } from "@/components/ui/Typography";

/** Red banner for a load or save failure. */
export function ErrorAlert({
  size = "3",
  className,
  ...props
}: Omit<ComponentProps<typeof Text>, "as" | "role">) {
  return (
    <Text
      as="p"
      size={size}
      role="alert"
      className={
        className
          ? `rounded-md bg-red-50 px-3 py-2 !text-red-800 ${className}`
          : "rounded-md bg-red-50 px-3 py-2 !text-red-800"
      }
      {...props}
    />
  );
}
