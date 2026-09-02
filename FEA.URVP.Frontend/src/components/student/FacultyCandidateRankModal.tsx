"use client";

import { useEffect, useId, useState } from "react";
import { Heading, Text } from "@radix-ui/themes";
import { Button } from "@/components/ui/Button";
import { useScrollLock } from "@/hooks/useScrollLock";
import { ApiError } from "@/lib/api";
import {
  removeFacultyCandidateRanking,
  upsertFacultyCandidateRanking,
} from "@/lib/faculty-candidate-rankings-api";
import {
  getProjectRankings,
  RANK_OPTIONS,
  rankLabel,
  type ProjectRankingStudentDto,
  type RankOption,
} from "@/lib/project-rankings-api";
import { ModalRowsSkeleton } from "@/components/ui/SectionSkeletons";

type FacultyCandidateRankModalProps = {
  open: boolean;
  onClose: () => void;
  projectId: string;
  studentUserId: string;
  studentName: string;
  volunteersRequired: number;
  onChanged?: () => void;
};

export function FacultyCandidateRankModal({
  open,
  onClose,
  projectId,
  studentUserId,
  studentName,
  volunteersRequired,
  onChanged,
}: FacultyCandidateRankModalProps) {
  const titleId = useId();
  const [rankings, setRankings] = useState<ProjectRankingStudentDto[]>([]);
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
        const next = await getProjectRankings(projectId);
        if (cancelled) return;
        setRankings(next);
        const current = next.find((r) => r.studentUserId === studentUserId);
        setSavedRank(
          current && RANK_OPTIONS.includes(current.facultyRank as RankOption)
            ? (current.facultyRank as RankOption)
            : null,
        );
      } catch (err) {
        if (cancelled) return;
        setError(
          err instanceof ApiError
            ? err.message
            : "Could not load candidate rankings.",
        );
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [open, projectId, studentUserId]);

  useEffect(() => {
    if (!open) return;

    function onKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") onClose();
    }

    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [open, onClose]);

  const seats = Math.max(0, volunteersRequired);
  const busy = submitting !== null || removing;
  const isAdjusting = savedRank !== null;

  async function refresh() {
    const next = await getProjectRankings(projectId);
    setRankings(next);
    const current = next.find((r) => r.studentUserId === studentUserId);
    setSavedRank(
      current && RANK_OPTIONS.includes(current.facultyRank as RankOption)
        ? (current.facultyRank as RankOption)
        : null,
    );
    onChanged?.();
  }

  async function handleSelectRank(rank: RankOption) {
    setError(null);
    setSubmitting(rank);
    try {
      await upsertFacultyCandidateRanking(projectId, studentUserId, rank);
      await refresh();
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.errors[0] || err.message
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
      await removeFacultyCandidateRanking(projectId, studentUserId);
      setSavedRank(null);
      await refresh();
      onClose();
    } catch (err) {
      setError(
        err instanceof ApiError
          ? err.errors[0] || err.message
          : "Could not remove this ranking. Please try again.",
      );
    } finally {
      setRemoving(false);
    }
  }

  if (!open) return null;

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
            {isAdjusting ? "Adjust rank" : "Rank candidate"}
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
            Rank this student as your 1st, 2nd, or 3rd choice. Several students
            may share a tier; matching fills the project&apos;s {seats} seat
            {seats === 1 ? "" : "s"} from your highest tiers first.
          </Text>
          <Text as="p" size="2" mt="2" weight="medium" className="!text-primary">
            {studentName}
          </Text>
        </div>

        <div className="space-y-2.5 px-6 py-5">
          {loading ? (
            <ModalRowsSkeleton />
          ) : (
            RANK_OPTIONS.map((rank) => {
              const others = rankings.filter(
                (r) =>
                  r.facultyRank === rank && r.studentUserId !== studentUserId,
              );
              const isCurrent = savedRank === rank;

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
                    ) : others.length > 0 ? (
                      <span className="text-xs text-muted">
                        {others.length} other
                        {others.length === 1 ? "" : "s"}
                      </span>
                    ) : (
                      <span className="text-xs text-muted">Available</span>
                    )}
                  </div>
                  {others.length > 0 ? (
                    <Text as="p" size="1" mt="1" className="!text-muted">
                      Also: {others.map((o) => o.studentName).join(", ")}
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
              Saved as your {rankLabel(savedRank)}.
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
