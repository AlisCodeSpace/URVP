import { useId, type ComponentPropsWithoutRef, type ReactNode } from "react";

export type CheckboxProps = Omit<
  ComponentPropsWithoutRef<"input">,
  "type" | "size" | "children"
> & {
  onCheckedChange?: (checked: boolean) => void;
  children?: ReactNode;
};

function cn(...parts: Array<string | undefined | false>) {
  return parts.filter(Boolean).join(" ");
}

export function Checkbox({
  id,
  className,
  children,
  disabled,
  onChange,
  onCheckedChange,
  ...props
}: CheckboxProps) {
  const autoId = useId();
  const inputId = id ?? autoId;

  const control = (
    <input
      {...props}
      id={inputId}
      type="checkbox"
      className={cn("ui-checkbox", !children && className)}
      disabled={disabled}
      onChange={(event) => {
        onChange?.(event);
        onCheckedChange?.(event.target.checked);
      }}
    />
  );

  if (!children) return control;

  return (
    <label htmlFor={inputId} className={cn("ui-checkbox-label", className)}>
      {control}
      <span className="ui-checkbox-copy">{children}</span>
    </label>
  );
}
