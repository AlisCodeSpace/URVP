"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Text } from "@radix-ui/themes";
import { newsItems, toNewsTickerItems, type NewsTickerItem } from "@/lib/home-content";
import { loadPublicNews } from "@/lib/news-api";

export function RollingBanners({ items }: { items?: NewsTickerItem[] }) {
  const [ticker, setTicker] = useState<NewsTickerItem[]>(items ?? newsItems);

  useEffect(() => {
    if (items) {
      setTicker(items);
      return;
    }
    let cancelled = false;
    void loadPublicNews().then((articles) => {
      if (!cancelled) setTicker(toNewsTickerItems(articles));
    });
    return () => {
      cancelled = true;
    };
  }, [items]);

  if (ticker.length === 0) return null;

  const news = [...ticker, ...ticker];

  return (
    <section className="overflow-hidden bg-background py-16 sm:py-20">
      <div className="site-container mb-10">
        <Text
          as="p"
          size="2"
          weight="medium"
          className="!uppercase !tracking-[0.2em] !text-secondary-deep"
        >
          Updates
        </Text>
      </div>

      <div className="relative border-y border-secondary/25 bg-secondary/8 py-5">
        <div
          className="marquee-track flex gap-12 px-6"
          style={{ animationDuration: "32s" }}
        >
          {news.map((item, i) => (
            <Link
              key={`${item.title}-${i}`}
              href={item.href}
              className="flex shrink-0 items-baseline gap-3 whitespace-nowrap transition hover:opacity-80"
            >
              <span className="text-xs font-semibold uppercase tracking-[0.18em] text-secondary-deep">
                {item.title}
              </span>
              <span className="text-sm text-muted">{item.detail}</span>
            </Link>
          ))}
        </div>
      </div>
    </section>
  );
}
