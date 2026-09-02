import Image from "next/image";
import { LOGO_SRC } from "@/components/ui/Logo";

type PageLoaderProps = {
  label?: string;
  className?: string;
};

function cn(...parts: Array<string | undefined | false>) {
  return parts.filter(Boolean).join(" ");
}

/** Branded full-page / route loading indicator. */
export function PageLoader({
  label = "Loading",
  className,
}: PageLoaderProps) {
  return (
    <div
      className={cn("page-loader", className)}
      role="status"
      aria-live="polite"
      aria-label={label}
    >
      <div className="page-loader-mark">
        <span className="page-loader-spin" aria-hidden />
        <Image
          src={LOGO_SRC}
          alt=""
          width={40}
          height={40}
          className="relative z-10 object-contain"
          unoptimized
        />
      </div>
      <p className="page-loader-label">{label}</p>
    </div>
  );
}
