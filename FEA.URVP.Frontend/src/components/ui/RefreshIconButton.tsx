"use client";

import { Button, type ButtonSize } from "@/components/ui/Button";
import { IconRefresh } from "@/components/ui/Icons";

type RefreshIconButtonProps = {
  onClick: () => void;
  loading?: boolean;
  disabled?: boolean;
  /** Accessible name. Shown as the tooltip as well. */
  label?: string;
  size?: ButtonSize;
};

export function RefreshIconButton({
  onClick,
  loading = false,
  disabled = false,
  label = "Refresh list",
  size = "md",
}: RefreshIconButtonProps) {
  const caption = loading ? "Refreshing list" : label;

  return (
    <Button
      type="button"
      variant="outline"
      size={size}
      className="btn-icon"
      aria-label={caption}
      title={caption}
      disabled={disabled || loading}
      onClick={onClick}
    >
      <IconRefresh className={loading ? "is-spinning" : undefined} />
    </Button>
  );
}
