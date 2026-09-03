"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { RunStatusBadge } from "@/components/admin/MatchingStatusBadge";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { AdminTableSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import { adminMatchingRunHref } from "@/lib/auth";
import {
  confirmMatchingRun,
  listMatchingRuns,
  matchRate,
  runMatching,
  type MatchingRunDto,
} from "@/lib/matching-api";
import { formatWindowDate, getActiveSemester, type SemesterDto } from "@/lib/semesters-api";

export function AdminMatchingView() {
  const router = useRouter();
  const [runs, setRuns] = useState<MatchingRunDto[]>([]);
  const [semester, setSemester] = useState<SemesterDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [running, setRunning] = useState(false);
  const [testBusy, setTestBusy] = useState<"run" | "confirm" | null>(null);
  const [testSeed, setTestSeed] = useState("42");
  const [confirmRun, setConfirmRun] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [nextRuns, active] = await Promise.all([
        listMatchingRuns(),
        getActiveSemester(),
      ]);
      setRuns(nextRuns);
      setSemester(active);
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Failed to load matching runs.",
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  async function handleRun() {
    setRunning(true);
    setError(null);
    try {
      const detail = await runMatching();
      setConfirmRun(false);
      router.push(adminMatchingRunHref(detail.run.id));
    } catch (err) {
      setConfirmRun(false);
      setError(
        err instanceof ApiError ? err.message : "Failed to run matching.",
      );
    } finally {
      setRunning(false);
    }
  }

  function parseTestSeed(): number | null {
    const trimmed = testSeed.trim();
    if (!trimmed) return 42;
    const parsed = Number.parseInt(trimmed, 10);
    if (!Number.isFinite(parsed)) return null;
    return parsed;
  }

  async function handleManualTest(alsoConfirm: boolean) {
    const seed = parseTestSeed();
    if (seed === null) {
      setError("Test seed must be a whole number.");
      return;
    }

    setTestBusy(alsoConfirm ? "confirm" : "run");
    setError(null);
    try {
      const detail = await runMatching({ seed });
      if (alsoConfirm) {
        await confirmMatchingRun(detail.run.id);
        await load();
      } else {
        router.push(adminMatchingRunHref(detail.run.id));
      }
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Manual test run failed.",
      );
    } finally {
      setTestBusy(null);
    }
  }

  const hasDraft = runs.some((r) => r.status === "Draft");
  const testDisabled = loading || running || testBusy !== null || !semester;

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title="Matching"
        description="Run the automatic matcher on student and faculty rankings, review the proposed placements, then confirm to fill project seats. Confirmed students are excluded from later runs, so declines can be resolved with a supplementary run."
        tag={
          runs.length > 0 ? `${runs.length} run${runs.length === 1 ? "" : "s"}` : null
        }
      />

      <div
        style={{
          display: "flex",
          alignItems: "center",
          gap: "0.75rem",
          flexWrap: "wrap",
          marginBottom: "1.25rem",
        }}
      >
        <Button
          type="button"
          variant="primary"
          size="sm"
          disabled={loading || running || !semester}
          onClick={() => setConfirmRun(true)}
        >
          {running ? "Running…" : "Run matching"}
        </Button>
        <span className="admin-users-meta" style={{ fontSize: "0.85rem" }}>
          {semester
            ? `Active semester: ${semester.name}${
                semester.isApplicationWindowOpen
                  ? " · application window still open"
                  : ""
              }`
            : loading
              ? "Loading semester…"
              : "No active semester — start a cycle before running matching."}
        </span>
      </div>

      <div
        className="admin-users-table-wrap"
        style={{ padding: "1rem 1.25rem", marginBottom: "1.25rem" }}
      >
        <p
          style={{
            margin: "0 0 0.75rem",
            fontSize: "0.8rem",
            textTransform: "uppercase",
            letterSpacing: "0.06em",
            color: "var(--muted)",
            fontWeight: 600,
          }}
        >
          Manual test
        </p>
        <div
          style={{
            display: "flex",
            alignItems: "center",
            gap: "0.75rem",
            flexWrap: "wrap",
          }}
        >
          <label
            htmlFor="matching-test-seed"
            style={{ display: "flex", alignItems: "center", gap: "0.45rem" }}
          >
            <span className="admin-users-meta" style={{ fontSize: "0.85rem" }}>
              Seed
            </span>
            <input
              id="matching-test-seed"
              type="number"
              inputMode="numeric"
              value={testSeed}
              onChange={(e) => setTestSeed(e.target.value)}
              disabled={testDisabled}
              className="admin-value-status admin-value-status--control"
              style={{ width: "6.5rem" }}
            />
          </label>
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={testDisabled}
            onClick={() => void handleManualTest(false)}
          >
            {testBusy === "run" ? "Running…" : "Manual test run"}
          </Button>
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={testDisabled}
            onClick={() => void handleManualTest(true)}
          >
            {testBusy === "confirm" ? "Working…" : "Manual test run & confirm"}
          </Button>
          <span className="admin-users-meta" style={{ fontSize: "0.82rem" }}>
            Skips the production prompt. Uses a fixed seed so results are
            reproducible.
          </span>
        </div>
      </div>

      {error ? (
        <p className="admin-users-banner is-error" role="alert">
          {error}
        </p>
      ) : null}

      {loading && runs.length === 0 ? (
        <AdminTableSkeleton columns={6} />
      ) : runs.length === 0 ? (
        <p className="admin-users-status">
          No matching runs yet. Run matching to generate a draft for review.
        </p>
      ) : (
        <div className="admin-users-table-wrap">
          <table className="admin-users-table">
            <thead>
              <tr>
                <th>Run</th>
                <th>Status</th>
                <th>Matched</th>
                <th>Seats</th>
                <th>Warnings</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {runs.map((run) => (
                <tr
                  key={run.id}
                  className={run.status === "Discarded" ? "is-inactive" : ""}
                >
                  <td>
                    <span className="admin-users-name">
                      {formatWindowDate(run.createdAt)}
                    </span>
                    <p
                      className="admin-users-meta"
                      style={{ fontSize: "0.8rem", margin: "0.3rem 0 0" }}
                    >
                      {run.semesterName} · seed {run.seed}
                    </p>
                  </td>
                  <td>
                    <RunStatusBadge status={run.status} />
                  </td>
                  <td>
                    <span className="admin-rank-count">
                      {run.studentsMatched}/{run.studentsConsidered}
                    </span>{" "}
                    <span className="admin-users-meta">({matchRate(run)}%)</span>
                  </td>
                  <td>
                    <span className="admin-rank-count">{run.seatsAvailable}</span>{" "}
                    <span className="admin-users-meta">
                      across {run.projectsConsidered} project
                      {run.projectsConsidered === 1 ? "" : "s"}
                    </span>
                  </td>
                  <td>
                    <span
                      className={`admin-rank-count${run.warningCount === 0 ? " is-zero" : ""}`}
                    >
                      {run.warningCount}
                    </span>
                  </td>
                  <td>
                    <Button
                      href={adminMatchingRunHref(run.id)}
                      variant="outline"
                      size="sm"
                    >
                      {run.status === "Draft" ? "Review" : "View"}
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <ConfirmModal
        open={confirmRun}
        onClose={() => setConfirmRun(false)}
        onConfirm={handleRun}
        title="Run matching?"
        description={
          hasDraft
            ? "A draft already exists for this semester and will be discarded and replaced. Confirmed placements are kept and their seats are excluded."
            : "This creates a draft you can review before anything becomes binding. Confirmed placements from earlier runs are kept."
        }
        confirmLabel="Run"
        confirmVariant="primary"
        busy={running}
        busyLabel="Running…"
      />
    </div>
  );
}
