import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { MyProjectsList } from "@/components/projects/MyProjectsList";
import { Button } from "@/components/ui/Button";
import { PageHeader } from "@/components/layout/PageHeader";
import { newProjectHref } from "@/lib/auth";

export const metadata: Metadata = {
  title: "My Projects | URVP",
  description:
    "Manage research projects you have posted for the Undergraduate Research Volunteer Program.",
};

type MyProjectsPageProps = {
  params: Promise<{ userId: string }>;
};

export default async function MyProjectsPage({ params }: MyProjectsPageProps) {
  const { userId } = await params;

  return (
    <RequireAuth userId={userId}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="Faculty portal"
          title="My projects"
          description="Review projects you have posted and open new opportunities for undergraduate volunteers."
          actions={
            <Button href={newProjectHref(userId)} variant="secondary" size="lg">
              New project
            </Button>
          }
        />

        <section className="mx-auto max-w-6xl px-6 py-14 sm:py-16">
          <MyProjectsList userId={userId} />
        </section>
      </main>
    </RequireAuth>
  );
}
