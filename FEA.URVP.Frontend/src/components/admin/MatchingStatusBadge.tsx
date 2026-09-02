import type { MatchingRunStatus, PlacementStatus } from "@/lib/matching-api";

type Tone = "active" | "matching" | "muted";

const runTone: Record<MatchingRunStatus, Tone> = {
  Draft: "matching",
  Confirmed: "active",
  Discarded: "muted",
};

const placementTone: Record<PlacementStatus, Tone> = {
  Proposed: "matching",
  Confirmed: "active",
  Declined: "muted",
  Cancelled: "muted",
};

function toneClass(tone: Tone): string {
  if (tone === "active") return " is-active";
  if (tone === "matching") return " is-matching";
  return "";
}

export function RunStatusBadge({ status }: { status: MatchingRunStatus }) {
  return (
    <span className={`admin-value-status${toneClass(runTone[status])}`}>
      {status}
    </span>
  );
}

export function PlacementStatusBadge({ status }: { status: PlacementStatus }) {
  return (
    <span className={`admin-value-status${toneClass(placementTone[status])}`}>
      {status}
    </span>
  );
}
