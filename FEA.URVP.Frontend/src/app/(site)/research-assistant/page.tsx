import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PageHeader } from "@/components/layout/PageHeader";
import { AssistedProjectsList } from "@/components/projects/AssistedProjectsList";

export const metadata: Metadata = {
  title: "Research Assistant | URVP",
  description: "Projects on which you are listed as a research assistant.",
};

export default function ResearchAssistantPage() {
  return (
    <RequireAuth>
      <main className="flex-1 bg-background">
        <PageHeader
          title="Research assistant"
          description="Projects you were added to as a research assistant. You work on these outside the portal, so there is nothing to do here."
        />
        <section className="site-container py-10 sm:py-14">
          <AssistedProjectsList />
        </section>
      </main>
    </RequireAuth>
  );
}
