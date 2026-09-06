"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { AdminTableSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import {
  getAdminOverview,
  type AdminOverviewDto,
} from "@/lib/admin-overview-api";
import {
  buildAdminKpis,
  buildRoleBreakdown,
  catalogTiles,
  formatCount,
  profileWindowLabel,
  seatFillPercent,
  semesterChipTitle,
} from "@/lib/admin-overview-stats";

function roleTotal(overview: AdminOverviewDto) {
  return (
    overview.accounts.students +
    overview.accounts.faculty +
    overview.accounts.admins
  );
}

export function AdminOverview() {
  const [data, setData] = useState<AdminOverviewDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await getAdminOverview());
    } catch (err) {
      setData(null);
      setError(
        err instanceof ApiError ? err.message : "Failed to load overview.",
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  const semester = data?.semester ?? null;
  const fill = data
    ? seatFillPercent(data.projects.seatsFilled, data.projects.seatsRequired)
    : 0;

  return (
    <div className="admin-panel admin-panel--wide">
      <header className="admin-page-header admin-overview-head">
        <div>
          <h2 className="admin-page-title">Overview</h2>
          <p className="admin-page-desc">
            {semester
              ? `Snapshot of matching readiness, capacity, and catalog health for ${semester.name.trim()}.`
              : "Snapshot of matching readiness, capacity, and catalog health."}
          </p>
        </div>
        <div
          className="admin-semester-chip"
          title={data ? semesterChipTitle(data) : undefined}
        >
          <span className="admin-semester-chip-label">Active semester</span>
          <span className="admin-semester-chip-value">
            {loading ? "Loading…" : (semester?.name ?? "None")}
          </span>
        </div>
      </header>

      {error ? (
        <p className="admin-users-banner is-error" role="alert">
          {error}{" "}
          <button type="button" className="admin-widget-link" onClick={() => void load()}>
            Try again
          </button>
        </p>
      ) : null}

      {loading || !data ? (
        <AdminTableSkeleton columns={4} rows={6} />
      ) : (
        <>
          <section aria-label="Key metrics" className="admin-kpi-grid">
            {buildAdminKpis(data).map((kpi) => {
              const inner = (
                <>
                  <p className="admin-kpi-label">{kpi.label}</p>
                  <p className="admin-kpi-value">{kpi.value}</p>
                  <p className="admin-kpi-hint">{kpi.hint}</p>
                  {kpi.delta ? (
                    <p className="admin-kpi-delta">{kpi.delta}</p>
                  ) : null}
                </>
              );

              return kpi.href ? (
                <Link key={kpi.id} href={kpi.href} className="admin-kpi">
                  {inner}
                </Link>
              ) : (
                <div key={kpi.id} className="admin-kpi">
                  {inner}
                </div>
              );
            })}
          </section>

          <div className="admin-widget-grid">
            <section className="admin-widget" aria-labelledby="pipeline-heading">
              <header className="admin-widget-head">
                <h3 id="pipeline-heading" className="admin-widget-title">
                  Matching pipeline
                </h3>
                <p className="admin-widget-sub">{profileWindowLabel(data)}</p>
              </header>
              <ol className="admin-pipeline">
                {data.pipeline.map((step, index) => (
                  <li key={step.id} className="admin-pipeline-step">
                    <span className="admin-pipeline-index" aria-hidden>
                      {index + 1}
                    </span>
                    <div className="min-w-0">
                      <p className="admin-pipeline-label">{step.label}</p>
                      <p className="admin-pipeline-note">{step.note}</p>
                    </div>
                    <p className="admin-pipeline-count">
                      {formatCount(step.count)}
                    </p>
                  </li>
                ))}
              </ol>
            </section>

            <section className="admin-widget" aria-labelledby="attention-heading">
              <header className="admin-widget-head">
                <h3 id="attention-heading" className="admin-widget-title">
                  Needs action
                </h3>
                <p className="admin-widget-sub">
                  {data.projects.seatsRequired > 0
                    ? `Open seats ${data.projects.seatsFilled}/${data.projects.seatsRequired} · ${fill}%`
                    : "Blockers that affect a clean matching run"}
                </p>
              </header>
              {data.projects.seatsRequired > 0 ? (
                <div
                  className="admin-meter"
                  style={{ marginBottom: "0.9rem" }}
                >
                  <div
                    className="admin-meter-track"
                    role="progressbar"
                    aria-valuenow={fill}
                    aria-valuemin={0}
                    aria-valuemax={100}
                    aria-label="Open project seat fill"
                  >
                    <span
                      className="admin-meter-fill"
                      style={{ width: `${fill}%` }}
                    />
                  </div>
                </div>
              ) : null}
              {data.attention.length === 0 ? (
                <p className="admin-users-status">
                  No blockers — matching inputs look ready.
                </p>
              ) : (
                <ul className="admin-activity-list">
                  {data.attention.map((item) => (
                    <li key={item.id}>
                      <Link
                        href={item.href}
                        className={`admin-activity-item admin-attention-link is-${item.severity}`}
                      >
                        <span
                          className={`admin-activity-mark is-${item.severity}`}
                          aria-hidden
                        />
                        <div className="min-w-0">
                          <p className="admin-activity-text">{item.text}</p>
                          <p className="admin-activity-meta">
                            {item.severity === "warning"
                              ? "Needs attention"
                              : "For follow-up"}
                          </p>
                        </div>
                      </Link>
                    </li>
                  ))}
                </ul>
              )}
            </section>

            <section className="admin-widget" aria-labelledby="accounts-heading">
              <header className="admin-widget-head">
                <h3 id="accounts-heading" className="admin-widget-title">
                  Account mix
                </h3>
                <p className="admin-widget-sub">
                  {formatCount(roleTotal(data))} signed-in roles
                </p>
              </header>
              <div className="admin-breakdown-bar" aria-hidden>
                {buildRoleBreakdown(data).map((item) => (
                  <span
                    key={item.label}
                    className={`admin-breakdown-seg is-${item.tone}`}
                    style={{ flexGrow: Math.max(item.value, 0), flexBasis: 0 }}
                    title={`${item.label}: ${item.value}`}
                  />
                ))}
              </div>
              <ul className="admin-breakdown-legend">
                {buildRoleBreakdown(data).map((item) => (
                  <li key={item.label}>
                    <span className={`admin-breakdown-swatch is-${item.tone}`} />
                    <span className="admin-breakdown-name">{item.label}</span>
                    <span className="admin-breakdown-value">
                      {formatCount(item.value)}
                    </span>
                  </li>
                ))}
              </ul>
              <Link href="/admin/users" className="admin-widget-link">
                Manage users →
              </Link>
            </section>

            <section className="admin-widget" aria-labelledby="catalog-heading">
              <header className="admin-widget-head">
                <h3 id="catalog-heading" className="admin-widget-title">
                  Catalog health
                </h3>
                <p className="admin-widget-sub">Directories, sessions, and news</p>
              </header>
              <ul className="admin-catalog-grid">
                {catalogTiles(data).map((item) => (
                  <li key={item.label}>
                    <Link href={item.href} className="admin-catalog-tile">
                      <span className="admin-catalog-value">
                        {formatCount(item.value)}
                      </span>
                      <span className="admin-catalog-label">{item.label}</span>
                    </Link>
                  </li>
                ))}
              </ul>
            </section>

            <section
              className="admin-widget admin-widget--span"
              aria-labelledby="activity-heading"
            >
              <header className="admin-widget-head">
                <h3 id="activity-heading" className="admin-widget-title">
                  Recent activity
                </h3>
                <p className="admin-widget-sub">
                  Latest projects, matching runs, workshops, and news
                </p>
              </header>
              {data.recentActivity.length === 0 ? (
                <p className="admin-users-status">
                  No recent records yet. Activity will appear as people use the
                  portal.
                </p>
              ) : (
                <ul className="admin-activity-list">
                  {data.recentActivity.map((item) => (
                    <li key={item.id} className="admin-activity-item">
                      <span className="admin-activity-mark" aria-hidden />
                      <div className="min-w-0">
                        <p className="admin-activity-text">{item.text}</p>
                        <p className="admin-activity-meta">{item.meta}</p>
                      </div>
                    </li>
                  ))}
                </ul>
              )}
            </section>
          </div>
        </>
      )}
    </div>
  );
}
