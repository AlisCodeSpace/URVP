import type { Metadata } from "next";
import Link from "next/link";
import { Heading, Text } from "@radix-ui/themes";
import { PostProjectForm } from "@/components/projects/PostProjectForm";

export const metadata: Metadata = {
  title: "Post a Project | URVP",
  description:
    "Post a new research project for undergraduate volunteers in the URVP portal.",
};

export default function NewProjectPage() {
  return (
    <main className="flex-1 bg-background">
      <section className="border-b border-primary/10 bg-primary-deep text-white">
        <div className="mx-auto max-w-6xl px-6 py-14 sm:py-16">
          <Link
            href="/my-projects"
            className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
          >
            <span aria-hidden>←</span>
            Back to my projects
          </Link>
          <Text
            as="p"
            size="2"
            weight="medium"
            mt="5"
            className="!uppercase !tracking-[0.2em] !text-secondary"
          >
            New listing
          </Text>
          <Heading
            as="h1"
            size="8"
            weight="medium"
            mt="3"
            className="!font-[family-name:var(--font-display)] !text-white"
          >
            Post a project
          </Heading>
          <Text
            as="p"
            size="4"
            mt="3"
            className="max-w-2xl !leading-relaxed !text-white/75"
          >
            Share a research opportunity across AUB faculties, centers, and
            institutes for undergraduate matching.
          </Text>
        </div>
      </section>

      <section className="mx-auto max-w-3xl px-6 py-12 sm:py-16">
        <PostProjectForm />
      </section>
    </main>
  );
}
