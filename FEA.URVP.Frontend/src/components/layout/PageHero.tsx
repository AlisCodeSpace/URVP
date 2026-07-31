import type { ReactNode } from "react";
import { Flex, Heading, Text } from "@radix-ui/themes";

type PageHeroProps = {
  /** Large display title — brand-level on Home, page titles elsewhere */
  title: string;
  /** Primary headline under the display title */
  headline: string;
  /** Short supporting sentence */
  description: string;
  /** Optional CTAs (same slot as Home Apply / Browse) */
  actions?: ReactNode;
  /**
   * `brand` — Home-scale display for the full program name.
   * `page` — slightly tighter for multi-word page titles.
   */
  titleScale?: "brand" | "page";
};

const titleScaleClass = {
  brand:
    "max-w-5xl text-[clamp(2.5rem,7.5vw,4.75rem)] !leading-[0.95]",
  page:
    "text-[clamp(3.25rem,12vw,7.5rem)]",
} as const;

export function PageHero({
  title,
  headline,
  description,
  actions,
  titleScale = "page",
}: PageHeroProps) {
  return (
    <section className="hero-plane relative min-h-[100svh] overflow-hidden text-white">
      <div className="hero-grid absolute inset-0" aria-hidden />
      <div className="relative z-10 mx-auto flex min-h-[100svh] max-w-6xl flex-col justify-end px-6 pb-16 pt-24 sm:justify-center sm:pb-24 sm:pt-28">
        <p
          className={`animate-fade-up font-[family-name:var(--font-display)] font-semibold leading-[0.85] tracking-tight text-white ${titleScaleClass[titleScale]}`}
        >
          {title}
        </p>
        <Heading
          as="h1"
          size="7"
          weight="medium"
          mt="5"
          className="animate-fade-up-delay max-w-xl !font-[family-name:var(--font-display)] !leading-tight !text-white"
        >
          {headline}
        </Heading>
        <Text
          as="p"
          size="4"
          mt="3"
          className="animate-fade-up-delay max-w-lg !leading-relaxed !text-white/80"
        >
          {description}
        </Text>
        {actions ? (
          <Flex gap="3" wrap="wrap" mt="6" className="animate-fade-up-delay-2">
            {actions}
          </Flex>
        ) : null}
      </div>
      <div
        className="pointer-events-none absolute -right-20 bottom-0 h-[55%] w-[55%] bg-[radial-gradient(circle_at_center,rgba(235,159,0,0.12),transparent_65%)]"
        aria-hidden
      />
    </section>
  );
}
