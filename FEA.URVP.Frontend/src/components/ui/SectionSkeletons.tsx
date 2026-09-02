import { Skeleton } from "@/components/ui/Skeleton";

function times(count: number) {
  return Array.from({ length: count }, (_, i) => i);
}

export function NewsListSkeleton() {
  return (
    <div aria-busy="true" aria-label="Loading news">
      <article className="border-b border-primary/10 bg-surface">
        <div className="site-container skeleton-news-featured">
          <div className="space-y-4">
            <Skeleton className="h-3 w-36" />
            <Skeleton className="h-3 w-24" />
          </div>
          <div className="space-y-4">
            <Skeleton className="h-10 w-full max-w-xl" />
            <Skeleton className="h-4 w-full max-w-lg" />
            <Skeleton className="h-4 w-2/3 max-w-md" />
            <Skeleton className="mt-4 h-4 w-24" />
          </div>
        </div>
      </article>

      <section className="site-container py-14 sm:py-16">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-4 w-28" />
        </div>
        <div className="mt-4 border-t border-primary/10">
          {times(5).map((i) => (
            <div key={i} className="skeleton-news-row">
              <Skeleton className="h-8 w-10" />
              <div className="space-y-2">
                <Skeleton className="h-3 w-20" />
                <Skeleton className="h-3 w-16" />
              </div>
              <div className="space-y-3">
                <Skeleton className="h-6 w-full max-w-lg" />
                <Skeleton className="h-4 w-full max-w-md" />
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}

export function NewsArticleSkeleton() {
  return (
    <div aria-busy="true" aria-label="Loading story">
      <header className="news-article-hero relative overflow-hidden text-white">
        <div className="news-article-hero-grid absolute inset-0" aria-hidden />
        <div className="relative z-10 site-container site-container--narrow pb-16 pt-28 sm:pb-20 sm:pt-32">
          <Skeleton tone="dark" className="h-4 w-24" />
          <div className="mt-8 flex gap-3">
            <Skeleton tone="dark" className="h-3 w-28" />
            <Skeleton tone="dark" className="h-3 w-20" />
          </div>
          <Skeleton tone="dark" className="mt-6 h-12 w-full max-w-xl" />
          <Skeleton tone="dark" className="mt-5 h-5 w-full max-w-lg" />
          <Skeleton tone="dark" className="mt-3 h-5 w-2/3 max-w-md" />
          <Skeleton tone="dark" className="mt-8 h-4 w-40" />
        </div>
      </header>
      <div className="site-container site-container--narrow space-y-4 py-14 sm:py-16">
        {times(6).map((i) => (
          <Skeleton key={i} className={`h-4 ${i % 3 === 2 ? "w-2/3" : "w-full"}`} />
        ))}
      </div>
    </div>
  );
}

export function WorkshopCardsSkeleton({ count = 3 }: { count?: number }) {
  return (
    <ul className="workshop-card-grid" aria-busy="true" aria-label="Loading workshops">
      {times(count).map((i) => (
        <li key={i} className="flex min-w-0 w-full">
          <article className="skeleton-workshop-card flex h-full min-w-0 w-full flex-col">
            <Skeleton className="aspect-[3/2] w-full !rounded-none" />
            <div className="flex flex-1 flex-col space-y-3 px-5 py-6 sm:px-6">
              <Skeleton className="h-3 w-32" />
              <Skeleton className="h-6 w-4/5" />
              <Skeleton className="h-4 w-1/2" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-5/6" />
              <Skeleton className="mt-auto h-10 w-full" />
            </div>
          </article>
        </li>
      ))}
    </ul>
  );
}

export function WorkshopTeaserSkeleton({ count = 4 }: { count?: number }) {
  return (
    <ul
      className="mt-12 divide-y divide-white/15 border-y border-white/15"
      aria-busy="true"
      aria-label="Loading workshops"
    >
      {times(count).map((i) => (
        <li key={i} className="flex flex-col gap-3 py-7 sm:flex-row sm:items-baseline sm:justify-between">
          <div className="w-full max-w-lg space-y-3">
            <Skeleton tone="dark" className="h-6 w-3/4" />
            <Skeleton tone="dark" className="h-4 w-full" />
          </div>
          <Skeleton tone="dark" className="h-3 w-28 shrink-0" />
        </li>
      ))}
    </ul>
  );
}

export function NewsTickerSkeleton() {
  return (
    <section className="overflow-hidden bg-background py-16 sm:py-20" aria-busy="true" aria-label="Loading updates">
      <div className="site-container mb-10">
        <Skeleton className="h-3 w-24" />
      </div>
      <div className="relative border-y border-secondary/25 bg-secondary/8 py-5">
        <div className="flex gap-12 overflow-hidden px-6">
          {times(4).map((i) => (
            <div key={i} className="flex shrink-0 items-baseline gap-3">
              <Skeleton className="h-3 w-28" />
              <Skeleton className="h-4 w-48" />
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}

export function ProjectCardsSkeleton({
  count = 4,
  className = "mt-6",
}: {
  count?: number;
  className?: string;
}) {
  return (
    <ul
      className={`grid w-full gap-5 ${className}`.trim()}
      aria-busy="true"
      aria-label="Loading projects"
    >
      {times(count).map((i) => (
        <li key={i} className="min-w-0 w-full">
          <article className="skeleton-project-card w-full rounded-[var(--radius-lg)]">
            <div className="flex gap-3">
              <Skeleton className="h-3 w-24" />
              <Skeleton className="h-3 w-16" />
            </div>
            <Skeleton className="mt-4 h-6 w-4/5" />
            <Skeleton className="mt-3 h-4 w-1/2" />
            <Skeleton className="mt-4 h-4 w-full" />
            <Skeleton className="mt-2 h-4 w-2/3" />
            <div className="mt-4 flex gap-2">
              <Skeleton className="h-6 w-20 rounded-full" />
              <Skeleton className="h-6 w-24 rounded-full" />
              <Skeleton className="h-6 w-16 rounded-full" />
            </div>
          </article>
        </li>
      ))}
    </ul>
  );
}

export function ProjectDetailSkeleton() {
  return (
    <div className="space-y-8" aria-busy="true" aria-label="Loading project">
      <div className="grid gap-6 sm:grid-cols-3">
        {times(3).map((i) => (
          <div key={i} className="space-y-2">
            <Skeleton className="h-3 w-20" />
            <Skeleton className="h-5 w-32" />
          </div>
        ))}
      </div>
      <div className="space-y-3">
        <Skeleton className="h-3 w-28" />
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-full" />
        <Skeleton className="h-4 w-4/5" />
      </div>
      <div className="flex flex-wrap gap-2">
        {times(4).map((i) => (
          <Skeleton key={i} className="h-7 w-24 rounded-full" />
        ))}
      </div>
    </div>
  );
}

export function RankingsListSkeleton({ count = 3 }: { count?: number }) {
  return (
    <ul className="mt-5 space-y-3" aria-busy="true" aria-label="Loading rankings">
      {times(count).map((i) => (
        <li
          key={i}
          className="flex items-center justify-between gap-4 rounded-md border border-primary/12 px-4 py-3"
        >
          <div className="flex-1 space-y-2">
            <Skeleton className="h-4 w-40" />
            <Skeleton className="h-3 w-56" />
          </div>
          <Skeleton className="h-8 w-24" />
        </li>
      ))}
    </ul>
  );
}

export function ProfileFormSkeleton() {
  return (
    <div className="space-y-6" aria-busy="true" aria-label="Loading profile">
      {times(3).map((i) => (
        <section
          key={i}
          className="rounded-[var(--radius-lg)] border border-primary/12 bg-surface p-5 sm:p-7"
        >
          <Skeleton className="h-6 w-40" />
          <div className="mt-5 grid gap-4 sm:grid-cols-2">
            {times(4).map((j) => (
              <div key={j} className="space-y-2">
                <Skeleton className="h-3 w-24" />
                <Skeleton className="h-10 w-full" />
              </div>
            ))}
          </div>
        </section>
      ))}
    </div>
  );
}

export function AdminTableSkeleton({
  columns = 5,
  rows = 6,
}: {
  columns?: number;
  rows?: number;
}) {
  return (
    <div
      className="admin-users-table-wrap"
      aria-busy="true"
      aria-label="Loading table"
    >
      <table className="admin-users-table">
        <thead>
          <tr>
            {times(columns).map((i) => (
              <th key={i} scope="col">
                <Skeleton className="h-3 w-20" />
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {times(rows).map((r) => (
            <tr key={r}>
              {times(columns).map((c) => (
                <td key={c}>
                  <Skeleton className={`h-4 ${c === 0 ? "w-40" : "w-24"}`} />
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function AdminFormSkeleton({ fields = 6 }: { fields?: number }) {
  return (
    <div className="mt-6 grid max-w-3xl gap-5" aria-busy="true" aria-label="Loading form">
      {times(fields).map((i) => (
        <div key={i} className="space-y-2">
          <Skeleton className="h-3 w-28" />
          <Skeleton className="h-10 w-full" />
        </div>
      ))}
      <Skeleton className="h-10 w-32" />
    </div>
  );
}

export function ModalRowsSkeleton({ count = 3 }: { count?: number }) {
  return (
    <div className="space-y-2.5" aria-busy="true" aria-label="Loading">
      {times(count).map((i) => (
        <Skeleton key={i} className="h-14 w-full rounded-md" />
      ))}
    </div>
  );
}
