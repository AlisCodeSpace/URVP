import { Heading, Text } from "@radix-ui/themes";
import Link from "next/link";
import { portalLinks } from "@/lib/site";

export function FeatureNav() {
  return (
    <section className="border-y border-primary/10 bg-primary-deep text-white">
      <div className="site-container py-16 sm:py-20">
        <Heading
          as="h2"
          size="6"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-white"
        >
          Explore the Portal
        </Heading>
        <Text as="p" size="3" mt="2" className="max-w-2xl !text-white/70">
          Use the portal not only to apply, but also to stay informed about the
          latest research projects, workshops, and URVP Research Day updates.
        </Text>
        <nav
          aria-label="Main portal sections"
          className="mt-10 grid gap-3 sm:grid-cols-2 lg:grid-cols-3 2xl:grid-cols-6"
        >
          {portalLinks.map((link, i) => (
            <Link
              key={link.href}
              href={link.href}
              className="group flex items-end justify-between rounded-lg border border-white/15 bg-white/5 px-5 py-6 transition duration-300 hover:border-secondary hover:bg-secondary/15 focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-secondary"
            >
              <span className="font-[family-name:var(--font-display)] text-2xl font-medium tracking-tight">
                {link.label}
              </span>
              <span
                aria-hidden
                className="text-secondary transition-transform duration-300 group-hover:translate-x-1"
              >
                {String(i + 1).padStart(2, "0")}
              </span>
            </Link>
          ))}
        </nav>
      </div>
    </section>
  );
}
