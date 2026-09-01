"use client";

import { useCallback, useEffect, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { ApiError } from "@/lib/api";
import {
  deleteSemester,
  formatWindowDate,
  listSemesters,
  setApplicationWindow,
  setSemesterActive,
  type SemesterDto,
} from "@/lib/semesters-api";

// ─── Status badge ─────────────────────────────────────────────────────────────

function StatusBadge({
  active,
  label,
}: {
  active: boolean;
  label: string;
}) {
  return (
    <span
      className={`admin-value-status${active ? " is-active" : ""}`}
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

// ─── Active semester control panel ────────────────────────────────────────────

function ActiveSemesterPanel({
  semester,
  onUpdated,
  onError,
}: {
  semester: SemesterDto;
  onUpdated: (next: SemesterDto) => void;
  onError: (msg: string) => void;
}) {
  const [busyCycle, setBusyCycle] = useState(false);
  const [busyWindow, setBusyWindow] = useState(false);

  async function handleToggleCycle() {
    setBusyCycle(true);
    try {
      const next = await setSemesterActive(semester.id, !semester.isActive);
      onUpdated(next);
    } catch (err) {
      onError(err instanceof ApiError ? err.message : "Failed to update cycle.");
    } finally {
      setBusyCycle(false);
    }
  }

  async function handleOpenApplications() {
    setBusyWindow(true);
    try {
      const next = await setApplicationWindow(semester.id, {
        applicationWindowStart: new Date().toISOString(),
        applicationWindowEnd: null,
      });
      onUpdated(next);
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
      const next = await setApplicationWindow(semester.id, {
        applicationWindowStart: semester.applicationWindowStart ?? null,
        applicationWindowEnd: new Date().toISOString(),
      });
      onUpdated(next);
    } catch (err) {
      onError(
        err instanceof ApiError ? err.message : "Failed to close applications.",
      );
    } finally {
      setBusyWindow(false);
    }
  }

  const appWindowIsOpen =
    semester.isApplicationWindowOpen ||
    (semester.applicationWindowStart && !semester.applicationWindowEnd);

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
        {/* ── Cycle card ── */}
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
            <StatusBadge
              active={semester.isActive}
              label={semester.isActive ? "Active" : "Inactive"}
            />
            <Button
              type="button"
              variant={semester.isActive ? "danger" : "primary"}
              size="sm"
              disabled={busyCycle}
              onClick={handleToggleCycle}
            >
              {busyCycle
                ? "Updating…"
                : semester.isActive
                  ? "End Cycle"
                  : "Start Cycle"}
            </Button>
          </div>
          <p
            style={{
              margin: "0.55rem 0 0",
              fontSize: "0.82rem",
              color: "var(--muted)",
            }}
          >
            {semester.isActive
              ? "Students can view projects. Control applications below."
              : "Start this cycle to make it the current semester."}
          </p>
        </div>

        {/* ── Application window card ── */}
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
            />
            {!semester.isApplicationWindowOpen ? (
              <Button
                type="button"
                variant="primary"
                size="sm"
                disabled={busyWindow || !semester.isActive}
                onClick={handleOpenApplications}
                title={
                  !semester.isActive
                    ? "Start the cycle first"
                    : "Open applications now"
                }
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
            {semester.applicationWindowStart
              ? `Opened: ${formatWindowDate(semester.applicationWindowStart)}`
              : "Not yet opened."}
            {semester.applicationWindowEnd
              ? ` · Closed: ${formatWindowDate(semester.applicationWindowEnd)}`
              : semester.applicationWindowStart
                ? " · Still open"
                : ""}
          </p>
          {!semester.isActive && (
            <p
              style={{
                margin: "0.3rem 0 0",
                fontSize: "0.78rem",
                color: "#b42318",
              }}
            >
              The cycle must be active to open applications.
            </p>
          )}
        </div>
      </div>
    </div>
  );
}

// ─── Main view ────────────────────────────────────────────────────────────────

export function AdminSemestersView() {
  const [semesters, setSemesters] = useState<SemesterDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pendingDelete, setPendingDelete] = useState<SemesterDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setSemesters(await listSemesters());
    } catch (err) {
      setSemesters([]);
      setError(
        err instanceof ApiError ? err.message : "Failed to load semesters.",
      );
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    void load();
  }, [load]);

  function handleUpdated(next: SemesterDto) {
    setSemesters((prev) =>
      prev.map((s) => {
        if (s.id === next.id) return next;
        // When a semester is activated, deactivate all others locally too.
        if (next.isActive && s.isActive) return { ...s, isActive: false };
        return s;
      }),
    );
  }

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

  const activeSemester = semesters.find((s) => s.isActive) ?? null;

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title="Semesters"
        description="Manage academic cycles and control when students can submit project applications."
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

      {/* ── Active semester controls ── */}
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
            onUpdated={handleUpdated}
            onError={(msg) => setError(msg)}
          />
        </div>
      ) : null}

      {/* ── Semesters table ── */}
      {loading && semesters.length === 0 ? (
        <p className="admin-users-status">Loading semesters…</p>
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
                <th>Window opens</th>
                <th>Window closes</th>
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
                  </td>
                  <td>
                    <StatusBadge
                      active={s.isApplicationWindowOpen}
                      label={s.isApplicationWindowOpen ? "Open" : "Closed"}
                    />
                  </td>
                  <td style={{ fontSize: "0.87rem", color: "var(--muted)" }}>
                    {s.applicationWindowStart
                      ? formatWindowDate(s.applicationWindowStart)
                      : "—"}
                  </td>
                  <td style={{ fontSize: "0.87rem", color: "var(--muted)" }}>
                    {s.applicationWindowEnd
                      ? formatWindowDate(s.applicationWindowEnd)
                      : s.applicationWindowStart
                        ? "Open"
                        : "—"}
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
