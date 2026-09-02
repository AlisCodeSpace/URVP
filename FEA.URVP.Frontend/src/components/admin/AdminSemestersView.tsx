"use client";

import { useCallback, useEffect, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { AdminTableSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import {
  deleteSemester,
  formatScheduleRange,
  listSemesters,
  parseApiDate,
  setApplicationWindow,
  setSemesterActive,
  type SemesterDto,
} from "@/lib/semesters-api";

function StatusBadge({
  active,
  label,
  size = "default",
}: {
  active: boolean;
  label: string;
  size?: "default" | "control";
}) {
  return (
    <span
      className={`admin-value-status${active ? " is-active" : ""}${
        size === "control" ? " admin-value-status--control" : ""
      }`}
      style={
        active
          ? undefined
          : {
              background: "color-mix(in srgb, var(--muted) 12%, white)",
              color: "var(--muted)",
            }
      }
    >
      {label}
    </span>
  );
}

function futureOrNull(iso: string | null | undefined): string | null {
  if (!iso) return null;
  return parseApiDate(iso) > new Date() ? iso : null;
}

function ActiveSemesterPanel({
  semester,
  onReload,
  onError,
}: {
  semester: SemesterDto;
  onReload: () => Promise<void>;
  onError: (msg: string) => void;
}) {
  const [busyCycle, setBusyCycle] = useState(false);
  const [busyWindow, setBusyWindow] = useState(false);

  async function handleEndCycle() {
    setBusyCycle(true);
    try {
      await setSemesterActive(semester.id, false);
      await onReload();
    } catch (err) {
      onError(err instanceof ApiError ? err.message : "Failed to end cycle.");
    } finally {
      setBusyCycle(false);
    }
  }

  async function handleOpenApplications() {
    setBusyWindow(true);
    try {
      await setApplicationWindow(semester.id, {
        applicationWindowStart: new Date().toISOString(),
        applicationWindowEnd: futureOrNull(semester.applicationWindowEnd),
      });
      await onReload();
    } catch (err) {
      onError(
        err instanceof ApiError ? err.message : "Failed to open applications.",
      );
    } finally {
      setBusyWindow(false);
    }
  }

  async function handleCloseApplications() {
    setBusyWindow(true);
    try {
      await setApplicationWindow(semester.id, {
        applicationWindowStart: semester.applicationWindowStart ?? new Date().toISOString(),
        applicationWindowEnd: new Date().toISOString(),
      });
      await onReload();
    } catch (err) {
      onError(
        err instanceof ApiError ? err.message : "Failed to close applications.",
      );
    } finally {
      setBusyWindow(false);
    }
  }

  return (
    <div
      className="admin-users-table-wrap"
      style={{ padding: "1.25rem 1.5rem", marginBottom: "1.5rem" }}
    >
      <div
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))",
          gap: "1.5rem",
        }}
      >
        <div>
          <p
            style={{
              margin: "0 0 0.45rem",
              fontSize: "0.8rem",
              textTransform: "uppercase",
              letterSpacing: "0.06em",
              color: "var(--muted)",
              fontWeight: 600,
            }}
          >
            Academic Cycle
          </p>
          <div style={{ display: "flex", alignItems: "center", gap: "0.6rem", flexWrap: "wrap" }}>
            <StatusBadge active label="Active" size="control" />
            <Button
              type="button"
              variant="danger"
              size="sm"
              disabled={busyCycle}
              onClick={handleEndCycle}
            >
              {busyCycle ? "Updating…" : "End Cycle"}
            </Button>
          </div>
          <p
            style={{
              margin: "0.55rem 0 0",
              fontSize: "0.82rem",
              color: "var(--muted)",
            }}
          >
            {formatScheduleRange(semester.cycleStart, semester.cycleEnd)}. Ends
            automatically at the end date, or instantly with the button.
          </p>
        </div>

        <div>
          <p
            style={{
              margin: "0 0 0.45rem",
              fontSize: "0.8rem",
              textTransform: "uppercase",
              letterSpacing: "0.06em",
              color: "var(--muted)",
              fontWeight: 600,
            }}
          >
            Application Window
          </p>
          <div style={{ display: "flex", alignItems: "center", gap: "0.6rem", flexWrap: "wrap" }}>
            <StatusBadge
              active={semester.isApplicationWindowOpen}
              label={semester.isApplicationWindowOpen ? "Open" : "Closed"}
              size="control"
            />
            {!semester.isApplicationWindowOpen ? (
              <Button
                type="button"
                variant="primary"
                size="sm"
                disabled={busyWindow}
                onClick={handleOpenApplications}
              >
                {busyWindow ? "Updating…" : "Open Applications"}
              </Button>
            ) : (
              <Button
                type="button"
                variant="danger"
                size="sm"
                disabled={busyWindow}
                onClick={handleCloseApplications}
              >
                {busyWindow ? "Updating…" : "Close Applications"}
              </Button>
            )}
          </div>
          <p
            style={{
              margin: "0.55rem 0 0",
              fontSize: "0.82rem",
              color: "var(--muted)",
            }}
          >
            {formatScheduleRange(
              semester.applicationWindowStart,
              semester.applicationWindowEnd,
            )}
            . Closes automatically at the end date, or instantly with the button.
          </p>
        </div>
      </div>
    </div>
  );
}

