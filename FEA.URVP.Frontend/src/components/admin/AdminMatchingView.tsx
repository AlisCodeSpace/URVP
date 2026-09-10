"use client";

import { useCallback, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { RunStatusBadge } from "@/components/admin/MatchingStatusBadge";
import { Button } from "@/components/ui/Button";
import { RefreshIconButton } from "@/components/ui/RefreshIconButton";
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
  const [testBusy, setTestBusy] = useState<"run" | "confirm" | null>(null);
  const [testSeed, setTestSeed] = useState("42");
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

  function parseTestSeed(): number | null {
    const trimmed = testSeed.trim();
    if (!trimmed) return 42;
    const parsed = Number.parseInt(trimmed, 10);
    if (!Number.isFinite(parsed)) return null;
    return parsed;
  }

  async function handleTest(alsoConfirm: boolean) {
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
        err instanceof ApiError ? err.message : "Test matching run failed.",
      );
    } finally {
      setTestBusy(null);
    }
  }

  const applicationsOpen = Boolean(semester?.isApplicationWindowOpen);
  const testDisabled =
    loading || testBusy !== null || !semester || applicationsOpen;

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title="Automatic matching (test)"
        description="Student assignments are made on each project. This page keeps the deferred-acceptance matcher available for testing: it still produces a draft from rankings that you can review or confirm."
        tag={
          runs.length > 0 ? `${runs.length} run${runs.length === 1 ? "" : "s"}` : null
        }
      />

      <div
        className="admin-list-toolbar-actions"
        style={{
          flexWrap: "wrap",
          marginBottom: "1.25rem",
          gap: "0.75rem",
        }}
      >
        <Button href="/admin/projects" variant="primary" size="md">
          Assign students on projects
        </Button>
        <span className="admin-users-meta" style={{ fontSize: "0.85rem" }}>
          {semester
            ? `Active cycle: ${semester.name}${
                applicationsOpen
                  ? " · applications still open — close the window before matching"
                  : " · applications closed"
              }`
            : loading
              ? "Loading cycle…"
              : "No active URVP cycle — start a cycle before running the matcher."}
        </span>
        <RefreshIconButton loading={loading} onClick={() => void load()} />
      </div>

      <div
        className="admin-users-table-wrap"
        style={{ padding: "1rem 1.25rem", marginBottom: "1.25rem" }}
      >
        <p
          style={{
            margin: "0 0 0.35rem",
            fontSize: "0.8rem",
            textTransform: "uppercase",
            letterSpacing: "0.06em",
            color: "var(--muted)",
            fontWeight: 600,
          }}
        >
          Algorithm test
        </p>
        <p
          className="admin-users-meta"
          style={{ fontSize: "0.85rem", margin: "0 0 0.85rem", maxWidth: "42rem" }}
        >
          Uses student and faculty rankings after the application window
          closes. A fixed seed makes results reproducible. Confirming a test
          run still fills seats, so prefer a draft review unless you intend to
          keep the placements.
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
            onClick={() => void handleTest(false)}
          >
            {testBusy === "run" ? "Running…" : "Test run"}
          </Button>
          <Button
            type="button"
            variant="outline"
            size="sm"
            disabled={testDisabled}
            onClick={() => void handleTest(true)}
          >
            {testBusy === "confirm" ? "Working…" : "Test run & confirm"}
          </Button>
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
          No algorithm test runs yet. Production assignments are made on each
          project.
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
    </div>
  );
}
