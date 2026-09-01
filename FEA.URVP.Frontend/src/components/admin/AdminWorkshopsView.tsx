"use client";

import { useCallback, useEffect, useId, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { ApiError } from "@/lib/api";
import {
  deleteWorkshop,
  listWorkshops,
  type PaginatedWorkshops,
  type WorkshopDto,
} from "@/lib/workshops-api";

const PAGE_SIZE = 20;

export function AdminWorkshopsView() {
  const searchId = useId();
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [data, setData] = useState<PaginatedWorkshops | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pendingDelete, setPendingDelete] = useState<WorkshopDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(
        await listWorkshops({
          search,
          pageNumber,
          pageSize: PAGE_SIZE,
        }),
      );
    } catch (err) {
      setData(null);
      setError(
        err instanceof ApiError ? err.message : "Failed to load workshops.",
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

  const totalPages = data
    ? Math.max(1, Math.ceil(data.totalCount / data.pageSize))
    : 1;

  async function onConfirmDelete() {
    if (!pendingDelete) return;
    setDeleting(true);
    try {
      await deleteWorkshop(pendingDelete.id);
      setPendingDelete(null);
      await load();
    } catch (err) {
      setError(
        err instanceof ApiError ? err.message : "Failed to delete workshop.",
      );
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title="Workshops"
        description="Sessions shown on the Workshops page and home teaser. Upload a 3:2 photo for each card."
        tag={
          data
            ? `${data.totalCount} workshop${data.totalCount === 1 ? "" : "s"}`
            : null
        }
      />

      <div className="admin-users-filters">
        <div className="admin-users-field">
          <label className="field-label" htmlFor={searchId}>
            Search
          </label>
          <input
            id={searchId}
            type="search"
            className="field-input"
            placeholder="Title, description, or location"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
        </div>
        <div className="admin-users-field flex items-end">
          <Button href="/admin/workshops/new" variant="primary" size="sm">
            Add workshop
          </Button>
        </div>
      </div>

      {loading && !data ? (
        <p className="admin-users-status">Loading workshops…</p>
      ) : error ? (
        <p className="admin-users-banner is-error" role="alert">
          {error}
        </p>
      ) : !data || data.items.length === 0 ? (
        <p className="admin-users-status">No workshops yet.</p>
      ) : (
        <>
          <div className="admin-users-table-wrap">
            <table className="admin-users-table">
              <thead>
                <tr>
                  <th>Title</th>
                  <th>Date</th>
                  <th>Location</th>
                  <th>Photo</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td>
                      <span className="admin-users-name">{item.title}</span>
                    </td>
                    <td>
                      {item.date}
                      {item.time ? ` · ${item.time}` : ""}
                    </td>
                    <td>{item.location || "—"}</td>
                    <td>{item.posterFileId ? "Yes" : "—"}</td>
                    <td>
                      <div className="admin-value-actions">
                        <Button
                          href={`/admin/workshops/${item.id}`}
                          variant="outline"
                          size="sm"
                        >
                          Edit
                        </Button>
                        <Button
                          type="button"
                          variant="danger"
                          size="sm"
                          onClick={() => setPendingDelete(item)}
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

          {totalPages > 1 ? (
            <div className="admin-users-pager">
              <p className="admin-users-count">
                Page {pageNumber} of {totalPages}
              </p>
              <div className="admin-users-pager-actions">
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  disabled={pageNumber <= 1}
                  onClick={() => setPageNumber((p) => Math.max(1, p - 1))}
                >
                  Previous
                </Button>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  disabled={pageNumber >= totalPages}
                  onClick={() => setPageNumber((p) => p + 1)}
                >
                  Next
                </Button>
              </div>
            </div>
          ) : null}
        </>
      )}

      <ConfirmModal
        open={Boolean(pendingDelete)}
        onClose={() => setPendingDelete(null)}
        onConfirm={onConfirmDelete}
        title="Delete this workshop?"
        description={
          pendingDelete
            ? `“${pendingDelete.title}” will be removed from the Workshops page.`
            : undefined
        }
        confirmLabel="Delete"
        busy={deleting}
      />
    </div>
  );
}
