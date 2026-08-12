import { Heading, Text } from "@radix-ui/themes";
import { featuredItems } from "@/lib/home-content";

export function Featured() {
  return (
    <section className="border-t border-primary/10 bg-background">
      <div className="site-container py-20 sm:py-24">
        <Heading
          as="h2"
          size="7"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          Featured now
        </Heading>
        <Text as="p" size="3" mt="2" className="max-w-xl !text-muted">
          Deadlines, cycles, and upcoming workshops — stay ahead of the matching
          window.
        </Text>

        <ul className="mt-12 grid gap-6 sm:grid-cols-2 xl:grid-cols-3">
          {featuredItems.map((item) => (
            <li
              key={item.title}
              className="rounded-lg border border-primary/12 bg-surface px-6 py-7 transition hover:border-secondary/60"
            >
              <Text
                as="p"
                size="1"
                weight="bold"
                className={
                  item.accent === "secondary"
                    ? "!uppercase !tracking-[0.2em] !text-secondary-deep"
                    : "!uppercase !tracking-[0.2em] !text-primary/55"
                }
              >
                {item.kind}
              </Text>
              <Heading
                as="h3"
                size="4"
                mt="3"
                className="!font-[family-name:var(--font-display)] !text-primary"
              >
                {item.title}
              </Heading>
              <Text as="p" size="3" mt="2" className="!text-muted">
                {item.detail}
              </Text>
            </li>
          ))}
        </ul>

        <div className="mt-14 border-t border-primary/10 pt-8 text-sm text-muted">
          <p>
            Questions:{" "}
            <a
              href="mailto:jc14@aub.edu.lb"
              className="font-medium text-primary hover:underline"
            >
              Prof. Joseph Costantine
            </a>
          </p>
        </div>
      </div>
    </section>
  );
}
