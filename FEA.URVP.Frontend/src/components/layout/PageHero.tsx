import type { ReactNode } from "react";
import { Flex, Heading, Text } from "@/components/ui/Typography";
import { Container } from "@/components/layout/Container";

type PageHeroProps = {
  /** Large display title — brand-level on Home, page titles elsewhere */
  title: string;
  /** Primary headline under the display title */
  headline: string;
  /** Short supporting sentence */
  description?: ReactNode;
  /** Cycle or deadline callout, shown above the supporting sentence */
  announcement?: ReactNode;
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
    "max-w-5xl xl:max-w-6xl 2xl:max-w-7xl text-[clamp(1.85rem,5.2vw+0.35rem,4.25rem)] !leading-[0.95]",
  page:
    "text-[clamp(2rem,6vw+0.5rem,6.25rem)]",
} as const;

export function PageHero({
  title,
  headline,
  description,
  announcement,
  actions,
  titleScale = "page",
}: PageHeroProps) {
  return (
    <section className="hero-plane relative min-h-[100svh] overflow-hidden text-primary">
      <div className="hero-grid absolute inset-0" aria-hidden />
      <Container className="relative z-10 flex min-h-[100svh] flex-col justify-end pb-16 pt-[calc(6rem+env(safe-area-inset-top,0px))] sm:justify-center sm:pb-24 sm:pt-[calc(7rem+env(safe-area-inset-top,0px))]">
        <p
          className={`animate-fade-up font-[family-name:var(--font-display)] font-semibold uppercase leading-[0.9] tracking-[0.02em] text-primary ${titleScaleClass[titleScale]}`}
        >
          {title}
        </p>
        <Heading
          as="h1"
          size="7"
          weight="medium"
          mt="5"
          className="animate-fade-up-delay whitespace-nowrap !font-[family-name:var(--font-display)] !text-[min(1.65rem,2.5vw)] !leading-tight !text-primary"
        >
          {headline}
        </Heading>
        {announcement ? (
          <Text
            as="p"
            size="4"
            mt="4"
            weight="medium"
            className="animate-fade-up-delay max-w-xl xl:max-w-2xl !leading-relaxed !text-secondary"
          >
            {announcement}
          </Text>
        ) : null}
        {description ? (
          <Text
            as="p"
            size="4"
            mt="3"
            className="animate-fade-up-delay max-w-lg xl:max-w-xl !leading-relaxed !text-primary/80"
          >
            {description}
          </Text>
        ) : null}
        {actions ? (
          <Flex gap="3" wrap="wrap" mt="6" className="animate-fade-up-delay-2">
            {actions}
          </Flex>
        ) : null}
      </Container>
    </section>
  );
}
