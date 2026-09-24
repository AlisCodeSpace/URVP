import { Heading, Text } from "@/components/ui/Typography";
import {
  researchDayDeadlines,
  researchDayIntro,
  researchDayUpdates,
} from "@/lib/research-day";

export function ResearchDayContent() {
  return (
    <>
      <section className="site-container py-16 sm:py-20">
        <Heading
          as="h2"
          size="7"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          About the day
        </Heading>
        <Text as="p" size="3" mt="2" className="max-w-xl !leading-relaxed !text-muted">
          {researchDayIntro}
        </Text>
        <Text as="p" size="3" mt="3" className="max-w-xl !leading-relaxed !text-muted">
          Content on this page will be updated as dates, the program, and
          registration details are confirmed for the semester.
        </Text>
      </section>

      <section
        id="research-day-deadlines"
        className="scroll-mt-24 border-y border-primary/10 bg-primary/8"
      >
        <div className="site-container py-16 sm:py-20">
          <Heading
            as="h2"
            size="7"
            weight="medium"
            className="!font-[family-name:var(--font-display)] !text-primary"
          >
            Deadlines
          </Heading>
          <Text as="p" size="3" mt="2" className="max-w-xl !text-muted">
            Key dates for poster submission, registration, program confirmation,
            and the Best Poster Competition. Exact deadlines will replace the
            placeholders below.
          </Text>

          <ol className="mt-12 divide-y divide-primary/10 border-y border-primary/10">
            {researchDayDeadlines.map((item, index) => (
              <li
                key={item.id}
                className="grid gap-4 py-8 md:grid-cols-[auto_1fr_auto] md:items-baseline md:gap-8"
              >
                <span
                  aria-hidden
                  className="font-[family-name:var(--font-display)] text-3xl font-medium text-secondary-deep"
                >
                  {String(index + 1).padStart(2, "0")}
                </span>
                <div>
                  <Heading
                    as="h3"
                    size="5"
                    weight="medium"
                    className="!font-[family-name:var(--font-display)] !text-primary"
                  >
                    {item.label}
                  </Heading>
                  <Text as="p" size="3" mt="2" className="!leading-relaxed !text-muted">
                    {item.detail}
                  </Text>
                </div>
                <Text
                  as="p"
                  size="2"
                  weight="medium"
                  className="shrink-0 !uppercase !tracking-[0.16em] !text-secondary-deep"
                >
                  {item.date}
                </Text>
              </li>
            ))}
          </ol>
        </div>
      </section>

      <section className="site-container py-16 sm:py-20">
        <Heading
          as="h2"
          size="7"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          Updates
        </Heading>
        <Text as="p" size="3" mt="2" className="max-w-xl !text-muted">
          Announcements and program news as Research Day takes shape.
        </Text>

        <ul className="mt-10 grid gap-6 lg:grid-cols-2">
          {researchDayUpdates.map((update) => (
            <li
              key={update.id}
              className="rounded-lg border border-primary/12 bg-surface px-6 py-7"
            >
              <Text
                as="p"
                size="1"
                weight="bold"
                className="!uppercase !tracking-[0.18em] !text-secondary-deep"
              >
                {update.date}
              </Text>
              <Heading
                as="h3"
                size="4"
                weight="medium"
                mt="3"
                className="!font-[family-name:var(--font-display)] !text-primary"
              >
                {update.title}
              </Heading>
              <Text as="p" size="3" mt="2" className="!leading-relaxed !text-muted">
                {update.body}
              </Text>
            </li>
          ))}
        </ul>
      </section>
    </>
  );
}
