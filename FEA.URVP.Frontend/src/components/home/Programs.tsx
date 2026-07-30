import { Heading, Text } from "@radix-ui/themes";

export function Programs() {
  return (
    <section className="bg-surface">
      <div className="mx-auto max-w-6xl px-6 py-20 sm:py-28">
        <Heading
          as="h2"
          size="7"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          One program. Your research path.
        </Heading>
        <Text as="p" size="3" mt="3" className="max-w-2xl !text-muted">
          Students from all undergraduate majors who have completed at least 24
          sophomore credits and hold a GPA of 3.0+ can apply.
        </Text>

        <article className="mt-12 max-w-2xl">
          <Text
            as="p"
            size="2"
            weight="medium"
            className="!uppercase !tracking-[0.18em] !text-secondary-deep"
          >
            URVP
          </Text>
          <Heading
            as="h3"
            size="5"
            mt="3"
            className="!font-[family-name:var(--font-display)]"
          >
            Across AUB faculties
          </Heading>
          <Text as="p" size="3" mt="2" className="!leading-relaxed !text-muted">
            Research projects from across AUB faculties and disciplines. Cycle:
            Monday, October 13, 2025 – Friday, August 21, 2026.
          </Text>
        </article>

        <ul className="mt-14 grid gap-4 text-sm text-muted sm:grid-cols-2">
          <li className="border-l-2 border-secondary pl-4">
            Profiles open Aug 25 – Sep 30 for create/update.
          </li>
          <li className="border-l-2 border-secondary pl-4">
            Expect at least 8 hours/week for a minimum of 6 months.
          </li>
          <li className="border-l-2 border-secondary pl-4">
            Matching is not guaranteed; incomplete profiles are not matched.
          </li>
          <li className="border-l-2 border-secondary pl-4">
            Apply again in future cycles if unmatched this year.
          </li>
        </ul>
      </div>
    </section>
  );
}
