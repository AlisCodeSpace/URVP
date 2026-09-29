"use client";

import { useEffect, useId, useState } from "react";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { ExportChoiceModal } from "@/components/admin/ExportChoiceModal";
import { Button } from "@/components/ui/Button";
import { FieldSelect } from "@/components/ui/FieldSelect";
import { IconDownload } from "@/components/ui/Icons";
import { RefreshIconButton } from "@/components/ui/RefreshIconButton";
import { AdminTableSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import { adminProjectHref } from "@/lib/auth";
import {
  exportAdminProjects,
  listAdminProjects,
  type AdminProjectListItemDto,
} from "@/lib/admin-projects-api";
import {
  formatProjectDate,
  projectStatusClass,
  type MyProjectStatus,
} from "@/lib/project-form";

const PAGE_SIZE = 20;

const PROJECT_EXPORT_OPTIONS = [
  {
    id: "full",
    title: "Export Full Details",
    description:
      "Project title, description, research areas, activity types, volunteer seats, IRB stage, and qualifications, plus the faculty member who posted it. Students listed are those matched to the project, not students who only ranked it. A project with several matched students appears once per student.",
  },
] as const;

const STATUS_FILTER_OPTIONS = [
  { value: "", label: "All statuses" },
  { value: "Open", label: "Open" },
  { value: "Matching", label: "Matching" },
  { value: "Closed", label: "Closed" },
  { value: "Inactive", label: "Inactive" },
] as const;

export function AdminProjectsView() {
  const searchId = useId();
  const statusFilterId = useId();

  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [exportOpen, setExportOpen] = useState(false);
  const [exporting, setExporting] = useState(false);
  const [exportError, setExportError] = useState<string | null>(null);

  const {
    data,
    loading,
    error,
    reload: load,
  } = useCancellableQuery(
    () =>
      listAdminProjects({
        search,
        status: (statusFilter as MyProjectStatus | "") || undefined,
        pageNumber,
        pageSize: PAGE_SIZE,
      }),
    [search, statusFilter, pageNumber],
    { fallbackError: "Failed to load projects." },
  );

  useEffect(() => {
    const handle = window.setTimeout(() => {
      setPageNumber(1);
      setSearch(searchInput.trim());
    }, 300);
    return () => window.clearTimeout(handle);
  }, [searchInput]);

  function closeExport() {
    if (exporting) return;
    setExportOpen(false);
    setExportError(null);
  }

  async function onExport() {
    setExporting(true);
    setExportError(null);
    try {
      await exportAdminProjects({
        search,
        status: (statusFilter as MyProjectStatus | "") || undefined,
      });
      setExportOpen(false);
    } catch (err) {
      setExportError(
        err instanceof ApiError ? err.message : "Failed to export projects.",
      );
    } finally {
      setExporting(false);
    }
  }

  const totalPages = data
    ? Math.max(1, Math.ceil(data.totalCount / data.pageSize))
    : 1;

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title="Projects"
        description="Open a listing to review details, seats, and ranking interest. Excel export includes each project's details, the faculty who posted it, and the students matched to it."
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
        <div className="admin-users-export">
          <Button
            type="button"
            variant="outline"
            size="md"
            disabled={exporting}
            onClick={() => {
              setExportError(null);
              setExportOpen(true);
            }}
          >
            <IconDownload />
            {exporting ? "Exporting…" : "Export Excel"}
          </Button>
        </div>
        <div className="admin-users-refresh">
          <RefreshIconButton loading={loading} onClick={() => void load()} />
        </div>
      </div>

      {loading && !data ? (
        <AdminTableSkeleton columns={6} />
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
                  <th scope="col">Cycle</th>
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

      <ExportChoiceModal
        open={exportOpen}
        title="Export projects"
        description="The workbook follows the current search and status filters."
        options={PROJECT_EXPORT_OPTIONS}
        busyId={exporting ? "full" : null}
        error={exportError}
        onClose={closeExport}
        onSelect={() => void onExport()}
      />
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
      <td>{project.semesterName || "—"}</td>
      <td>
        <span
          className={`admin-value-status ${projectStatusClass(project.status)}`}
        >
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
          <Button href={adminProjectHref(project.id)} variant="primary" size="sm">
            View project
          </Button>
        </div>
      </td>
    </tr>
  );
}
