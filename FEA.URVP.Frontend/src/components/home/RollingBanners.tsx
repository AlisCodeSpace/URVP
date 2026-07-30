"use client";

import Link from "next/link";
import { Text } from "@radix-ui/themes";
import { newsItems, testimonials } from "@/lib/home-content";

export function RollingBanners() {
  const quotes = [...testimonials, ...testimonials];
  const news = [...newsItems, ...newsItems];

  return (
    <section className="overflow-hidden bg-background py-16 sm:py-20">
      <div className="mb-10 px-6">
        <Text
          as="p"
          size="2"
          weight="medium"
          className="mx-auto max-w-6xl !uppercase !tracking-[0.2em] !text-secondary-deep"
        >
          Voices &amp; updates
        </Text>
      </div>

      <div className="relative mb-8 border-y border-primary/10 py-8">
        <div className="marquee-track flex gap-10 px-6">
          {quotes.map((item, i) => (
            <blockquote
              key={`${item.name}-${i}`}
              className="w-[min(28rem,80vw)] shrink-0"
            >
              <Text
                as="p"
                size="4"
                className="!font-[family-name:var(--font-display)] !leading-snug !text-primary"
              >
                “{item.quote}”
              </Text>
              <footer className="mt-3 text-sm text-muted">
                <span className="font-medium text-foreground">{item.name}</span>
                {" · "}
                {item.role}
              </footer>
            </blockquote>
          ))}
        </div>
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
