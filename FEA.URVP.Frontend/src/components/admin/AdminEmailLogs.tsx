"use client";

import { useCallback, useEffect, useState } from "react";
import { Button } from "@/components/ui/Button";
import { RefreshIconButton } from "@/components/ui/RefreshIconButton";
import { AdminTableSkeleton } from "@/components/ui/SectionSkeletons";
import { Tag } from "@/components/ui/Tag";
import { ApiError } from "@/lib/api";
import { formatAppDateTime } from "@/lib/datetime";
import {
  listEmailLogs,
  type EmailLogDto,
  type PaginatedEmailLogs,
} from "@/lib/email-settings-api";

const PAGE_SIZE = 50;

function dash(value: string | null | undefined) {
  const trimmed = value?.trim();
  return trimmed ? trimmed : "—";
}

export function AdminEmailLogs() {
  const [pageNumber, setPageNumber] = useState(1);
  const [data, setData] = useState<PaginatedEmailLogs | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      setData(await listEmailLogs({ pageNumber, pageSize: PAGE_SIZE }));
    } catch (err) {
      setData(null);
      setError(
        err instanceof ApiError ? err.message : "Failed to load email logs.",
      );
    } finally {
      setLoading(false);
    }
  }, [pageNumber]);

  useEffect(() => {
    void load();
  }, [load]);

  const totalPages = data
    ? Math.max(1, Math.ceil(data.totalCount / data.pageSize))
    : 1;

  return (
    <section className="admin-email-logs" aria-labelledby="admin-email-logs-heading">
      <div className="admin-email-logs-header">
        <div>
          <h3 id="admin-email-logs-heading" className="admin-email-logs-title">
            Email logs
          </h3>
          <p className="admin-email-logs-desc">
            Every outbound message recorded by the mailer, newest first.
          </p>
        </div>
        <RefreshIconButton loading={loading} onClick={() => void load()} />
      </div>

      {loading && !data ? (
        <AdminTableSkeleton columns={7} rows={4} />
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
        <p className="admin-users-status">No email logs yet.</p>
      ) : (
        <>
          <div className="admin-users-table-wrap">
            <table className="admin-users-table admin-email-logs-table">
              <thead>
                <tr>
                  <th>Sent</th>
                  <th>Status</th>
                  <th>From</th>
                  <th>To</th>
                  <th>Cc</th>
                  <th>Bcc</th>
                  <th>Error</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((log) => (
                  <EmailLogRow key={log.id} log={log} />
                ))}
              </tbody>
            </table>
          </div>

          <div className="admin-users-pager">
            <p className="admin-users-count">
              {data.totalCount} log{data.totalCount === 1 ? "" : "s"}
              {totalPages > 1 ? ` · Page ${pageNumber} of ${totalPages}` : null}
            </p>
            {totalPages > 1 ? (
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
            ) : null}
          </div>
        </>
      )}
    </section>
  );
}

function EmailLogRow({ log }: { log: EmailLogDto }) {
  return (
    <tr>
      <td>{formatAppDateTime(log.createdOn)}</td>
      <td>
        <Tag tone={log.success ? "secondary" : "muted"}>
          {log.success ? "Sent" : "Failed"}
        </Tag>
      </td>
      <td className="admin-email-log-address">{dash(log.from)}</td>
      <td className="admin-email-log-address">{dash(log.to)}</td>
      <td className="admin-email-log-address">{dash(log.cc)}</td>
      <td className="admin-email-log-address">{dash(log.bcc)}</td>
      <td>
        {log.exception ? (
          <pre className="admin-email-log-pre is-error">{log.exception}</pre>
        ) : (
          "—"
        )}
      </td>
    </tr>
  );
}
