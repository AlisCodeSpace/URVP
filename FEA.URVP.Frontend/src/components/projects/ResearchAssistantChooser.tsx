"use client";

import type { AddResearchAssistantInput } from "@/lib/research-assistants-api";

/**
 * Where an admin or faculty member picks a research assistant.
 * The source — a student list or a typed name and email — is not decided yet.
 * Call `onSelect` with the chosen person; the project page already saves it.
 */
export function ResearchAssistantChooser({
  disabled = false,
  onSelect,
}: {
  disabled?: boolean;
  onSelect: (selection: AddResearchAssistantInput) => void;
}) {
  void onSelect;
  void disabled;

  return (
    <p className="research-assistant-chooser">
      Choosing the assistant, from a list or by entering their details, will be connected here.
    </p>
  );
}
