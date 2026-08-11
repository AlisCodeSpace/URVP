"use client";

import { useCallback, useEffect, useId, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { Button } from "@/components/ui/Button";
import { FieldSelect } from "@/components/ui/FieldSelect";
import { ApiError } from "@/lib/api";
import {
  assignUserRole,
  listUsers,
  USER_ROLE_OPTIONS,
  type PaginatedUsers,
  type SortDirection,
  type UserDto,
  type UserRoleName,
  type UserSortField,
} from "@/lib/users-api";

const PAGE_SIZE = 20;

const roleFilterOptions = [
  { value: "", label: "All roles" },
  ...USER_ROLE_OPTIONS,
] as const;

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

  const [data, setData] = useState<PaginatedUsers | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [savingId, setSavingId] = useState<string | null>(null);
  const [rowError, setRowError] = useState<string | null>(null);
  const [draftRoles, setDraftRoles] = useState<Record<string, UserRoleName>>({});

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const page = await listUsers({
        search,
        role: (roleFilter as UserRoleName | "") || undefined,
        sortBy,
        sortDir,
        pageNumber,
        pageSize: PAGE_SIZE,
      });
      setData(page);
      setDraftRoles(
        Object.fromEntries(page.items.map((u) => [u.id, u.role])),
      );
      setRowError(null);
    } catch (err) {
      setData(null);
      setError(
        err instanceof ApiError ? err.message : "Failed to load users.",
      );
    } finally {
      setLoading(false);
    }
  }, [search, roleFilter, sortBy, sortDir, pageNumber]);

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

  const totalPages = data
    ? Math.max(1, Math.ceil(data.totalCount / data.pageSize))
    : 1;

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title="Users"
        description="View accounts and assign Student, Faculty, or Admin roles."
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
      </div>

      {rowError ? (
        <p className="admin-users-banner is-error" role="alert">
          {rowError}
        </p>
      ) : null}

      {loading && !data ? (
        <p className="admin-users-status">Loading users…</p>
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
                        <div className="admin-users-name">{user.name}</div>
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
    </div>
  );
}
