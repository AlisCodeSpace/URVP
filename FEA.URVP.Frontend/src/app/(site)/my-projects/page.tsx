import type { Metadata } from "next";
import { Heading, Text } from "@radix-ui/themes";
import { MyProjectsList } from "@/components/projects/MyProjectsList";
import { Button } from "@/components/ui/Button";

export const metadata: Metadata = {
  title: "My Projects | URVP",
  description:
    "Manage research projects you have posted for the Undergraduate Research Volunteer Program.",
};

export default function MyProjectsPage() {
  return (
    <main className="flex-1 bg-background">
      <section className="border-b border-primary/10 bg-primary-deep text-white">
        <div className="mx-auto flex max-w-6xl flex-col gap-6 px-6 py-16 sm:flex-row sm:items-end sm:justify-between sm:py-20">
          <div>
            <Text
              as="p"
              size="2"
              weight="medium"
              className="!uppercase !tracking-[0.2em] !text-secondary"
            >
              Faculty portal
            </Text>
            <Heading
              as="h1"
              size="8"
              weight="medium"
              mt="3"
              className="!font-[family-name:var(--font-display)] !text-white"
            >
              My projects
            </Heading>
            <Text
              as="p"
              size="4"
              mt="3"
              className="max-w-xl !leading-relaxed !text-white/75"
            >
              Review projects you have posted and open new opportunities for
              undergraduate volunteers.
            </Text>
          </div>
          <Button href="/my-projects/new" variant="secondary" size="lg">
            New project
          </Button>
        </div>
      </section>

      <section className="mx-auto max-w-6xl px-6 py-14 sm:py-16">
        <MyProjectsList />
      </section>
    </main>
  );
}
