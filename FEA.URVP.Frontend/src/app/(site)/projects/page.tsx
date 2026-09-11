import type { Metadata } from "next";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { ProjectsCatalogView } from "@/components/projects/ProjectsCatalogView";

export const metadata: Metadata = {
  title: "Projects | URVP",
  description:
    "Browse faculty research projects and find volunteer opportunities in the Undergraduate Research Volunteer Program at AUB.",
};

export default function ProjectsPage() {
  return (
    <RequireAuth>
      <main className="flex-1 bg-background">
        <ProjectsCatalogView />
      </main>
    </RequireAuth>
  );
}
