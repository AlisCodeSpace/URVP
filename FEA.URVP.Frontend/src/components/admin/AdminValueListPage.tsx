"use client";

import { useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { AdminValueListSection } from "@/components/admin/AdminValueListSection";
import type { ValueListKindSlug } from "@/lib/value-lists-api";

type AdminValueListPageProps = {
  kind: ValueListKindSlug;
  title: string;
  description: string;
};

/** Dedicated admin page for a single value-list catalog. */
export function AdminValueListPage({
  kind,
  title,
  description,
}: AdminValueListPageProps) {
  const [totalCount, setTotalCount] = useState<number | null>(null);

  return (
    <div className="admin-panel admin-panel--wide">
      <AdminPageHeader
        title={title}
        description={description}
        tag={
          totalCount == null ? null : `${totalCount} ${title}`
        }
      />
      <AdminValueListSection
        kind={kind}
        title={title}
        showHeader={false}
        onTotalCountChange={setTotalCount}
      />
    </div>
  );
}
