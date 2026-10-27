"use client";

import { useId, useState, type FormEvent } from "react";
import { Button } from "@/components/ui/Button";

/**
 * Where a research assistant is picked. For now the admin or faculty types an email;
 * the person need not have signed in yet. To offer a list instead, replace this
 * component and keep the `onSelect` contract: resolve `true` once the person is saved.
 */
export function ResearchAssistantChooser({
  disabled = false,
  onSelect,
}: {
  disabled?: boolean;
  onSelect: (selection: { email: string }) => Promise<boolean>;
}) {
  const inputId = useId();
  const [email, setEmail] = useState("");
  const [saving, setSaving] = useState(false);

  const trimmed = email.trim();

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!trimmed || saving) return;
    setSaving(true);
    try {
      if (await onSelect({ email: trimmed })) setEmail("");
    } finally {
      setSaving(false);
    }
  }

  return (
    <form className="research-assistant-form" onSubmit={(event) => void submit(event)}>
      <div className="research-assistant-form-field">
        <label className="field-label" htmlFor={inputId}>
          Add by email
        </label>
        <input
          id={inputId}
          type="email"
          className="field-input"
          placeholder="name@mail.aub.edu"
          autoComplete="off"
          maxLength={256}
          value={email}
          disabled={disabled || saving}
          onChange={(event) => setEmail(event.target.value)}
        />
      </div>
      <Button
        type="submit"
        variant="primary"
        size="md"
        disabled={disabled || saving || !trimmed}
      >
        {saving ? "Adding…" : "Add assistant"}
      </Button>
    </form>
  );
}
