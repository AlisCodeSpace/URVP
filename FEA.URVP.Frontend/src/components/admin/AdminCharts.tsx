import { formatCount, type AdminChartSlice } from "@/lib/admin-overview-stats";

const TONE_STROKE: Record<AdminChartSlice["tone"], string> = {
  primary: "var(--primary)",
  secondary: "var(--secondary)",
  muted: "color-mix(in srgb, var(--muted) 55%, white)",
  soft: "var(--primary-soft)",
};

function ChartLegend({ slices }: { slices: AdminChartSlice[] }) {
  return (
    <ul className="admin-breakdown-legend">
      {slices.map((item) => (
        <li key={item.label}>
          <span className={`admin-breakdown-swatch is-${item.tone}`} />
          <span className="admin-breakdown-name">
            {item.label}
            {item.note ? (
              <span className="admin-chart-note">{item.note}</span>
            ) : null}
          </span>
          <span className="admin-breakdown-value">{formatCount(item.value)}</span>
        </li>
      ))}
    </ul>
  );
}

export function AdminBarChart({
  slices,
  ariaLabel,
}: {
  slices: AdminChartSlice[];
  ariaLabel: string;
}) {
  const max = Math.max(0, ...slices.map((item) => item.value));

  return (
    <div className="admin-chart-bars" role="img" aria-label={ariaLabel}>
      {slices.map((item) => {
        const pct = max > 0 ? Math.max((item.value / max) * 100, item.value > 0 ? 4 : 0) : 0;
        return (
          <div key={item.label} className="admin-chart-bar-row">
            <div className="admin-chart-bar-head">
              <span>{item.label}</span>
              <span>{formatCount(item.value)}</span>
            </div>
            {item.note ? <p className="admin-chart-bar-note">{item.note}</p> : null}
            <div className="admin-chart-bar-track">
              <span
                className={`admin-chart-bar-fill is-${item.tone}`}
                style={{ width: `${pct}%` }}
              />
            </div>
          </div>
        );
      })}
    </div>
  );
}

export function AdminDonutChart({
  slices,
  centerLabel,
  centerValue,
  ariaLabel,
}: {
  slices: AdminChartSlice[];
  centerLabel: string;
  centerValue: string;
  ariaLabel: string;
}) {
  const total = slices.reduce((sum, item) => sum + item.value, 0);
  const radius = 42;
  const circumference = 2 * Math.PI * radius;
  let offset = 0;

  return (
    <div className="admin-chart-donut-wrap">
      <div className="admin-chart-donut" role="img" aria-label={ariaLabel}>
        <svg viewBox="0 0 120 120" aria-hidden>
          <circle
            className="admin-chart-donut-track"
            cx="60"
            cy="60"
            r={radius}
            fill="none"
            strokeWidth="14"
          />
          {total > 0
            ? slices.map((item) => {
                const length = (item.value / total) * circumference;
                const dashoffset = -offset;
                offset += length;
                if (item.value <= 0) return null;
                return (
                  <circle
                    key={item.label}
                    cx="60"
                    cy="60"
                    r={radius}
                    fill="none"
                    stroke={TONE_STROKE[item.tone]}
                    strokeWidth="14"
                    strokeDasharray={`${length} ${circumference}`}
                    strokeDashoffset={dashoffset}
                    strokeLinecap="butt"
                  />
                );
              })
            : null}
        </svg>
        <div className="admin-chart-donut-center">
          <span className="admin-chart-donut-value">{centerValue}</span>
          <span className="admin-chart-donut-label">{centerLabel}</span>
        </div>
      </div>
      <ChartLegend slices={slices} />
    </div>
  );
}
