"use client";

import { Button, type ButtonSize, type ButtonVariant } from "@/components/ui/Button";
import { IconTrash } from "@/components/ui/Icons";

type DeleteIconButtonProps = {
  onClick: () => void;
  loading?: boolean;
  disabled?: boolean;
  /** Accessible name. Shown as the tooltip as well unless `title` is set. */
  label?: string;
  loadingLabel?: string;
  title?: string;
  size?: ButtonSize;
  variant?: ButtonVariant;
  className?: string;
};

export function DeleteIconButton({
  onClick,
  loading = false,
  disabled = false,
  label = "Delete",
  loadingLabel = "Deleting…",
  title,
  size = "sm",
  variant = "danger",
  className,
}: DeleteIconButtonProps) {
  const caption = loading ? loadingLabel : label;

  return (
    <Button
      type="button"
      variant={variant}
      size={size}
      className={["btn-icon", className].filter(Boolean).join(" ")}
      aria-label={caption}
      title={title ?? caption}
      disabled={disabled || loading}
      onClick={onClick}
    >
      <IconTrash />
    </Button>
  );
}