export function AdminSemestersView() {
  const [semesters, setSemesters] = useState<SemesterDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pendingDelete, setPendingDelete] = useState<SemesterDto | null>(null);
  const [startingId, setStartingId] = useState<string | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async (silent = false) => {
    if (!silent) setLoading(true);
    setError(null);
    try {
      setSemesters(await listSemesters());
    } catch (err) {
      setSemesters([]);
      setError(
        err instanceof ApiError ? err.message : "Failed to load semesters.",
      );
    } finally {
      if (!silent) setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  async function onConfirmDelete() {
    if (!pendingDelete) return;
    setDeleting(true);
    try {
      await deleteSemester(pendingDelete.id);
      setPendingDelete(null);
      await load();
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Failed to delete semester.",
      );
    } finally {
      setDeleting(false);
    }
  }

  async function handleStartCycle(semester: SemesterDto) {
    setStartingId(semester.id);
    setError(null);
    try {
      await setSemesterActive(semester.id, true);
      await load(true);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to start cycle.");
    } finally {
      setStartingId(null);
    }
  }

  const activeSemester = semesters.find((s) => s.isActive) ?? null;

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title="Semesters"
        description="Schedule academic cycles and application windows with start and end dates. Each period closes automatically when its end date is reached. Edit a semester to extend or shorten it, or use the instant controls below."
        tag={
          semesters.length > 0
            ? `${semesters.length} semester${semesters.length === 1 ? "" : "s"}`
            : null
        }
      />

      <div style={{ marginBottom: "1.25rem" }}>
        <Button href="/admin/semesters/new" variant="primary" size="sm">
          Add semester
        </Button>
      </div>

      {error ? (
        <p className="admin-users-banner is-error" role="alert">
          {error}
        </p>
      ) : null}

      {!loading && activeSemester ? (
        <div>
          <p
            style={{
              margin: "0 0 0.6rem",
              fontSize: "0.85rem",
              fontWeight: 600,
              color: "var(--primary-deep)",
            }}
          >
            {activeSemester.name} — Active Semester Controls
          </p>
          <ActiveSemesterPanel
            semester={activeSemester}
            onReload={() => load(true)}
            onError={(msg) => setError(msg)}
          />
        </div>
      ) : null}

      {loading && semesters.length === 0 ? (
        <AdminTableSkeleton columns={5} />
      ) : semesters.length === 0 ? (
        <p className="admin-users-status">
          No semesters yet. Add one to get started.
        </p>
      ) : (
        <div className="admin-users-table-wrap">
          <table className="admin-users-table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Cycle</th>
                <th>Applications</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {semesters.map((s) => (
                <tr key={s.id} className={s.isActive ? "" : "is-inactive"}>
                  <td>
                    <span className="admin-users-name">{s.name}</span>
                    {s.description ? (
                      <p
                        className="admin-users-meta"
                        style={{ fontSize: "0.82rem" }}
                      >
                        {s.description}
                      </p>
                    ) : null}
                  </td>
                  <td>
                    <StatusBadge
                      active={s.isActive}
                      label={s.isActive ? "Active" : "Inactive"}
                    />
                    <p
                      className="admin-users-meta"
                      style={{ fontSize: "0.8rem", margin: "0.35rem 0 0" }}
                    >
                      {formatScheduleRange(s.cycleStart, s.cycleEnd)}
                    </p>
                  </td>
                  <td>
                    <StatusBadge
                      active={s.isApplicationWindowOpen}
                      label={s.isApplicationWindowOpen ? "Open" : "Closed"}
                    />
                    <p
                      className="admin-users-meta"
                      style={{ fontSize: "0.8rem", margin: "0.35rem 0 0" }}
                    >
                      {formatScheduleRange(
                        s.applicationWindowStart,
                        s.applicationWindowEnd,
                      )}
                    </p>
                  </td>
                  <td>
                    <div className="admin-value-actions">
                      <Button
                        href={`/admin/semesters/${s.id}`}
                        variant="outline"
                        size="sm"
                      >
                        Edit
                      </Button>
                      {!s.isActive ? (
                        <Button
                          type="button"
                          variant="primary"
                          size="sm"
                          disabled={startingId === s.id}
                          onClick={() => void handleStartCycle(s)}
                        >
                          {startingId === s.id ? "Starting…" : "Start Cycle"}
                        </Button>
                      ) : null}
                      <Button
                        type="button"
                        variant="danger"
                        size="sm"
                        disabled={s.isActive}
                        title={
                          s.isActive
                            ? "End the cycle before deleting"
                            : undefined
                        }
                        onClick={() => setPendingDelete(s)}
                      >
                        Delete
                      </Button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <ConfirmModal
        open={Boolean(pendingDelete)}
        onClose={() => setPendingDelete(null)}
        onConfirm={onConfirmDelete}
        title="Delete this semester?"
        description={
          pendingDelete
            ? `"${pendingDelete.name}" will be permanently removed.`
            : undefined
        }
        confirmLabel="Delete"
        busy={deleting}
      />
    </div>
  );
}
