import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PageHeader } from "@/components/layout/PageHeader";
import { AssistedProjectsList } from "@/components/projects/AssistedProjectsList";

export const metadata: Metadata = {
  title: "Research Assistant | URVP",
  description: "Projects you assist. Open one to review it and rank students.",
};

export default function ResearchAssistantPage() {
  return (
    <RequireAuth>
      <main className="flex-1 bg-background">
        <PageHeader
          title="Research assistant"
          description="Projects you assist. Open one to review it and rank students."
        />
        <section className="site-container py-10 sm:py-14">
          <AssistedProjectsList />
        </section>
      </main>
    </RequireAuth>
  );
}
