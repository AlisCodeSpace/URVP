import Image from "next/image";
import { Heading, Text } from "@radix-ui/themes";
import type { Workshop } from "@/lib/workshops";
import { workshops as defaultWorkshops } from "@/lib/workshops";

function WorkshopPoster({ workshop }: { workshop: Workshop }) {
  if (workshop.posterSrc) {
    return (
      <div className="workshop-poster relative aspect-[3/2] overflow-hidden bg-primary-deep">
        <Image
          src={workshop.posterSrc}
          alt={workshop.posterAlt ?? `${workshop.title} poster`}
          fill
          className="object-cover"
          sizes="(max-width: 768px) 100vw, 33vw"
        />
      </div>
    );
  }

  return (
    <div
      className="workshop-poster workshop-poster-fallback relative flex aspect-[3/2] flex-col justify-between p-5 text-white"
      aria-hidden
    >
      <p className="text-xs font-medium uppercase tracking-[0.22em] text-secondary">
        URVP Workshop
      </p>
      <div>
        <p className="font-[family-name:var(--font-display)] text-2xl font-medium leading-tight">
          {workshop.title}
        </p>
        <p className="mt-3 text-sm uppercase tracking-[0.16em] text-white/70">
          {workshop.date}
        </p>
      </div>
    </div>
  );
}

function WorkshopCard({ workshop }: { workshop: Workshop }) {
  return (
    <article className="workshop-card flex flex-col overflow-hidden border border-primary/12 bg-surface transition duration-300 hover:border-secondary/70">
      <WorkshopPoster workshop={workshop} />

      <div className="flex flex-1 flex-col px-5 py-6 sm:px-6">
        <Text
          as="p"
          size="1"
          weight="bold"
          className="!uppercase !tracking-[0.18em] !text-secondary-deep"
        >
          {workshop.date}
          {workshop.time ? ` · ${workshop.time}` : null}
        </Text>

        <Heading
          as="h2"
          size="5"
          weight="medium"
          mt="3"
          className="!font-[family-name:var(--font-display)] !leading-snug !text-primary"
        >
          {workshop.title}
        </Heading>

        {workshop.location ? (
          <Text as="p" size="2" mt="2" className="!text-muted">
            {workshop.location}
          </Text>
        ) : null}

        <Text
          as="p"
          size="3"
          mt="3"
          className="flex-1 !leading-relaxed !text-muted"
        >
          {workshop.description}
        </Text>

        <div className="mt-6">
          <a
            href={workshop.registrationUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="btn btn-secondary btn-md w-full"
          >
            Register
          </a>
        </div>
      </div>
    </article>
  );
}

export function WorkshopsList({
  items = defaultWorkshops,
}: {
  items?: Workshop[];
}) {
  if (items.length === 0) {
    return (
      <div className="rounded-lg border border-dashed border-primary/20 px-6 py-16 text-center">
        <Heading
          as="h2"
          size="5"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          Workshops coming soon
        </Heading>
        <Text as="p" size="3" mt="2" className="mx-auto max-w-md !text-muted">
          Sessions are announced at the beginning of each semester. Check back
          once the AY schedule is published.
        </Text>
      </div>
    );
  }

  return (
    <ul className="grid gap-8 sm:grid-cols-2 lg:grid-cols-3">
      {items.map((workshop) => (
        <li key={workshop.id} className="flex">
          <WorkshopCard workshop={workshop} />
        </li>
      ))}
    </ul>
  );
}
