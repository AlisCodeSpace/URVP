import { Heading, Text } from "@radix-ui/themes";
import Link from "next/link";
import { Button } from "@/components/ui/Button";
import { workshops } from "@/lib/home-content";

export function Workshops() {
  return (
    <section className="border-y border-primary/10 bg-primary-deep text-white">
      <div className="mx-auto max-w-6xl px-6 py-20 sm:py-28">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
          <div>
            <Text
              as="p"
              size="2"
              weight="medium"
              className="!uppercase !tracking-[0.2em] !text-secondary"
            >
              Workshops
            </Text>
            <Heading
              as="h2"
              size="8"
              weight="medium"
              mt="2"
              className="!font-[family-name:var(--font-display)] !text-white"
            >
              Prepare to match.
            </Heading>
            <Text as="p" size="3" mt="2" className="max-w-lg !text-white/70">
              Short sessions to help you build a strong profile and thrive once
              you join a research team.
            </Text>
          </div>
          <Button href="/workshops" variant="outline-light" size="md">
            All workshops
          </Button>
        </div>

        <ul className="mt-12 divide-y divide-white/15 border-y border-white/15">
          {workshops.map((workshop) => (
            <li key={workshop.title}>
              <Link
                href={workshop.href}
                className="group flex flex-col gap-2 py-7 transition-colors sm:flex-row sm:items-baseline sm:justify-between sm:gap-8 hover:bg-white/[0.04]"
              >
                <div>
                  <Text
                    as="p"
                    size="5"
                    weight="medium"
                    className="!font-[family-name:var(--font-display)] !text-white group-hover:!text-secondary"
                  >
                    {workshop.title}
                  </Text>
                  <Text as="p" size="3" mt="1" className="!text-white/65">
                    {workshop.blurb}
                  </Text>
                </div>
                <Text
                  as="p"
                  size="2"
                  weight="medium"
                  className="shrink-0 !uppercase !tracking-wider !text-secondary"
                >
                  {workshop.date}
                </Text>
              </Link>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}
