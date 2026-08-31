import type { ReactNode } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { Container } from "@/components/layout/Container";

type PageHeaderProps = {
  title: string;
  description: string;
  eyebrow?: string;
  actions?: ReactNode;
  children?: ReactNode;
};

/** Compact dark header — navbar overlays this background on matching routes. */
export function PageHeader({
  title,
  description,
  eyebrow,
  actions,
  children,
}: PageHeaderProps) {
  return (
    <section className="page-header relative overflow-hidden text-white">
      <div className="page-header-grid absolute inset-0" aria-hidden />
      <Container className="relative z-10 pb-14 pt-28 sm:pb-16 sm:pt-32">
        {children}
        <div className={children ? "mt-5" : undefined}>
          {eyebrow ? (
            <Text
              as="p"
              size="2"
              weight="medium"
              className="!uppercase !tracking-[0.2em] !text-secondary"
            >
              {eyebrow}
            </Text>
          ) : null}
          <Heading
            as="h1"
            size="8"
            weight="medium"
            mt={eyebrow ? "3" : "0"}
            className="!font-[family-name:var(--font-display)] !text-white"
          >
            {title}
          </Heading>
          <Text
            as="p"
            size="4"
            mt="3"
            className="max-w-2xl xl:max-w-3xl !leading-relaxed !text-white/75"
          >
            {description}
          </Text>
          {actions ? (
            <div className="mt-6 flex flex-wrap gap-3">{actions}</div>
          ) : null}
        </div>
      </Container>
    </section>
  );
}
