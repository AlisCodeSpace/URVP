"use client";

import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { formatApplicationAnnouncement } from "@/hooks/useApplicationWindow";
import { studentProfileHref } from "@/lib/auth";

const otherPages = [
  { href: "/", label: "Home" },
  { href: studentProfileHref(), label: "My profile" },
  { href: "/workshops", label: "Workshops" },
  { href: "/news", label: "News" },
] as const;

export function ApplicationWindowClosedNotice({
  semesterName,
}: {
  semesterName?: string | null;
}) {
  return (
    <div
      className="ranked-empty-cta px-6 py-12 text-center sm:px-10 sm:py-16"
      data-tour="project-filters"
    >
      <div data-tour="project-catalog">
        <Heading
          as="h2"
          size="6"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          Application window closed
        </Heading>
        <Text
          as="p"
          size="3"
          mt="3"
          className="mx-auto max-w-lg !leading-relaxed !text-muted"
        >
          {semesterName
            ? `${formatApplicationAnnouncement(semesterName, false)} Please wait until it opens to browse research projects and express interest.`
            : "The student application window is currently closed. Please wait until it opens to browse research projects and express interest."}{" "}
          In the meantime, you can visit other pages on the website.
        </Text>
        <div className="mt-8 flex flex-wrap justify-center gap-3">
          {otherPages.map((page, index) => (
            <Button
              key={page.href}
              href={page.href}
              variant={index === 0 ? "primary" : "outline"}
              size="md"
            >
              {page.label}
            </Button>
          ))}
        </div>
      </div>
    </div>
  );
}
