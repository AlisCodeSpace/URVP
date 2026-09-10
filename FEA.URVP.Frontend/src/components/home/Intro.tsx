"use client";

import { useEffect, useState } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { useApplicationWindow } from "@/hooks/useApplicationWindow";
import {
  defaultHomeIntro,
  loadPublicHomeIntro,
  splitIntroDescription,
  type HomeIntroDto,
} from "@/lib/home-intro-api";

export function Intro() {
  const { loading, semesterName } = useApplicationWindow();
  const [intro, setIntro] = useState<HomeIntroDto>(defaultHomeIntro);
  const eyebrow = loading
    ? "Welcome"
    : semesterName
      ? `Welcome · ${semesterName}`
      : "Welcome";

  useEffect(() => {
    let cancelled = false;
    void loadPublicHomeIntro().then((next) => {
      if (!cancelled) setIntro(next);
    });
    return () => {
      cancelled = true;
    };
  }, []);

  const paragraphs = splitIntroDescription(intro.description);
  const keyPoints = intro.keyPoints.filter((point) => point.trim());

  return (
    <section className="bg-background">
      <div className="site-container py-20 sm:py-28">
        <div className="grid items-start gap-12 lg:grid-cols-[minmax(0,1.45fr)_minmax(16rem,0.9fr)] lg:gap-16">
          <div>
            <Text
              as="p"
              size="2"
              weight="medium"
              className="!uppercase !tracking-[0.2em] !text-secondary-deep"
            >
              {eyebrow}
            </Text>
            <Heading
              as="h2"
              size="7"
              weight="medium"
              mt="3"
              className="!font-[family-name:var(--font-display)] !leading-tight !text-primary"
            >
              {intro.headline}
            </Heading>
            <div className="mt-6 grid gap-4 text-muted">
              {paragraphs.map((paragraph) => (
                <Text as="p" size="3" key={paragraph} className="!leading-relaxed">
                  {paragraph}
                </Text>
              ))}
            </div>
          </div>

          {keyPoints.length > 0 ? (
            <aside className="rounded-lg border border-primary/12 bg-surface px-6 py-7">
              <Text
                as="p"
                size="2"
                weight="medium"
                className="!uppercase !tracking-[0.2em] !text-secondary-deep"
              >
                Key information
              </Text>
              <ul className="mt-5 grid gap-4">
                {keyPoints.map((fact) => (
                  <li
                    key={fact}
                    className="border-l-2 border-secondary pl-4 text-sm leading-relaxed text-muted"
                  >
                    {fact}
                  </li>
                ))}
              </ul>
            </aside>
          ) : null}
        </div>
      </div>
    </section>
  );
}
