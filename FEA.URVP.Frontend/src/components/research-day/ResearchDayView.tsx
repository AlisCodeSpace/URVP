import { Heading, Text } from "@radix-ui/themes";
import {
  researchDayDeadlines,
  researchDayForms,
  researchDayIntro,
  researchDayUpdates,
  showResearchDayForms,
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
        className="scroll-mt-24 border-y border-primary/10 bg-primary-deep text-white"
      >
        <div className="site-container py-16 sm:py-20">
          <Heading
            as="h2"
            size="7"
            weight="medium"
            className="!font-[family-name:var(--font-display)] !text-white"
          >
            Deadlines
          </Heading>
          <Text as="p" size="3" mt="2" className="max-w-xl !text-white/70">
            Key dates for poster submission, registration, program confirmation,
            and the Best Poster Competition. Exact deadlines will replace the
            placeholders below.
          </Text>

          <ol className="mt-12 divide-y divide-white/15 border-y border-white/15">
            {researchDayDeadlines.map((item, index) => (
              <li
                key={item.id}
                className="grid gap-4 py-8 md:grid-cols-[auto_1fr_auto] md:items-baseline md:gap-8"
              >
                <span
                  aria-hidden
                  className="font-[family-name:var(--font-display)] text-3xl font-medium text-secondary"
                >
                  {String(index + 1).padStart(2, "0")}
                </span>
                <div>
                  <Heading
                    as="h3"
                    size="5"
                    weight="medium"
                    className="!font-[family-name:var(--font-display)] !text-white"
                  >
                    {item.label}
                  </Heading>
                  <Text as="p" size="3" mt="2" className="!leading-relaxed !text-white/70">
                    {item.detail}
                  </Text>
                </div>
                <Text
                  as="p"
                  size="2"
                  weight="medium"
                  className="shrink-0 !uppercase !tracking-[0.16em] !text-secondary"
                >
                  {item.date}
                </Text>
              </li>
            ))}
          </ol>
        </div>
      </section>

      {showResearchDayForms ? (
      <section
        id="research-day-forms"
        className="scroll-mt-24 border-t border-primary/10 bg-primary-deep text-white"
      >
        <div className="site-container py-16 sm:py-20">
          <Heading
            as="h2"
            size="7"
            weight="medium"
            className="!font-[family-name:var(--font-display)] !text-white"
          >
            Forms &amp; applications
          </Heading>
          <Text as="p" size="3" mt="2" className="max-w-xl !text-white/70">
            Use the Google Forms below to apply, register, or request updates.
            Links will be replaced with the official forms when they open.
          </Text>

          <ul className="mt-12 grid gap-5 lg:grid-cols-3">
            {researchDayForms.map((form) => (
              <li
                key={form.id}
                className="flex flex-col rounded-lg border border-white/15 bg-white/5 px-6 py-7 transition hover:border-secondary/60"
              >
                <Heading
                  as="h3"
                  size="4"
                  weight="medium"
                  className="!font-[family-name:var(--font-display)] !text-white"
                >
                  {form.title}
                </Heading>
                <Text
                  as="p"
                  size="3"
                  mt="3"
                  className="flex-1 !leading-relaxed !text-white/70"
                >
                  {form.description}
                </Text>
                <a
                  href={form.href}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="btn btn-secondary btn-md mt-6 w-full"
                >
                  {form.cta}
                </a>
              </li>
            ))}
          </ul>
        </div>
      </section>
      ) : null}

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
