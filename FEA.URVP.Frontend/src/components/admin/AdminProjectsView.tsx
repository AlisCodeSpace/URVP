"use client";

import { useCallback, useEffect, useId, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { FieldSelect } from "@/components/ui/FieldSelect";
import { ApiError } from "@/lib/api";
import {
  listAdminProjects,
  type AdminProjectListItemDto,
  type PaginatedAdminProjects,
} from "@/lib/admin-projects-api";
import { formatProjectDate, type MyProjectStatus } from "@/lib/project-form";

const PAGE_SIZE = 20;

const STATUS_FILTER_OPTIONS = [
  { value: "", label: "All statuses" },
  { value: "Open", label: "Open" },
  { value: "Matching", label: "Matching" },
  { value: "Closed", label: "Closed" },
] as const;

function statusClass(status: MyProjectStatus) {
  if (status === "Open") return "is-active";
  if (status === "Matching") return "is-matching";
  return "";
}

export function AdminProjectsView() {
  const searchId = useId();
  const statusFilterId = useId();

  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [pageNumber, setPageNumber] = useState(1);

  const [data, setData] = useState<PaginatedAdminProjects | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const page = await listAdminProjects({
        search,
        status: (statusFilter as MyProjectStatus | "") || undefined,
        pageNumber,
        pageSize: PAGE_SIZE,
      });
      setData(page);
    } catch (err) {
      setData(null);
      setError(
        err instanceof ApiError ? err.message : "Failed to load projects.",
      );
    } finally {
      setLoading(false);
    }
  }, [search, statusFilter, pageNumber]);

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

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title="Projects"
        description="All research listings and how many students have ranked each one."
        tag={
          data
            ? `${data.totalCount} project${data.totalCount === 1 ? "" : "s"}`
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
            placeholder="Title, faculty, affiliation, or email"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
        </div>
        <div className="admin-users-field admin-users-field--role">
          <label className="field-label" htmlFor={statusFilterId}>
            Status
          </label>
          <FieldSelect
            id={statusFilterId}
            name="statusFilter"
            placeholder="All statuses"
            options={STATUS_FILTER_OPTIONS}
            value={statusFilter}
            onValueChange={(value) => {
              setPageNumber(1);
              setStatusFilter(value);
            }}
          />
        </div>
      </div>

      {loading && !data ? (
        <p className="admin-users-status">Loading projects…</p>
      ) : error ? (
        <div className="admin-users-status">
          <p className="admin-users-banner is-error" role="alert">
            {error}
          </p>
          <Button type="button" variant="outline" size="sm" onClick={() => void load()}>
            Retry
          </Button>
        </div>
      ) : !data?.items.length ? (
        <p className="admin-users-status">No projects match these filters.</p>
      ) : (
        <>
          <div className="admin-users-table-wrap">
            <table className="admin-users-table">
              <thead>
                <tr>
                  <th scope="col">Project</th>
                  <th scope="col">Faculty</th>
                  <th scope="col">Status</th>
                  <th scope="col">Volunteers</th>
                  <th scope="col">Students ranked</th>
                  <th scope="col">Actions</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((project) => (
                  <ProjectRow key={project.id} project={project} />
                ))}
              </tbody>
            </table>
          </div>

          <div className="admin-users-pager">
            <p className="admin-users-count">
              {data.totalCount} project{data.totalCount === 1 ? "" : "s"}
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
              <span className="admin-users-page-label">
                Page {pageNumber} of {totalPages}
              </span>
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
        </>
      )}
    </div>
  );
}

function ProjectRow({ project }: { project: AdminProjectListItemDto }) {
  const ranked = project.rankingCount;

  return (
    <tr>
      <td>
        <div className="admin-users-name">{project.title}</div>
        <div className="admin-users-meta">
          Posted {formatProjectDate(project.createdAt)}
        </div>
      </td>
      <td>
        <div className="admin-users-name">{project.facultyName}</div>
        <div className="admin-users-meta">{project.affiliation}</div>
      </td>
      <td>
        <span className={`admin-value-status ${statusClass(project.status)}`}>
          {project.status}
        </span>
      </td>
      <td>
        {project.volunteersFilled}/{project.volunteersRequired}
      </td>
      <td>
        <span className={`admin-rank-count${ranked === 0 ? " is-zero" : ""}`}>
          {ranked}
        </span>
      </td>
      <td>
        <div className="admin-value-actions">
          <Button href={`/admin/projects/${project.id}`} variant="primary" size="sm">
            View
          </Button>
        </div>
      </td>
    </tr>
  );
}
