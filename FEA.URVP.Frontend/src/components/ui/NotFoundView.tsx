import Link from "next/link";
import { Heading, Text } from "@radix-ui/themes";
import { PageHeader } from "@/components/layout/PageHeader";

type NotFoundViewProps = {
  title?: string;
  description?: string;
  homeHref?: string;
};

const destinations = [
  {
    href: "/",
    label: "Home",
    hint: "Program overview and current cycle",
    featured: true,
  },
  {
    href: "/projects",
    label: "Projects",
    hint: "Faculty research opportunities",
    featured: false,
  },
  {
    href: "/news",
    label: "News",
    hint: "Deadlines, workshops, and stories",
    featured: false,
  },
  {
    href: "/contact",
    label: "Contact",
    hint: "Student Success Unit",
    featured: false,
  },
] as const;

export function NotFoundView({
  title = "Page not found",
  description = "This address is not part of the Undergraduate Research Volunteer Program site. Check the URL, or continue from one of the pages below.",
  homeHref = "/",
}: NotFoundViewProps) {
  return (
    <main className="flex-1 bg-background">
      <PageHeader title={title} description={description}>
        <p className="not-found-code" aria-hidden>
          404
        </p>
      </PageHeader>

      <section className="site-container py-14 sm:py-16">
        <Heading
          as="h2"
          size="6"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          Where to go next
        </Heading>
        <Text as="p" size="3" mt="2" className="max-w-xl !text-muted">
          Browse open research, catch up on program news, or return to the
          homepage.
        </Text>
        <nav aria-label="Suggested pages" className="not-found-links">
          {destinations.map((item, index) => {
            const href = item.featured ? homeHref : item.href;
            return (
              <Link
                key={item.label}
                href={href}
                className={
                  item.featured
                    ? "not-found-link not-found-link--featured"
                    : "not-found-link"
                }
              >
                <span className="not-found-link-copy">
                  <span className="not-found-link-index" aria-hidden>
                    {String(index + 1).padStart(2, "0")}
                  </span>
                  <span className="not-found-link-label">{item.label}</span>
                  <span className="not-found-link-hint">{item.hint}</span>
                </span>
                <span className="not-found-link-arrow" aria-hidden>
                  →
                </span>
              </Link>
            );
          })}
        </nav>
      </section>
    </main>
  );
}
