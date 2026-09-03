"use client";

import { useCallback, useEffect, useId, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { ConfirmModal } from "@/components/ui/ConfirmModal";
import { AdminTableSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import { adminNewsEditHref } from "@/lib/auth";
import {
  deleteNews,
  formatNewsDate,
  listNews,
  type NewsArticleDto,
  type PaginatedNews,
} from "@/lib/news-api";

const PAGE_SIZE = 20;

export function AdminNewsView() {
  const searchId = useId();
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [data, setData] = useState<PaginatedNews | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [pendingDelete, setPendingDelete] = useState<NewsArticleDto | null>(null);
  const [deleting, setDeleting] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(
        await listNews({
          search,
          pageNumber,
          pageSize: PAGE_SIZE,
        }),
      );
    } catch (err) {
      setData(null);
      setError(err instanceof ApiError ? err.message : "Failed to load news.");
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
      await deleteNews(pendingDelete.id);
      setPendingDelete(null);
      await load();
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "Failed to delete article.");
    } finally {
      setDeleting(false);
    }
  }

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title="News"
        description="Publish stories that appear on the News page and the home updates ticker."
        tag={
          data
            ? `${data.totalCount} stor${data.totalCount === 1 ? "y" : "ies"}`
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
            placeholder="Title, excerpt, category, or author"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
        </div>
        <div className="admin-users-field flex items-end">
          <Button href="/admin/news/new" variant="primary" size="sm">
            Add news
          </Button>
        </div>
      </div>

      {loading && !data ? (
        <AdminTableSkeleton columns={5} />
      ) : error ? (
        <p className="admin-users-banner is-error" role="alert">
          {error}
        </p>
      ) : !data || data.items.length === 0 ? (
        <p className="admin-users-status">No news articles yet.</p>
      ) : (
        <>
          <div className="admin-users-table-wrap">
            <table className="admin-users-table">
              <thead>
                <tr>
                  <th>Title</th>
                  <th>Category</th>
                  <th>Date</th>
                  <th>Author</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((item) => (
                  <tr key={item.id}>
                    <td>
                      <div>
                        <span className="admin-users-name">{item.title}</span>
                        {item.featured ? (
                          <div className="admin-users-meta">Featured</div>
                        ) : null}
                      </div>
                    </td>
                    <td>{item.category}</td>
                    <td>{formatNewsDate(item.publishedAt)}</td>
                    <td>{item.author}</td>
                    <td>
                      <div className="admin-value-actions">
                        <Button
                          href={adminNewsEditHref(item.id)}
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
        title="Delete this article?"
        description={
          pendingDelete
            ? `“${pendingDelete.title}” will be removed from the News page.`
            : undefined
        }
        confirmLabel="Delete"
        busy={deleting}
      />
    </div>
  );
}
