import type { ComponentPropsWithoutRef, ReactNode } from "react";

type Size = "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9";
type Weight = "light" | "regular" | "medium" | "bold";
type Space = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9";

const textSize: Record<Size, string> = {
  "1": "text-[0.75rem] leading-4 tracking-[0.0025em]",
  "2": "text-[0.875rem] leading-5 tracking-normal",
  "3": "text-base leading-6 tracking-normal",
  "4": "text-[1.125rem] leading-[1.625rem] tracking-[-0.0025em]",
  "5": "text-xl leading-7 tracking-[-0.005em]",
  "6": "text-2xl leading-[1.875rem] tracking-[-0.00625em]",
  "7": "text-[1.75rem] leading-9 tracking-[-0.0075em]",
  "8": "text-[2.1875rem] leading-10 tracking-[-0.01em]",
  "9": "text-[3.75rem] leading-[3.75rem] tracking-[-0.025em]",
};

const headingSize: Record<Size, string> = {
  "1": "text-[0.75rem] leading-4 tracking-[0.0025em]",
  "2": "text-[0.875rem] leading-[1.125rem] tracking-normal",
  "3": "text-base leading-[1.375rem] tracking-normal",
  "4": "text-[1.125rem] leading-6 tracking-[-0.0025em]",
  "5": "text-xl leading-[1.625rem] tracking-[-0.005em]",
  "6": "text-2xl leading-[1.875rem] tracking-[-0.00625em]",
  "7": "text-[1.75rem] leading-9 tracking-[-0.0075em]",
  "8": "text-[2.1875rem] leading-10 tracking-[-0.01em]",
  "9": "text-[3.75rem] leading-[3.75rem] tracking-[-0.025em]",
};

const weightClass: Record<Weight, string> = {
  light: "font-light",
  regular: "font-normal",
  medium: "font-medium",
  bold: "font-bold",
};

const marginClass = {
  mt: {
    "0": "mt-0",
    "1": "mt-1",
    "2": "mt-2",
    "3": "mt-3",
    "4": "mt-4",
    "5": "mt-6",
    "6": "mt-8",
    "7": "mt-10",
    "8": "mt-12",
    "9": "mt-16",
  },
  mb: {
    "0": "mb-0",
    "1": "mb-1",
    "2": "mb-2",
    "3": "mb-3",
    "4": "mb-4",
    "5": "mb-6",
    "6": "mb-8",
    "7": "mb-10",
    "8": "mb-12",
    "9": "mb-16",
  },
  my: {
    "0": "my-0",
    "1": "my-1",
    "2": "my-2",
    "3": "my-3",
    "4": "my-4",
    "5": "my-6",
    "6": "my-8",
    "7": "my-10",
    "8": "my-12",
    "9": "my-16",
  },
} as const satisfies Record<string, Record<Space, string>>;

const gapClass: Record<Space, string> = {
  "0": "gap-0",
  "1": "gap-1",
  "2": "gap-2",
  "3": "gap-3",
  "4": "gap-4",
  "5": "gap-6",
  "6": "gap-8",
  "7": "gap-10",
  "8": "gap-12",
  "9": "gap-16",
};

function cn(...parts: Array<string | undefined | false>) {
  return parts.filter(Boolean).join(" ");
}

type SharedProps = {
  size?: Size;
  weight?: Weight;
  mt?: Space;
  mb?: Space;
  my?: Space;
  className?: string;
  children?: ReactNode;
};

type TextProps = SharedProps &
  Omit<ComponentPropsWithoutRef<"span">, "color"> & {
    as?: "span" | "p" | "div" | "label";
  };

type HeadingProps = SharedProps &
  Omit<ComponentPropsWithoutRef<"h2">, "color"> & {
    as?: "h1" | "h2" | "h3" | "h4" | "h5" | "h6";
  };

export function Text({
  as: Tag = "span",
  size = "3",
  weight = "regular",
  mt,
  mb,
  my,
  className,
  ...props
}: TextProps) {
  return (
    <Tag
      className={cn(
        textSize[size],
        weightClass[weight],
        my && marginClass.my[my],
        mt && marginClass.mt[mt],
        mb && marginClass.mb[mb],
        className,
      )}
      {...props}
    />
  );
}

export function Heading({
  as: Tag = "h1",
  size = "6",
  weight = "bold",
  mt,
  mb,
  my,
  className,
  ...props
}: HeadingProps) {
  return (
    <Tag
      className={cn(
        headingSize[size],
        weightClass[weight],
        my && marginClass.my[my],
        mt && marginClass.mt[mt],
        mb && marginClass.mb[mb],
        className,
      )}
      {...props}
    />
  );
}

type FlexProps = Omit<ComponentPropsWithoutRef<"div">, "color"> & {
  gap?: Space;
  wrap?: "wrap" | "nowrap" | "wrap-reverse";
  mt?: Space;
  children?: ReactNode;
};

export function Flex({ gap, wrap, mt, className, ...props }: FlexProps) {
  return (
    <div
      className={cn(
        "flex",
        gap && gapClass[gap],
        wrap === "wrap" && "flex-wrap",
        wrap === "nowrap" && "flex-nowrap",
        wrap === "wrap-reverse" && "flex-wrap-reverse",
        mt && marginClass.mt[mt],
        className,
      )}
      {...props}
    />
  );
}
