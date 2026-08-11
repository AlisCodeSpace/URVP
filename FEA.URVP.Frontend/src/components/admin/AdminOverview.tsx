"use client";

import Link from "next/link";
import {
  adminCatalogCounts,
  adminKpis,
  adminOverviewMeta,
  adminPipeline,
  adminRecentActivity,
  adminRoleBreakdown,
  adminSeatMeters,
} from "@/lib/admin-overview-stats";

function pct(filled: number, total: number) {
  if (total <= 0) return 0;
  return Math.round((filled / total) * 100);
}

function roleTotal() {
  return adminRoleBreakdown.reduce((sum, item) => sum + item.value, 0);
}

export function AdminOverview() {
  const accounts = roleTotal();

  return (
    <div className="admin-panel admin-panel--wide">
      <header className="admin-page-header admin-overview-head">
        <div>
          <h2 className="admin-page-title">Overview</h2>
          <p className="admin-page-desc">
            Snapshot of the Undergraduate Research Volunteer Program — matching,
            capacity, and catalog health for {adminOverviewMeta.cycle}.
          </p>
        </div>
        <div className="admin-semester-chip" title={adminOverviewMeta.cycleWindow}>
          <span className="admin-semester-chip-label">Active semester</span>
          <span className="admin-semester-chip-value">
            {adminOverviewMeta.semester}
          </span>
        </div>
      </header>

      <section aria-label="Key metrics" className="admin-kpi-grid">
        {adminKpis.map((kpi) => {
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
            <p className="admin-widget-sub">
              Profile window {adminOverviewMeta.profileWindow}
            </p>
          </header>
          <ol className="admin-pipeline">
            {adminPipeline.map((step, index) => (
              <li key={step.label} className="admin-pipeline-step">
                <span className="admin-pipeline-index" aria-hidden>
                  {index + 1}
                </span>
                <div className="min-w-0">
                  <p className="admin-pipeline-label">{step.label}</p>
                  <p className="admin-pipeline-note">{step.note}</p>
                </div>
                <p className="admin-pipeline-count">{step.count}</p>
              </li>
            ))}
          </ol>
        </section>

        <section className="admin-widget" aria-labelledby="seats-heading">
          <header className="admin-widget-head">
            <h3 id="seats-heading" className="admin-widget-title">
              Volunteer seat fill
            </h3>
            <p className="admin-widget-sub">By faculty cluster</p>
          </header>
          <ul className="admin-meter-list">
            {adminSeatMeters.map((meter) => {
              const value = pct(meter.filled, meter.total);
              return (
                <li key={meter.label} className="admin-meter">
                  <div className="admin-meter-meta">
                    <span>{meter.label}</span>
                    <span>
                      {meter.filled}/{meter.total} · {value}%
                    </span>
                  </div>
                  <div
                    className="admin-meter-track"
                    role="progressbar"
                    aria-valuenow={value}
                    aria-valuemin={0}
                    aria-valuemax={100}
                    aria-label={`${meter.label} seat fill`}
                  >
                    <span
                      className="admin-meter-fill"
                      style={{ width: `${value}%` }}
                    />
                  </div>
                </li>
              );
            })}
          </ul>
        </section>

        <section className="admin-widget" aria-labelledby="accounts-heading">
          <header className="admin-widget-head">
            <h3 id="accounts-heading" className="admin-widget-title">
              Account mix
            </h3>
            <p className="admin-widget-sub">{accounts} signed-in roles</p>
          </header>
          <div className="admin-breakdown-bar" aria-hidden>
            {adminRoleBreakdown.map((item) => (
              <span
                key={item.label}
                className={`admin-breakdown-seg is-${item.tone}`}
                style={{ flexGrow: item.value, flexBasis: 0 }}
                title={`${item.label}: ${item.value}`}
              />
            ))}
          </div>
          <ul className="admin-breakdown-legend">
            {adminRoleBreakdown.map((item) => (
              <li key={item.label}>
                <span className={`admin-breakdown-swatch is-${item.tone}`} />
                <span className="admin-breakdown-name">{item.label}</span>
                <span className="admin-breakdown-value">{item.value}</span>
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
            <p className="admin-widget-sub">Directories & value lists</p>
          </header>
          <ul className="admin-catalog-grid">
            {adminCatalogCounts.map((item) => (
              <li key={item.label}>
                <Link href={item.href} className="admin-catalog-tile">
                  <span className="admin-catalog-value">{item.value}</span>
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
            <p className="admin-widget-sub">Illustrative feed</p>
          </header>
          <ul className="admin-activity-list">
            {adminRecentActivity.map((item) => (
              <li key={item.id} className="admin-activity-item">
                <span className="admin-activity-mark" aria-hidden />
                <div className="min-w-0">
                  <p className="admin-activity-text">{item.text}</p>
                  <p className="admin-activity-meta">{item.meta}</p>
                </div>
              </li>
            ))}
          </ul>
        </section>
      </div>
    </div>
  );
}
