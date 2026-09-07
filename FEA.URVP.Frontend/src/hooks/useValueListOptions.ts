"use client";

import { useEffect, useState } from "react";
import {
  listValueListItems,
  type ValueListKindSlug,
} from "@/lib/value-lists-api";

/** Active catalog names from the value-list table, with a static fallback. */
export function useValueListOptions(
  kind: ValueListKindSlug,
  fallback: readonly string[],
): string[] {
  const [names, setNames] = useState<string[]>(() => [...fallback]);

  useEffect(() => {
    let cancelled = false;

    void (async () => {
      try {
        const page = await listValueListItems(kind, {
          pageNumber: 1,
          pageSize: 200,
          activeOnly: true,
        });
        if (cancelled) return;
        const next = page.items.map((item) => item.name);
        if (next.length > 0) setNames(next);
      } catch {
        if (!cancelled) setNames([...fallback]);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [kind, fallback]);

  return names;
}
