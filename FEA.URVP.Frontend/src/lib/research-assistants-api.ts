import { apiFetch } from "@/lib/api";
import type { MyProjectStatus } from "@/lib/project-form";

export type ProjectResearchAssistant = {
  id: string;
  /** Stored lowercase. */
  email: string;
  /** Account name; null until the person has signed in. */
  name: string | null;
  hasSignedIn: boolean;
  addedAt: string;
};

/** A project the signed-in person is listed on as a research assistant. */
export type AssistedProject = {
  projectId: string;
  title: string;
  semesterName: string;
  status: MyProjectStatus;
  facultyName: string;
  facultyEmail: string;
  addedAt: string;
};

export async function listResearchAssistants(
  projectId: string,
): Promise<ProjectResearchAssistant[]> {
  return apiFetch<ProjectResearchAssistant[]>(
    `/api/projects/${projectId}/research-assistants`,
  );
}

export async function addResearchAssistant(
  projectId: string,
  email: string,
): Promise<ProjectResearchAssistant> {
  return apiFetch<ProjectResearchAssistant>(
    `/api/projects/${projectId}/research-assistants`,
    {
      method: "POST",
      body: JSON.stringify({ email }),
    },
  );
}

export async function removeResearchAssistant(
  projectId: string,
  assistantId: string,
): Promise<void> {
  await apiFetch<null>(
    `/api/projects/${projectId}/research-assistants/${assistantId}`,
    { method: "DELETE" },
  );
}

/** Projects where the signed-in user is listed as a research assistant, matched by email. */
export async function listAssistedProjects(): Promise<AssistedProject[]> {
  return apiFetch<AssistedProject[]>("/api/projects/assisting");
}
