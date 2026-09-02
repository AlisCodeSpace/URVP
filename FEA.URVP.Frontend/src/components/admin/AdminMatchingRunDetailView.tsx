"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import {
  PlacementStatusBadge,
  RunStatusBadge,
} from "@/components/admin/MatchingStatusBadge";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { AdminFormSkeleton, AdminTableSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import {
  confirmMatchingRun,
  discardMatchingRun,
  getMatchingRun,
  matchRate,
  updatePlacementStatus,
  type MatchingRunDetailDto,
  type PlacementDto,
} from "@/lib/matching-api";
import { rankLabel } from "@/lib/project-rankings-api";
import { formatWindowDate } from "@/lib/semesters-api";

type PendingAction =
  | { kind: "confirm" }
  | { kind: "discard" }
  | { kind: "release"; placement: PlacementDto; status: "Declined" | "Cancelled" };

export function AdminMatchingRunDetailView({ runId }: { runId: string }) {
  const [data, setData] = useState<MatchingRunDetailDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [pending, setPending] = useState<PendingAction | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await getMatchingRun(runId));
    } catch (err) {
      setData(null);
      setError(
        err instanceof ApiError ? err.message : "Failed to load matching run.",
      );
    } finally {
      setLoading(false);
    }
  }, [runId]);

  useEffect(() => {
    void load();
  }, [load]);

  const choiceSplit = useMemo(() => {
    if (!data?.placements.length) return null;
    const counts = { 1: 0, 2: 0, 3: 0 };
    for (const p of data.placements) {
      if (p.studentRank === 1 || p.studentRank === 2 || p.studentRank === 3) {
        counts[p.studentRank] += 1;
      }
    }
    return counts;
  }, [data]);

  async function performPending() {
    if (!pending || !data) return;
    setBusy(true);
    setError(null);
    try {
      if (pending.kind === "confirm") {
        setData(await confirmMatchingRun(data.run.id));
      } else if (pending.kind === "discard") {
        await discardMatchingRun(data.run.id);
        await load();
      } else {
        await updatePlacementStatus(pending.placement.id, pending.status);
        await load();
      }
      setPending(null);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Action failed.");
    } finally {
      setBusy(false);
    }
  }

  if (loading && !data) {
    return (
      <div className="admin-panel admin-panel--wide">
        <AdminPageHeader title="Matching run" description="Loading run details." />
        <AdminFormSkeleton fields={4} />
        <AdminTableSkeleton columns={6} />
      </div>
    );
  }

  if (error && !data) {
    return (
      <div className="admin-panel admin-panel--wide">
        <div className="admin-detail-back">
          <Button href="/admin/matching" variant="outline" size="sm">
            Back to matching
          </Button>
        </div>
        <p className="admin-users-banner is-error" role="alert">
          {error}
        </p>
        <Button type="button" variant="outline" size="sm" onClick={() => void load()}>
          Retry
        </Button>
      </div>
    );
  }

  if (!data) return null;

  const { run, warnings, placements } = data;
  const isDraft = run.status === "Draft";

  const kpis = [
    { label: "Students matched", value: `${run.studentsMatched} / ${run.studentsConsidered}`, hint: `${matchRate(run)}% of eligible students` },
    { label: "Seats available", value: String(run.seatsAvailable), hint: `${run.projectsConsidered} project${run.projectsConsidered === 1 ? "" : "s"} with open seats` },
    {
      label: "Choice split",
      value: choiceSplit ? `${choiceSplit[1]} · ${choiceSplit[2]} · ${choiceSplit[3]}` : "—",
      hint: "1st · 2nd · 3rd student choice",
    },
    { label: "Tie-breaks", value: String(run.tieBreaksUsed), hint: `Seeded lottery (seed ${run.seed})` },
  ];

  return (
    <div className="admin-panel admin-panel--wide">
      <div className="admin-detail-back">
        <Button href="/admin/matching" variant="outline" size="sm">
          Back to matching
        </Button>
      </div>

      <AdminPageHeader
        title={`${run.semesterName} — ${formatWindowDate(run.createdAt)}`}
        description={`${run.algorithmVersion}${
          run.confirmedAt ? ` · confirmed ${formatWindowDate(run.confirmedAt)}` : ""
        }`}
        tag={run.status}
      />

      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: "0.6rem",
          flexWrap: "wrap",
          marginBottom: "1.25rem",
        }}
      >
        <RunStatusBadge status={run.status} />
        {isDraft ? (
          <>
            <Button
              type="button"
              variant="primary"
              size="sm"
              disabled={busy || placements.length === 0}
              onClick={() => setPending({ kind: "confirm" })}
            >
              Confirm placements
            </Button>
            <Button
              type="button"
              variant="danger"
              size="sm"
              disabled={busy}
              onClick={() => setPending({ kind: "discard" })}
            >
              Discard draft
            </Button>
          </>
        ) : null}
      </div>

      {error ? (
        <p className="admin-users-banner is-error" role="alert">
          {error}
        </p>
      ) : null}

      <section aria-label="Run summary" className="admin-kpi-grid" style={{ marginBottom: "1.5rem" }}>
        {kpis.map((kpi) => (
          <div key={kpi.label} className="admin-kpi">
            <p className="admin-kpi-label">{kpi.label}</p>
            <p className="admin-kpi-value">{kpi.value}</p>
            <p className="admin-kpi-hint">{kpi.hint}</p>
          </div>
        ))}
      </section>

      {warnings.length > 0 ? (
        <section className="admin-detail-section" aria-labelledby="warnings-heading">
          <div className="admin-detail-section-head">
            <h3 id="warnings-heading" className="admin-detail-section-title">
              Warnings
            </h3>
            <p className="admin-detail-section-desc">
              Review before confirming. These do not block the run.
            </p>
          </div>
          <ul className="admin-activity-list">
            {warnings.map((w) => (
              <li key={w} className="admin-activity-item">
                <span className="admin-activity-mark" aria-hidden />
                <p className="admin-activity-text">{w}</p>
              </li>
            ))}
          </ul>
        </section>
      ) : null}

      <section className="admin-detail-section" aria-labelledby="placements-heading">
        <div className="admin-detail-section-head">
          <h3 id="placements-heading" className="admin-detail-section-title">
            Placements
          </h3>
          <p className="admin-detail-section-desc">
            Student choice is how the student ranked the project; faculty choice is
            how the project owner ranked the student.
          </p>
        </div>

        {placements.length === 0 ? (
          <p className="admin-users-status">
            No placements were produced. Check the warnings above.
          </p>
        ) : (
          <div className="admin-users-table-wrap">
            <table className="admin-users-table">
              <thead>
                <tr>
                  <th>Project</th>
                  <th>Student</th>
                  <th>Student choice</th>
                  <th>Faculty choice</th>
                  <th>Status</th>
                  {run.status === "Confirmed" ? <th>Actions</th> : null}
                </tr>
              </thead>
              <tbody>
                {placements.map((p) => (
                  <tr
                    key={p.id}
                    className={
                      p.status === "Declined" || p.status === "Cancelled"
                        ? "is-inactive"
                        : ""
                    }
                  >
                    <td>
                      <span className="admin-users-name">{p.projectTitle}</span>
                      <p
                        className="admin-users-meta"
                        style={{ fontSize: "0.8rem", margin: "0.3rem 0 0" }}
                      >
                        {p.facultyName}
                      </p>
                    </td>
                    <td>
                      <span className="admin-users-name">{p.studentName}</span>
                      <p
                        className="admin-users-meta"
                        style={{ fontSize: "0.8rem", margin: "0.3rem 0 0" }}
                      >
                        <a className="admin-users-email" href={`mailto:${p.studentEmail}`}>
                          {p.studentEmail}
                        </a>
                      </p>
                    </td>
                    <td>
                      <span className="admin-rank-badge">{rankLabel(p.studentRank)}</span>
                    </td>
                    <td>
                      <span className="admin-rank-badge is-faculty">
                        {rankLabel(p.facultyRank)}
                      </span>
                      {p.resolvedByTieBreak ? (
                        <p
                          className="admin-users-meta"
                          style={{ fontSize: "0.75rem", margin: "0.3rem 0 0" }}
                          title="An equally ranked candidate was rejected by the seeded lottery"
                        >
                          tie-break
                        </p>
                      ) : null}
                    </td>
                    <td>
                      <PlacementStatusBadge status={p.status} />
                    </td>
                    {run.status === "Confirmed" ? (
                      <td>
                        {p.status === "Confirmed" ? (
                          <div className="admin-value-actions">
                            <Button
                              type="button"
                              variant="outline"
                              size="sm"
                              disabled={busy}
                              onClick={() =>
                                setPending({ kind: "release", placement: p, status: "Declined" })
                              }
                            >
                              Student declined
                            </Button>
                            <Button
                              type="button"
                              variant="danger"
                              size="sm"
                              disabled={busy}
                              onClick={() =>
                                setPending({ kind: "release", placement: p, status: "Cancelled" })
                              }
                            >
                              Withdraw
                            </Button>
                          </div>
                        ) : null}
                      </td>
                    ) : null}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      <ConfirmModal
        open={pending !== null}
        onClose={() => setPending(null)}
        onConfirm={performPending}
        title={
          pending?.kind === "confirm"
            ? "Confirm these placements?"
            : pending?.kind === "discard"
              ? "Discard this draft?"
              : pending?.status === "Declined"
                ? "Mark as declined?"
                : "Withdraw this placement?"
        }
        description={
          pending?.kind === "confirm"
            ? `${placements.length} placement${placements.length === 1 ? "" : "s"} will become binding and fill project seats.`
            : pending?.kind === "discard"
              ? "Proposed placements will be voided. You can run matching again at any time."
              : pending
                ? `${pending.placement.studentName} will be released from "${pending.placement.projectTitle}" and the seat reopened for a supplementary run.`
                : undefined
        }
        confirmLabel={
          pending?.kind === "confirm"
            ? "Confirm"
            : pending?.kind === "discard"
              ? "Discard"
              : pending?.status === "Declined"
                ? "Mark declined"
                : "Withdraw"
        }
        confirmVariant={pending?.kind === "confirm" ? "primary" : "danger"}
        busy={busy}
      />
    </div>
  );
}
