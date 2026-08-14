import { Heading, Text } from "@radix-ui/themes";
import { testimonials } from "@/lib/home-content";

export function Testimonials() {
  return (
    <section className="bg-surface">
      <div className="site-container py-20 sm:py-28">
        <Text
          as="p"
          size="2"
          weight="medium"
          className="!uppercase !tracking-[0.2em] !text-secondary-deep"
        >
          Student voices
        </Text>
        <Heading
          as="h2"
          size="7"
          weight="medium"
          mt="3"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          What students say.
        </Heading>

        <ul className="mt-12 grid gap-6 sm:grid-cols-2">
          {testimonials.map((item) => (
            <li
              key={item.name}
              className="flex flex-col rounded-lg border border-primary/12 bg-background px-6 py-7"
            >
              <blockquote className="flex flex-1 flex-col">
                <Text
                  as="p"
                  size="3"
                  className="!font-[family-name:var(--font-display)] !leading-relaxed !text-primary"
                >
                  “{item.quote}”
                </Text>
                <footer className="mt-5 border-t border-primary/10 pt-4 text-sm">
                  <span className="font-medium text-foreground">{item.name}</span>
                  <span className="text-muted"> — {item.role}</span>
                </footer>
              </blockquote>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
}
