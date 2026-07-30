import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import type { MyProject, MyProjectStatus } from "@/lib/project-form";
import { sampleMyProjects } from "@/lib/project-form";

const statusClass: Record<MyProjectStatus, string> = {
  Open: "text-secondary-deep",
  Matching: "text-primary",
  Closed: "text-muted",
};

function ProjectRow({ project }: { project: MyProject }) {
  return (
    <li className="grid gap-4 border-b border-primary/10 py-7 last:border-b-0 sm:grid-cols-[1fr_auto] sm:items-end">
      <div>
        <div className="flex flex-wrap items-center gap-x-3 gap-y-1">
          <Text
            as="p"
            size="1"
            weight="bold"
            className={`!uppercase !tracking-[0.18em] ${statusClass[project.status]}`}
          >
            {project.status}
          </Text>
          <Text as="p" size="1" className="!text-muted">
            Updated {project.updatedAt}
          </Text>
        </div>
        <Heading
          as="h2"
          size="5"
          weight="medium"
          mt="2"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          {project.title}
        </Heading>
        <Text as="p" size="2" mt="2" className="!text-muted">
          {project.researchArea}
          <span className="mx-2 text-primary/25" aria-hidden>
            ·
          </span>
          {project.activityType}
          <span className="mx-2 text-primary/25" aria-hidden>
            ·
          </span>
          {project.volunteersRequired} volunteer
          {project.volunteersRequired === 1 ? "" : "s"}
        </Text>
      </div>
      <div className="flex flex-wrap gap-2">
        <Button type="button" variant="outline" size="sm">
          View
        </Button>
        <Button type="button" variant="ghost" size="sm">
          Edit
        </Button>
      </div>
    </li>
  );
}

export function MyProjectsList({
  projects = sampleMyProjects,
}: {
  projects?: MyProject[];
}) {
  if (projects.length === 0) {
    return (
      <div className="rounded-lg border border-dashed border-primary/20 px-6 py-16 text-center">
        <Heading
          as="h2"
          size="5"
          weight="medium"
          className="!font-[family-name:var(--font-display)] !text-primary"
        >
          No projects yet
        </Heading>
        <Text as="p" size="3" mt="2" className="mx-auto max-w-md !text-muted">
          Post your first research opportunity so undergraduates can apply and
          match with your work.
        </Text>
        <div className="mt-6 flex justify-center">
          <Button href="/my-projects/new" variant="secondary" size="md">
            New project
          </Button>
        </div>
      </div>
    );
  }

  return (
    <ul className="border-y border-primary/10">
      {projects.map((project) => (
        <ProjectRow key={project.id} project={project} />
      ))}
    </ul>
  );
}
