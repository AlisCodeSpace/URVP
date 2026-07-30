import { Heading, Text } from "@radix-ui/themes";

export function Intro() {
  return (
    <section className="bg-background">
      <div className="mx-auto max-w-6xl px-6 py-20 sm:py-28">
        <Text
          as="p"
          size="2"
          weight="medium"
          className="!uppercase !tracking-[0.2em] !text-secondary-deep"
        >
          Welcome · AY 2025–26
        </Text>
        <Heading
          as="h2"
          size="7"
          weight="medium"
          mt="3"
          className="!font-[family-name:var(--font-display)] !leading-tight !text-primary"
        >
          Research starts earlier than you think.
        </Heading>
        <div className="mt-6 flex max-w-2xl flex-col gap-4 text-muted">
          <Text as="p" size="3" className="!leading-relaxed">
            Now in its seventh year at AUB, the Undergraduate Research Volunteer
            Program (URVP) is an initiative hosted under the Office of the
            Provost. It targets undergraduate students interested in research
            early in their academic journey.
          </Text>
          <Text as="p" size="3" className="!leading-relaxed">
            Since its launch in 2019, URVP has matched over{" "}
            <span className="font-semibold text-primary">800 students</span> with
            projects across faculties. We hope to match as many this year.
          </Text>
          <Text as="p" size="3" className="!leading-relaxed">
            URVP helps students advance knowledge through experiential learning,
            strengthen critical thinking and teamwork, and deepen their
            understanding of research beyond the curriculum. Faculty post
            projects; students apply to find the right match.
          </Text>
        </div>
      </div>
    </section>
  );
}
