"use client";

import { useCallback, useEffect, useId, useState } from "react";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { FieldSelect } from "@/components/ui/FieldSelect";
import { IconPencil, IconPlus, IconTrash } from "@/components/ui/Icons";
import { ApiError } from "@/lib/api";
import {
  createValueListItem,
  deleteValueListItem,
  listValueListItems,
  updateValueListItem,
  type PaginatedValueListItems,
  type ValueListItemDto,
  type ValueListKindSlug,
} from "@/lib/value-lists-api";

const PAGE_SIZE = 20;

const STATUS_OPTIONS = [
  { value: "active", label: "Active" },
  { value: "inactive", label: "Inactive" },
] as const;

type AdminValueListSectionProps = {
  kind: ValueListKindSlug;
  title: string;
  description?: string;
  /** When false, page-level header is used instead. */
  showHeader?: boolean;
  onTotalCountChange?: (totalCount: number) => void;
};

export function AdminValueListSection({
  kind,
  title,
  description,
  showHeader = true,
  onTotalCountChange,
}: AdminValueListSectionProps) {
  const searchId = useId();
  const draftNameId = useId();
  const draftStatusId = useId();

  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [data, setData] = useState<PaginatedValueListItems | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);

  const [draftOpen, setDraftOpen] = useState(false);
  const [draftName, setDraftName] = useState("");
  const [draftActive, setDraftActive] = useState(true);
  const [adding, setAdding] = useState(false);

  const [busyId, setBusyId] = useState<string | null>(null);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editName, setEditName] = useState("");
  const [editActive, setEditActive] = useState(true);
  const [pendingDelete, setPendingDelete] = useState<ValueListItemDto | null>(
    null,
  );

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const page = await listValueListItems(kind, {
        search,
        pageNumber,
        pageSize: PAGE_SIZE,
      });
      setData(page);
      onTotalCountChange?.(page.totalCount);
    } catch (err) {
      setData(null);
      setError(
        err instanceof ApiError ? err.message : `Failed to load ${title}.`,
      );
    } finally {
      setLoading(false);
    }
  }, [kind, search, pageNumber, title, onTotalCountChange]);

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
    setDraftActive(true);
    setMessage(null);
    setError(null);
  }

  function cancelDraft() {
    setDraftOpen(false);
    setDraftName("");
    setDraftActive(true);
  }

  async function onSaveDraft() {
    const name = draftName.trim();
    if (!name) return;

    setAdding(true);
    setMessage(null);
    setError(null);
    try {
      await createValueListItem(kind, { name, isActive: draftActive });
      cancelDraft();
      setMessage(`Added “${name}”.`);
      if (pageNumber === 1) {
        await load();
      } else {
        setPageNumber(1);
      }
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to add value.");
    } finally {
      setAdding(false);
    }
  }

  async function onSaveEdit(item: ValueListItemDto) {
    const name = editName.trim();
    if (!name) return;

    setBusyId(item.id);
    setMessage(null);
    setError(null);
    try {
      await updateValueListItem(kind, item.id, {
        name,
        isActive: editActive,
      });
      setEditingId(null);
      setMessage(`Updated “${name}”.`);
      await load();
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Failed to update value.",
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
      await deleteValueListItem(kind, item.id);
      setPendingDelete(null);
      setMessage(`Deleted “${item.name}”.`);
      await load();
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Failed to delete value.",
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

  return (
    <section
      className="admin-value-section"
      aria-labelledby={showHeader ? `${kind}-heading` : undefined}
      aria-label={showHeader ? undefined : title}
    >
      {showHeader ? (
        <header className="admin-value-section-head">
          <div>
            <h3 id={`${kind}-heading`} className="admin-value-section-title">
              {title}
            </h3>
            {description ? (
              <p className="admin-value-section-desc">{description}</p>
            ) : null}
          </div>
          <p className="admin-value-section-count">
            {data
              ? `${data.totalCount} value${data.totalCount === 1 ? "" : "s"}`
              : "—"}
          </p>
        </header>
      ) : null}

      <div className="admin-value-toolbar">
        <div className="admin-value-field">
          <label className="field-label" htmlFor={searchId}>
            Search
          </label>
          <input
            id={searchId}
            type="search"
            className="field-input"
            placeholder={`Filter ${title.toLowerCase()}`}
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
            Add value
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
        <p className="admin-users-status">No values yet.</p>
      ) : (
        <>
          <div className="admin-users-table-wrap">
            <table className="admin-users-table">
              <thead>
                <tr>
                  <th scope="col">Name</th>
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
                        placeholder="Value name"
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
                      <div className="admin-value-status-select">
                        <FieldSelect
                          id={draftStatusId}
                          name={`${kind}-draft-status`}
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
        title="Delete value?"
        description={
          pendingDelete
            ? `Delete “${pendingDelete.name}” from ${title}? This cannot be undone.`
            : undefined
        }
        confirmLabel="Delete"
        busyLabel="Deleting…"
        busy={pendingDelete !== null && busyId === pendingDelete.id}
      />
    </section>
  );
}
