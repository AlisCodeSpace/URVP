import { apiFetch } from "@/lib/api";

export type ProjectResearchAssistant = {
  id: string;
  userId: string | null;
  name: string;
  email: string;
  addedAt: string;
};

/** Either a directory user, or a name and email entered directly. */
export type AddResearchAssistantInput = {
  userId?: string;
  name?: string;
  email?: string;
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
  input: AddResearchAssistantInput,
): Promise<ProjectResearchAssistant> {
  return apiFetch<ProjectResearchAssistant>(
    `/api/projects/${projectId}/research-assistants`,
    {
      method: "POST",
      body: JSON.stringify({
        userId: input.userId ?? null,
        name: input.name ?? null,
        email: input.email ?? null,
      }),
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
