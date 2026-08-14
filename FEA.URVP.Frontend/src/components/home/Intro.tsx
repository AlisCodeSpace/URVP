import { Heading, Text } from "@radix-ui/themes";
import {
  introEyebrow,
  introHeadline,
  introKeyFacts,
  introParagraphs,
} from "@/lib/home-content";

export function Intro() {
  return (
    <section className="bg-background">
      <div className="site-container py-20 sm:py-28">
        <div className="grid items-start gap-12 lg:grid-cols-[minmax(0,1.45fr)_minmax(16rem,0.9fr)] lg:gap-16">
          <div>
            <Text
              as="p"
              size="2"
              weight="medium"
              className="!uppercase !tracking-[0.2em] !text-secondary-deep"
            >
              {introEyebrow}
            </Text>
            <Heading
              as="h2"
              size="7"
              weight="medium"
              mt="3"
              className="!font-[family-name:var(--font-display)] !leading-tight !text-primary"
            >
              {introHeadline}
            </Heading>
            <div className="mt-6 grid gap-4 text-muted">
              {introParagraphs.map((paragraph) => (
                <Text as="p" size="3" key={paragraph} className="!leading-relaxed">
                  {paragraph}
                </Text>
              ))}
            </div>
          </div>

          <aside className="rounded-lg border border-primary/12 bg-surface px-6 py-7">
            <Text
              as="p"
              size="2"
              weight="medium"
              className="!uppercase !tracking-[0.2em] !text-secondary-deep"
            >
              Key information
            </Text>
            <ul className="mt-5 grid gap-4">
              {introKeyFacts.map((fact) => (
                <li
                  key={fact}
                  className="border-l-2 border-secondary pl-4 text-sm leading-relaxed text-muted"
                >
                  {fact}
                </li>
              ))}
            </ul>
          </aside>
        </div>
      </div>
    </section>
  );
}
