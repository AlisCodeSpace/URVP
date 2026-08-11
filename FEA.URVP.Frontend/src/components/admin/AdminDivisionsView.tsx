"use client";

import { useCallback, useEffect, useId, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { FieldSelect } from "@/components/ui/FieldSelect";
import { IconPencil, IconPlus, IconTrash } from "@/components/ui/Icons";
import { ApiError } from "@/lib/api";
import {
  createDivision,
  deleteDivision,
  listDivisions,
  updateDivision,
  type DivisionDto,
  type PaginatedDivisions,
} from "@/lib/divisions-api";

const PAGE_SIZE = 20;

const STATUS_OPTIONS = [
  { value: "active", label: "Active" },
  { value: "inactive", label: "Inactive" },
] as const;

export function AdminDivisionsView() {
  const searchId = useId();
  const draftNameId = useId();
  const draftDescId = useId();
  const draftStatusId = useId();

  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [data, setData] = useState<PaginatedDivisions | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  const [draftOpen, setDraftOpen] = useState(false);
  const [draftName, setDraftName] = useState("");
  const [draftDescription, setDraftDescription] = useState("");
  const [draftActive, setDraftActive] = useState(true);
  const [adding, setAdding] = useState(false);

  const [busyId, setBusyId] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editName, setEditName] = useState("");
  const [editDescription, setEditDescription] = useState("");
  const [editActive, setEditActive] = useState(true);
  const [pendingDelete, setPendingDelete] = useState<DivisionDto | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const page = await listDivisions({
        search,
        pageNumber,
        pageSize: PAGE_SIZE,
      });
      setData(page);
    } catch (err) {
      setData(null);
      setError(
        err instanceof ApiError ? err.message : "Failed to load divisions.",
      );
    } finally {
      setLoading(false);
    }
  }, [search, pageNumber]);

  useEffect(() => {
    void load();
  }, [load]);

  useEffect(() => {
    const handle = window.setTimeout(() => {
      setPageNumber(1);
      setSearch(searchInput.trim());
    }, 300);
    return () => window.clearTimeout(handle);
  }, [searchInput]);

  function openDraft() {
    setEditingId(null);
    setDraftOpen(true);
    setDraftName("");
    setDraftDescription("");
    setDraftActive(true);
    setMessage(null);
    setError(null);
  }

  function cancelDraft() {
    setDraftOpen(false);
    setDraftName("");
    setDraftDescription("");
    setDraftActive(true);
  }

  async function onSaveDraft() {
    const name = draftName.trim();
    if (!name) return;

    setAdding(true);
    setMessage(null);
    setError(null);
    try {
      await createDivision({
        name,
        description: draftDescription.trim(),
        isActive: draftActive,
      });
      cancelDraft();
      setMessage(`Added “${name}”.`);
      if (pageNumber === 1) {
        await load();
      } else {
        setPageNumber(1);
      }
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Failed to add division.",
      );
    } finally {
      setAdding(false);
    }
  }

  async function onSaveEdit(item: DivisionDto) {
    const name = editName.trim();
    if (!name) return;

    setBusyId(item.id);
    setMessage(null);
    setError(null);
    try {
      await updateDivision(item.id, {
        name,
        description: editDescription.trim(),
        isActive: editActive,
      });
      setEditingId(null);
      setMessage(`Updated “${name}”.`);
      await load();
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Failed to update division.",
      );
    } finally {
      setBusyId(null);
    }
  }

  async function onConfirmDelete() {
    if (!pendingDelete) return;

    const item = pendingDelete;
    setBusyId(item.id);
    setMessage(null);
    setError(null);
    try {
      await deleteDivision(item.id);
      setPendingDelete(null);
      setMessage(`Deleted “${item.name}”.`);
      await load();
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Failed to delete division.",
      );
    } finally {
      setBusyId(null);
    }
  }

  const totalPages = data
    ? Math.max(1, Math.ceil(data.totalCount / data.pageSize))
    : 1;
  const items = data?.items ?? [];
  const showTable = Boolean(data) || draftOpen;
  const totalCount = data?.totalCount ?? null;

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title="Divisions"
        description="Departments and academic divisions."
        tag={
          totalCount == null
            ? null
            : `${totalCount} Division${totalCount === 1 ? "" : "s"}`
        }
      />

      <section className="admin-value-section" aria-label="Divisions">
        <div className="admin-value-toolbar">
          <div className="admin-value-field">
            <label className="field-label" htmlFor={searchId}>
              Search
            </label>
            <input
              id={searchId}
              type="search"
              className="field-input"
              placeholder="Filter divisions"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
            />
          </div>
          <div className="admin-value-toolbar-actions">
            <Button
              type="button"
              variant="primary"
              size="sm"
              disabled={draftOpen}
              onClick={openDraft}
            >
              <IconPlus />
              Add division
            </Button>
          </div>
        </div>

        {error ? (
          <p className="admin-users-banner is-error" role="alert">
            {error}
          </p>
        ) : null}
        {message ? (
          <p className="admin-users-banner" role="status">
            {message}
          </p>
        ) : null}

        {loading && !data && !draftOpen ? (
          <p className="admin-users-status">Loading…</p>
        ) : !showTable || (!items.length && !draftOpen) ? (
          <p className="admin-users-status">No divisions yet.</p>
        ) : (
          <>
            <div className="admin-users-table-wrap">
              <table className="admin-users-table">
                <thead>
                  <tr>
                    <th scope="col">Name</th>
                    <th scope="col">Description</th>
                    <th scope="col">Status</th>
                    <th scope="col">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {draftOpen ? (
                    <tr className="admin-value-draft-row">
                      <td>
                        <input
                          id={draftNameId}
                          type="text"
                          className="field-input"
                          placeholder="Division name"
                          value={draftName}
                          disabled={adding}
                          autoFocus
                          onChange={(e) => setDraftName(e.target.value)}
                          onKeyDown={(e) => {
                            if (e.key === "Enter") {
                              e.preventDefault();
                              void onSaveDraft();
                            }
                            if (e.key === "Escape") cancelDraft();
                          }}
                        />
                      </td>
                      <td>
                        <input
                          id={draftDescId}
                          type="text"
                          className="field-input"
                          placeholder="Description"
                          value={draftDescription}
                          disabled={adding}
                          onChange={(e) => setDraftDescription(e.target.value)}
                          onKeyDown={(e) => {
                            if (e.key === "Enter") {
                              e.preventDefault();
                              void onSaveDraft();
                            }
                            if (e.key === "Escape") cancelDraft();
                          }}
                        />
                      </td>
                      <td>
                        <div className="admin-value-status-select">
                          <FieldSelect
                            id={draftStatusId}
                            name="division-draft-status"
                            placeholder="Status"
                            options={STATUS_OPTIONS}
                            value={draftActive ? "active" : "inactive"}
                            disabled={adding}
                            onValueChange={(value) =>
                              setDraftActive(value === "active")
                            }
                          />
                        </div>
                      </td>
                      <td>
                        <div className="admin-value-actions">
                          <Button
                            type="button"
                            variant="primary"
                            size="sm"
                            disabled={adding || !draftName.trim()}
                            onClick={() => void onSaveDraft()}
                          >
                            {adding ? "Saving…" : "Save"}
                          </Button>
                          <Button
                            type="button"
                            variant="ghost"
                            size="sm"
                            disabled={adding}
                            onClick={cancelDraft}
                          >
                            Cancel
                          </Button>
                        </div>
                      </td>
                    </tr>
                  ) : null}

                  {items.map((item) => {
                    const busy = busyId === item.id;
                    const editing = editingId === item.id;
                    return (
                      <tr
                        key={item.id}
                        className={item.isActive ? undefined : "is-inactive"}
                      >
                        <td>
                          {editing ? (
                            <input
                              type="text"
                              className="field-input"
                              value={editName}
                              disabled={busy}
                              onChange={(e) => setEditName(e.target.value)}
                              onKeyDown={(e) => {
                                if (e.key === "Enter") {
                                  e.preventDefault();
                                  void onSaveEdit(item);
                                }
                                if (e.key === "Escape") setEditingId(null);
                              }}
                            />
                          ) : (
                            <span className="admin-users-name">{item.name}</span>
                          )}
                        </td>
                        <td>
                          {editing ? (
                            <input
                              type="text"
                              className="field-input"
                              value={editDescription}
                              disabled={busy}
                              onChange={(e) =>
                                setEditDescription(e.target.value)
                              }
                              onKeyDown={(e) => {
                                if (e.key === "Enter") {
                                  e.preventDefault();
                                  void onSaveEdit(item);
                                }
                                if (e.key === "Escape") setEditingId(null);
                              }}
                            />
                          ) : (
                            <span className="admin-division-desc">
                              {item.description || "—"}
                            </span>
                          )}
                        </td>
                        <td>
                          {editing ? (
                            <div className="admin-value-status-select">
                              <FieldSelect
                                id={`status-${item.id}`}
                                name={`status-${item.id}`}
                                placeholder="Status"
                                options={STATUS_OPTIONS}
                                value={editActive ? "active" : "inactive"}
                                disabled={busy}
                                onValueChange={(value) =>
                                  setEditActive(value === "active")
                                }
                              />
                            </div>
                          ) : (
                            <span
                              className={`admin-value-status${item.isActive ? " is-active" : ""}`}
                            >
                              {item.isActive ? "Active" : "Inactive"}
                            </span>
                          )}
                        </td>
                        <td>
                          <div className="admin-value-actions">
                            {editing ? (
                              <>
                                <Button
                                  type="button"
                                  variant="primary"
                                  size="sm"
                                  disabled={busy || !editName.trim()}
                                  onClick={() => void onSaveEdit(item)}
                                >
                                  Save
                                </Button>
                                <Button
                                  type="button"
                                  variant="ghost"
                                  size="sm"
                                  disabled={busy}
                                  onClick={() => setEditingId(null)}
                                >
                                  Cancel
                                </Button>
                              </>
                            ) : (
                              <>
                                <Button
                                  type="button"
                                  variant="outline"
                                  size="sm"
                                  disabled={busy || draftOpen}
                                  onClick={() => {
                                    setDraftOpen(false);
                                    setEditingId(item.id);
                                    setEditName(item.name);
                                    setEditDescription(item.description);
                                    setEditActive(item.isActive);
                                  }}
                                >
                                  <IconPencil />
                                  Edit
                                </Button>
                                <Button
                                  type="button"
                                  variant="danger"
                                  size="sm"
                                  disabled={busy || draftOpen}
                                  onClick={() => setPendingDelete(item)}
                                >
                                  <IconTrash />
                                  Delete
                                </Button>
                              </>
                            )}
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>

            {data ? (
              <div className="admin-users-pager">
                <p className="admin-users-count">
                  Page {pageNumber} of {totalPages}
                  {loading ? " · Refreshing…" : ""}
                </p>
                <div className="admin-users-pager-actions">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    disabled={pageNumber <= 1 || loading}
                    onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                  >
                    Previous
                  </Button>
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    disabled={pageNumber >= totalPages || loading}
                    onClick={() =>
                      setPageNumber((p) => Math.min(totalPages, p + 1))
                    }
                  >
                    Next
                  </Button>
                </div>
              </div>
            ) : null}
          </>
        )}

        <ConfirmModal
          open={pendingDelete !== null}
          onClose={() => {
            if (busyId === null) setPendingDelete(null);
          }}
          onConfirm={onConfirmDelete}
          title="Delete division?"
          description={
            pendingDelete
              ? `Delete “${pendingDelete.name}”? This cannot be undone.`
              : undefined
          }
          confirmLabel="Delete"
          busyLabel="Deleting…"
          busy={pendingDelete !== null && busyId === pendingDelete.id}
        />
      </section>
    </div>
  );
}
