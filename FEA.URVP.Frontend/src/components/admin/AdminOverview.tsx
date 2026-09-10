"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { AdminBarChart, AdminDonutChart } from "@/components/admin/AdminCharts";
import { AdminTableSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import {
  getAdminOverview,
  type AdminOverviewDto,
} from "@/lib/admin-overview-api";
import {
  buildAdminKpis,
  buildPipelineChart,
  buildPlacementChart,
  buildProjectStatusChart,
  buildRankingChart,
  buildRoleBreakdown,
  buildSeatChart,
  catalogTiles,
  chartTotal,
  formatCount,
  profileWindowLabel,
  seatFillPercent,
  semesterChipTitle,
} from "@/lib/admin-overview-stats";

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
  const pipeline = data ? buildPipelineChart(data) : [];
  const accounts = data ? buildRoleBreakdown(data) : [];
  const projects = data ? buildProjectStatusChart(data) : [];
  const seats = data ? buildSeatChart(data) : [];
  const placements = data ? buildPlacementChart(data) : [];
  const rankings = data ? buildRankingChart(data) : [];
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
              ? `Snapshot of assignments, capacity, and catalog health for ${semester.name.trim()}.`
              : "Snapshot of assignments, capacity, and catalog health."}
          </p>
        </div>
        <div
          className="admin-semester-chip"
          title={data ? semesterChipTitle(data) : undefined}
        >
          <span className="admin-semester-chip-label">Active URVP cycle</span>
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
            <section
              className="admin-widget admin-widget--wide"
              aria-labelledby="pipeline-heading"
            >
              <header className="admin-widget-head">
                <h3 id="pipeline-heading" className="admin-widget-title">
                  Assignment pipeline
                </h3>
                <p className="admin-widget-sub">{profileWindowLabel(data)}</p>
              </header>
              <AdminBarChart
                slices={pipeline}
                ariaLabel="Assignment pipeline counts"
              />
            </section>

            <section className="admin-widget" aria-labelledby="accounts-heading">
              <header className="admin-widget-head">
                <h3 id="accounts-heading" className="admin-widget-title">
                  Account mix
                </h3>
                <p className="admin-widget-sub">
                  {formatCount(chartTotal(accounts))} signed-in roles
                </p>
              </header>
              <AdminDonutChart
                slices={accounts}
                centerValue={formatCount(chartTotal(accounts))}
                centerLabel="accounts"
                ariaLabel="Account mix by role"
              />
              <Link href="/admin/users" className="admin-widget-link">
                Manage users →
              </Link>
            </section>

            <section className="admin-widget" aria-labelledby="projects-heading">
              <header className="admin-widget-head">
                <h3 id="projects-heading" className="admin-widget-title">
                  Project status
                </h3>
                <p className="admin-widget-sub">
                  {formatCount(chartTotal(projects))} projects this cycle
                </p>
              </header>
              <AdminDonutChart
                slices={projects}
                centerValue={formatCount(data.projects.open)}
                centerLabel="open"
                ariaLabel="Projects by status"
              />
            </section>

            <section className="admin-widget" aria-labelledby="seats-heading">
              <header className="admin-widget-head">
                <h3 id="seats-heading" className="admin-widget-title">
                  Volunteer seats
                </h3>
                <p className="admin-widget-sub">
                  {data.projects.seatsRequired > 0
                    ? `${fill}% of open-project capacity`
                    : "No open seats posted"}
                </p>
              </header>
              <AdminDonutChart
                slices={seats}
                centerValue={`${fill}%`}
                centerLabel="filled"
                ariaLabel="Volunteer seat fill"
              />
            </section>

            <section className="admin-widget" aria-labelledby="placements-heading">
              <header className="admin-widget-head">
                <h3 id="placements-heading" className="admin-widget-title">
                  Placement outcomes
                </h3>
                <p className="admin-widget-sub">
                  Confirmed, declined, and cancelled this cycle
                </p>
              </header>
              <AdminDonutChart
                slices={placements}
                centerValue={formatCount(data.matching.confirmedPlacements)}
                centerLabel="assigned"
                ariaLabel="Placement outcomes"
              />
            </section>

            <section className="admin-widget" aria-labelledby="rankings-heading">
              <header className="admin-widget-head">
                <h3 id="rankings-heading" className="admin-widget-title">
                  Interest rankings
                </h3>
                <p className="admin-widget-sub">
                  {formatCount(data.rankings.studentRankingRows)} ranking rows
                </p>
              </header>
              <AdminBarChart
                slices={rankings}
                ariaLabel="Interest ranking coverage"
              />
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
