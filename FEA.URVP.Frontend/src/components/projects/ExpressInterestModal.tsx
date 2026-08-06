"use client";

import { useEffect, useId, useState } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { useScrollLock } from "@/hooks/useScrollLock";
import { ApiError } from "@/lib/api";
import { studentRankingsHref } from "@/lib/auth";
import {
  getMyProjectRankings,
  RANK_OPTIONS,
  rankLabel,
  removeProjectRanking,
  upsertProjectRanking,
  type ProjectRankingDto,
  type RankOption,
} from "@/lib/project-rankings-api";

type ExpressInterestModalProps = {
  open: boolean;
  onClose: () => void;
  projectId: string;
  projectTitle: string;
  /** Prefer adjust copy/actions when the project is already ranked. */
  mode?: "express" | "adjust";
  /** Called after a rank is saved or removed so parent lists can refresh. */
  onChanged?: () => void;
};

export function ExpressInterestModal({
  open,
  onClose,
  projectId,
  projectTitle,
  mode = "express",
  onChanged,
}: ExpressInterestModalProps) {
  const titleId = useId();
  const [rankings, setRankings] = useState<ProjectRankingDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState<RankOption | null>(null);
  const [removing, setRemoving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [savedRank, setSavedRank] = useState<RankOption | null>(null);

  useScrollLock(open);

  useEffect(() => {
    if (!open) return;

    let cancelled = false;
    setError(null);
    setSavedRank(null);
    setLoading(true);

    void (async () => {
      try {
        const mine = await getMyProjectRankings();
        if (cancelled) return;
        setRankings(mine);
        const current = mine.find((r) => r.projectId === projectId);
        setSavedRank(
          current && RANK_OPTIONS.includes(current.rank as RankOption)
            ? (current.rank as RankOption)
            : null,
        );
      } catch (err) {
        if (cancelled) return;
        setError(
          err instanceof ApiError
            ? err.message
            : "Could not load your current rankings.",
        );
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [open, projectId]);

  useEffect(() => {
    if (!open) return;

    function onKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") onClose();
    }

    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [open, onClose]);

  async function handleSelectRank(rank: RankOption) {
    setError(null);
    setSubmitting(rank);
    try {
      const updated = await upsertProjectRanking(projectId, rank);
      setSavedRank(updated.rank as RankOption);
      const mine = await getMyProjectRankings();
      setRankings(mine);
      onChanged?.();
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : "Could not save this ranking. Please try again.",
      );
    } finally {
      setSubmitting(null);
    }
  }

  async function handleRemove() {
    setError(null);
    setRemoving(true);
    try {
      await removeProjectRanking(projectId);
      setSavedRank(null);
      const mine = await getMyProjectRankings();
      setRankings(mine);
      onChanged?.();
      onClose();
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.message
          : "Could not remove this ranking. Please try again.",
      );
    } finally {
      setRemoving(false);
    }
  }

  if (!open) return null;

  const busy = submitting !== null || removing;
  const isAdjusting = mode === "adjust" || savedRank !== null;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center px-4 py-6"
      role="presentation"
    >
      <button
        type="button"
        className="absolute inset-0 bg-primary/45 backdrop-blur-[2px]"
        aria-label="Close"
        onClick={onClose}
      />

      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        className="relative z-10 w-full max-w-md overflow-hidden rounded-[var(--radius-lg)] border border-primary/12 bg-surface shadow-[0_24px_60px_-28px_rgba(61,18,72,0.45)]"
      >
        <div className="border-b border-primary/10 px-6 py-5">
          <Text
            as="p"
            size="1"
            weight="bold"
            className="!uppercase !tracking-[0.18em] !text-secondary-deep"
          >
            {isAdjusting ? "Adjust rank" : "Express interest"}
          </Text>
          <Heading
            id={titleId}
            as="h2"
            size="5"
            weight="medium"
            mt="2"
            className="!font-[family-name:var(--font-display)] !text-primary"
          >
            Choose a rank
          </Heading>
          <Text as="p" size="2" mt="2" className="!leading-relaxed !text-muted">
            Rank up to 3 projects as your 1st, 2nd, and 3rd choices. Selecting a
            slot that is already used will replace that project.
          </Text>
          <Text as="p" size="2" mt="2" weight="medium" className="!text-primary">
            {projectTitle}
          </Text>
        </div>

        <div className="space-y-2.5 px-6 py-5">
          {loading ? (
            <Text as="p" size="2" className="!text-muted">
              Loading your rankings…
            </Text>
          ) : (
            RANK_OPTIONS.map((rank) => {
              const occupant = rankings.find((r) => r.rank === rank);
              const isCurrent = savedRank === rank;
              const occupiedByOther =
                Boolean(occupant) && occupant!.projectId !== projectId;

              return (
                <button
                  key={rank}
                  type="button"
                  disabled={busy}
                  onClick={() => void handleSelectRank(rank)}
                  className={`w-full rounded-md border px-4 py-3 text-left transition ${
                    isCurrent
                      ? "border-secondary bg-secondary/10"
                      : "border-primary/12 bg-background hover:border-secondary/60"
                  } disabled:opacity-60`}
                >
                  <div className="flex items-center justify-between gap-3">
                    <span className="text-sm font-semibold text-primary">
                      {rankLabel(rank)}
                    </span>
                    {submitting === rank ? (
                      <span className="text-xs text-muted">Saving…</span>
                    ) : isCurrent ? (
                      <span className="text-xs font-medium text-secondary-deep">
                        Selected
                      </span>
                    ) : occupiedByOther ? (
                      <span className="text-xs text-muted">Replace</span>
                    ) : (
                      <span className="text-xs text-muted">Available</span>
                    )}
                  </div>
                  {occupiedByOther ? (
                    <Text as="p" size="1" mt="1" className="!text-muted">
                      Currently: {occupant!.projectTitle}
                    </Text>
                  ) : null}
                </button>
              );
            })
          )}

          {error ? (
            <Text as="p" size="2" role="alert" className="!text-red-700">
              {error}
            </Text>
          ) : null}

          {savedRank ? (
            <Text as="p" size="2" className="!text-muted">
              Saved as your {rankLabel(savedRank)}. View all in{" "}
              <a
                href={studentRankingsHref()}
                className="font-medium text-secondary-deep underline-offset-2 hover:underline"
              >
                My Rankings
              </a>
              .
            </Text>
          ) : null}
        </div>

        <div className="flex flex-wrap items-center justify-between gap-2 border-t border-primary/10 px-6 py-4">
          {savedRank ? (
            <Button
              type="button"
              variant="ghost"
              size="md"
              disabled={busy}
              onClick={() => void handleRemove()}
              className="!text-red-800 hover:!text-red-900"
            >
              {removing ? "Removing…" : "Remove ranking"}
            </Button>
          ) : (
            <span />
          )}
          <Button type="button" variant="ghost" size="md" onClick={onClose}>
            Close
          </Button>
        </div>
      </div>
    </div>
  );
}
