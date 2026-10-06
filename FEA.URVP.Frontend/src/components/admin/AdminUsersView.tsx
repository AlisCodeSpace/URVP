"use client";

import { useEffect, useId, useState } from "react";
import Link from "next/link";
import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { ExportChoiceModal } from "@/components/admin/ExportChoiceModal";
import { Button } from "@/components/ui/Button";
import { FieldSelect } from "@/components/ui/FieldSelect";
import { IconDownload } from "@/components/ui/Icons";
import { RefreshIconButton } from "@/components/ui/RefreshIconButton";
import { AdminTableSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import { adminUserStudentProfileHref } from "@/lib/auth";
import {
  assignUserRole,
  exportUsers,
  listUsers,
  USER_ROLE_OPTIONS,
  type PaginatedUsers,
  type SortDirection,
  type UserDto,
  type UserExportDetail,
  type UserRoleName,
  type UserSortField,
} from "@/lib/users-api";

const PAGE_SIZE = 20;

const FACULTY_WITH_PROJECTS = "FacultyWithProjects";

const roleFilterOptions = [
  { value: "", label: "All roles" },
  { value: "Student", label: "Student" },
  { value: "Faculty", label: "Faculty" },
  { value: FACULTY_WITH_PROJECTS, label: "Faculty with projects" },
  { value: "Admin", label: "Admin" },
] as const;

function usersFilter(roleFilter: string): {
  role?: UserRoleName;
  facultyWithProjects?: boolean;
} {
  if (roleFilter === FACULTY_WITH_PROJECTS) {
    return { role: "Faculty", facultyWithProjects: true };
  }

  if (roleFilter === "Student" || roleFilter === "Faculty" || roleFilter === "Admin") {
    return { role: roleFilter };
  }

  return {};
}

const BASIC_EXPORT_OPTION = {
  id: "basic",
  title: "Export Basic Details",
  description: "Name, email, and role.",
} as const;

const STUDENT_FULL_EXPORT_OPTION = {
  id: "full",
  title: "Export Full Details",
  description:
    "Name, email, role, the projects each student was matched to, and profile details: username, affiliation, contact, degree, faculty, major, languages, credits, cumulative average, research topics, publications, availability, and whether a transcript and CV were uploaded.",
} as const;

const FACULTY_FULL_EXPORT_OPTION = {
  id: "full",
  title: "Export Full Details",
  description:
    "Name, email, role, username, affiliation, and the projects each faculty member has posted.",
} as const;

const ADMIN_FULL_EXPORT_OPTION = {
  id: "full",
  title: "Export Full Details",
  description: "Name, email, role, username, and affiliation.",
} as const;

function exportOptions(roleFilter: string) {
  if (roleFilter === "Student") {
    return [BASIC_EXPORT_OPTION, STUDENT_FULL_EXPORT_OPTION];
  }

  if (roleFilter === "Faculty" || roleFilter === FACULTY_WITH_PROJECTS) {
    return [BASIC_EXPORT_OPTION, FACULTY_FULL_EXPORT_OPTION];
  }

  if (roleFilter === "Admin") {
    return [BASIC_EXPORT_OPTION, ADMIN_FULL_EXPORT_OPTION];
  }

  return [BASIC_EXPORT_OPTION];
}

function exportDescription(roleFilter: string): string {
  if (roleFilter === "Student") {
    return "The workbook follows the current search and sort. Student rows include only students who have completed their profile.";
  }

  if (roleFilter === FACULTY_WITH_PROJECTS) {
    return "The workbook follows the current search and sort. Only faculty who have posted a project are included.";
  }

  if (roleFilter === "") {
    return "The workbook follows the current search and sort. All roles exports name, email, and role.";
  }

  return "The workbook follows the current search and sort.";
}

const SORTABLE: { field: UserSortField; label: string }[] = [
  { field: "Name", label: "Name" },
  { field: "Email", label: "Email" },
  { field: "Role", label: "Role" },
];

export function AdminUsersView() {
  const searchId = useId();
  const roleFilterId = useId();

  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [roleFilter, setRoleFilter] = useState("");
  const [sortBy, setSortBy] = useState<UserSortField>("Name");
  const [sortDir, setSortDir] = useState<SortDirection>("Asc");
  const [pageNumber, setPageNumber] = useState(1);
  const [savingId, setSavingId] = useState<string | null>(null);
  const [rowError, setRowError] = useState<string | null>(null);
  const [draftRoles, setDraftRoles] = useState<Record<string, UserRoleName>>({});
  const [draftSource, setDraftSource] = useState<PaginatedUsers | null>(null);
  const [exportOpen, setExportOpen] = useState(false);
  const [exporting, setExporting] = useState<UserExportDetail | null>(null);
  const [exportError, setExportError] = useState<string | null>(null);
  const {
    data,
    loading,
    error,
    setData,
    reload: load,
  } = useCancellableQuery(
    () =>
      listUsers({
        search,
        ...usersFilter(roleFilter),
        sortBy,
        sortDir,
        pageNumber,
        pageSize: PAGE_SIZE,
      }),
    [search, roleFilter, sortBy, sortDir, pageNumber],
    { fallbackError: "Failed to load users." },
  );

  if (data !== draftSource) {
    setDraftSource(data);
    setDraftRoles(
      data
        ? Object.fromEntries(data.items.map((user) => [user.id, user.role]))
        : {},
    );
    setRowError(null);
  }

  useEffect(() => {
    const handle = window.setTimeout(() => {
      setPageNumber(1);
      setSearch(searchInput.trim());
    }, 300);
    return () => window.clearTimeout(handle);
  }, [searchInput]);

  function toggleSort(field: UserSortField) {
    setPageNumber(1);
    if (sortBy === field) {
      setSortDir((d) => (d === "Asc" ? "Desc" : "Asc"));
      return;
    }
    setSortBy(field);
    setSortDir("Asc");
  }

  async function onAssignRole(user: UserDto, role: UserRoleName) {
    if (role === user.role) return;

    setSavingId(user.id);
    setRowError(null);
    try {
      const updated = await assignUserRole(user.id, role);
      setData((prev) =>
        prev
          ? {
              ...prev,
              items: prev.items.map((item) =>
                item.id === updated.id ? updated : item,
              ),
            }
          : prev,
      );
      setDraftRoles((prev) => ({ ...prev, [updated.id]: updated.role }));
    } catch (err) {
      setDraftRoles((prev) => ({ ...prev, [user.id]: user.role }));
      setRowError(
        err instanceof ApiError
          ? err.message
          : "Failed to update role.",
      );
    } finally {
      setSavingId(null);
    }
  }

  function closeExport() {
    if (exporting) return;
    setExportOpen(false);
    setExportError(null);
  }

  async function onExport(detail: UserExportDetail) {
    setExporting(detail);
    setExportError(null);
    try {
      await exportUsers(detail, {
        search,
        ...usersFilter(roleFilter),
        sortBy,
        sortDir,
      });
      setExportOpen(false);
    } catch (err) {
      setExportError(
        err instanceof ApiError
          ? err.message
          : "Failed to export users.",
      );
    } finally {
      setExporting(null);
    }
  }

  const totalPages = data
    ? Math.max(1, Math.ceil(data.totalCount / data.pageSize))
    : 1;

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title="Users"
        description="View accounts and assign Student, Faculty, or Admin roles. Excel export follows the current search and role filters. All roles exports name, email, and role. A selected role can also export the details available for that role. Student exports include only students who have completed their profile. Faculty with projects includes only faculty who have posted a project."
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
            placeholder="Name, email, or username"
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
          />
        </div>
        <div className="admin-users-field admin-users-field--role">
          <label className="field-label" htmlFor={roleFilterId}>
            Role
          </label>
          <FieldSelect
            id={roleFilterId}
            name="roleFilter"
            placeholder="All roles"
            options={roleFilterOptions}
            value={roleFilter}
            onValueChange={(value) => {
              setPageNumber(1);
              setRoleFilter(value);
            }}
          />
        </div>
        <div className="admin-users-export">
          <Button
            type="button"
            variant="outline"
            size="md"
            disabled={exporting !== null}
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

      {rowError ? (
        <p className="admin-users-banner is-error" role="alert">
          {rowError}
        </p>
      ) : null}

      {loading && !data ? (
        <AdminTableSkeleton columns={5} />
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
        <p className="admin-users-status">No users match these filters.</p>
      ) : (
        <>
          <div className="admin-users-table-wrap">
            <table className="admin-users-table">
              <thead>
                <tr>
                  {SORTABLE.map((col) => {
                    const active = sortBy === col.field;
                    return (
                      <th key={col.field} scope="col">
                        <button
                          type="button"
                          className={`admin-sort-btn${active ? " is-active" : ""}`}
                          onClick={() => toggleSort(col.field)}
                          aria-sort={
                            active
                              ? sortDir === "Asc"
                                ? "ascending"
                                : "descending"
                              : "none"
                          }
                        >
                          <span>{col.label}</span>
                          <span className="admin-sort-indicator" aria-hidden>
                            {active ? (sortDir === "Asc" ? "↑" : "↓") : "↕"}
                          </span>
                        </button>
                      </th>
                    );
                  })}
                </tr>
              </thead>
              <tbody>
                {data.items.map((user) => {
                  const draft = draftRoles[user.id] ?? user.role;
                  const busy = savingId === user.id;
                  return (
                    <tr key={user.id}>
                      <td>
                        {user.role === "Student" ? (
                          <Link
                            href={adminUserStudentProfileHref(user.id)}
                            className="admin-users-name-link"
                          >
                            {user.name}
                          </Link>
                        ) : (
                          <div className="admin-users-name">{user.name}</div>
                        )}
                        {user.userName ? (
                          <div className="admin-users-meta">@{user.userName}</div>
                        ) : null}
                      </td>
                      <td>
                        <a
                          className="admin-users-email"
                          href={`mailto:${user.email}`}
                        >
                          {user.email}
                        </a>
                      </td>
                      <td>
                        <div className="admin-users-role-cell">
                          <FieldSelect
                            id={`role-${user.id}`}
                            name={`role-${user.id}`}
                            placeholder="Select role"
                            options={USER_ROLE_OPTIONS}
                            value={draft}
                            disabled={busy}
                            onValueChange={(value) => {
                              const next = value as UserRoleName;
                              setDraftRoles((prev) => ({
                                ...prev,
                                [user.id]: next,
                              }));
                              void onAssignRole(user, next);
                            }}
                          />
                          {busy ? (
                            <span className="admin-users-saving">Saving…</span>
                          ) : null}
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          <div className="admin-users-pager">
            <p className="admin-users-count">
              {data.totalCount} user{data.totalCount === 1 ? "" : "s"}
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
        title="Export users"
        description={exportDescription(roleFilter)}
        options={exportOptions(roleFilter)}
        busyId={exporting}
        error={exportError}
        onClose={closeExport}
        onSelect={(id) => void onExport(id as UserExportDetail)}
      />
    </div>
  );
}
