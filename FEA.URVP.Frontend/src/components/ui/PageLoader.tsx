type PageLoaderProps = {
  label?: string;
  className?: string;
};

function cn(...parts: Array<string | undefined | false>) {
  return parts.filter(Boolean).join(" ");
}

/** Branded full-viewport loading overlay used by every route. */
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
      </div>
      <p className="page-loader-label">{label}</p>
    </div>
  );
}

/** Next.js `loading.tsx` default — same overlay for every route group. */
export default function RouteLoading() {
  return <PageLoader />;
}
