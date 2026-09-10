"use client";

import { useEffect, useState } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { FeaturedNowSkeleton } from "@/components/ui/SectionSkeletons";
import { loadFeaturedNow } from "@/lib/featured-now";
import { featuredItems, type FeaturedItem } from "@/lib/home-content";

export function Featured({ items }: { items?: FeaturedItem[] }) {
  const [list, setList] = useState<FeaturedItem[] | null>(items ?? null);

  useEffect(() => {
    if (items) {
      setList(items);
      return;
    }
    let cancelled = false;
    void loadFeaturedNow()
      .then((next) => {
        if (!cancelled) setList(next);
      })
      .catch(() => {
        if (!cancelled) setList(featuredItems);
      });
    return () => {
      cancelled = true;
    };
  }, [items]);

  return (
    <section className="border-t border-primary/10 bg-background">
      <div className="site-container py-20 sm:py-24">
        <Heading
          as="h2"
          size="7"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          Featured now
        </Heading>
        <Text as="p" size="3" mt="2" className="max-w-xl !text-muted">
          Deadlines, cycles, and upcoming workshops — stay ahead of the matching
          window.
        </Text>

        {list == null ? (
          <FeaturedNowSkeleton />
        ) : (
          <ul className="mt-12 grid gap-6 sm:grid-cols-2 xl:grid-cols-3">
            {list.map((item) => (
              <li
                key={`${item.kind}-${item.title}`}
                className="rounded-lg border border-primary/12 bg-surface px-6 py-7 transition hover:border-secondary/60"
              >
                <Text
                  as="p"
                  size="1"
                  weight="bold"
                  className={
                    item.accent === "secondary"
                      ? "!uppercase !tracking-[0.2em] !text-secondary-deep"
                      : "!uppercase !tracking-[0.2em] !text-primary/55"
                  }
                >
                  {item.kind}
                </Text>
                <Heading
                  as="h3"
                  size="4"
                  mt="3"
                  className="!font-[family-name:var(--font-display)] !text-primary"
                >
                  {item.title}
                </Heading>
                <Text as="p" size="3" mt="2" className="!text-muted">
                  {item.detail}
                </Text>
              </li>
            ))}
          </ul>
        )}
      </div>
    </section>
  );
}
