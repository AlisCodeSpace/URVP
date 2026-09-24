"use client";

import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import {
  listValueListItems,
  type ValueListKindSlug,
} from "@/lib/value-lists-api";

/** Active catalog names from the value-list table, with a static fallback. */
export function useValueListOptions(
  kind: ValueListKindSlug,
  fallback: readonly string[],
): string[] {
  const query = useCancellableQuery(
    async () => {
      const page = await listValueListItems(kind, {
        pageNumber: 1,
        pageSize: 200,
        activeOnly: true,
      });
      const next = page.items.map((item) => item.name);
      return next.length > 0 ? next : [...fallback];
    },
    [kind, fallback],
    {
      initialData: [...fallback],
      initialLoading: false,
      dataOnError: () => [...fallback],
    },
  );

  return query.data ?? [...fallback];
}
