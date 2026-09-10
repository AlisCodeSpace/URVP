import { formatAppDate, formatAppDateRange, parseApiDate } from "@/lib/datetime";
import {
  featuredItems,
  type FeaturedItem,
} from "@/lib/home-content";
import { getActiveSemester, type SemesterDto } from "@/lib/semesters-api";
import { contacts } from "@/lib/site";
import { formatTimeDisplay } from "@/lib/time";
import { listWorkshops, type WorkshopDto } from "@/lib/workshops-api";

function applicationWindowDetail(semester: SemesterDto): string {
  if (!semester.applicationWindowStart && !semester.applicationWindowEnd) {
    return "Profile window dates will be announced.";
  }

  const range = formatAppDateRange(
    semester.applicationWindowStart,
    semester.applicationWindowEnd,
  );

  if (semester.isApplicationWindowOpen) {
    return `Open ${range} to create or update your profile.`;
  }

  if (semester.applicationWindowStart) {
    const start = parseApiDate(semester.applicationWindowStart);
    if (!Number.isNaN(start.getTime()) && start.getTime() > Date.now()) {
      return `Opens ${range} to create or update your profile.`;
    }
  }

  return `Closed. Window: ${range}.`;
}

function cycleDetail(semester: SemesterDto): string {
  const range = formatAppDateRange(semester.cycleStart, semester.cycleEnd);
  const email = contacts[0]?.email;
  return email ? `${range}. Contact ${email}.` : `${range}.`;
}

function pickUpcomingWorkshop(items: WorkshopDto[]): WorkshopDto | null {
  if (items.length === 0) return null;

  const dated = items
    .map((item) => {
      const time = parseApiDate(item.date).getTime();
      return Number.isNaN(time) ? null : { item, time };
    })
    .filter((entry): entry is { item: WorkshopDto; time: number } => entry != null)
    .sort((a, b) => a.time - b.time);

  if (dated.length === 0) return items[0] ?? null;

  const now = Date.now();
  const upcoming = dated.find((entry) => entry.time >= now);
  return (upcoming ?? dated[dated.length - 1]).item;
}

function workshopDetail(workshop: WorkshopDto): string {
  const parts = [
    formatAppDate(workshop.date),
    workshop.time ? formatTimeDisplay(workshop.time) : "",
    workshop.location?.trim() ?? "",
  ].filter(Boolean);
  return parts.join(" · ");
}

export function toFeaturedItems(
  semester: SemesterDto | null,
  workshops: WorkshopDto[],
): FeaturedItem[] {
  const [deadlineFallback, cycleFallback, workshopFallback] = featuredItems;
  const workshop = pickUpcomingWorkshop(workshops);

  return [
    semester
      ? {
          kind: "Deadline",
          title: "Student profiles",
          detail: applicationWindowDetail(semester),
          accent: "secondary",
        }
      : deadlineFallback,
    semester
      ? {
          kind: "Cycle",
          title: semester.name.trim() || "URVP main cycle",
          detail: cycleDetail(semester),
          accent: "primary",
        }
      : cycleFallback,
    workshop
      ? {
          kind: "Workshop",
          title: workshop.title,
          detail: workshopDetail(workshop),
          accent: "secondary",
        }
      : workshopFallback,
  ];
}

export async function loadFeaturedNow(): Promise<FeaturedItem[]> {
  const [semesterResult, workshopsResult] = await Promise.allSettled([
    getActiveSemester(),
    listWorkshops({ publishedOnly: true, pageNumber: 1, pageSize: 200 }),
  ]);

  const semester =
    semesterResult.status === "fulfilled" ? semesterResult.value : null;
  const workshops =
    workshopsResult.status === "fulfilled" ? workshopsResult.value.items : [];

  return toFeaturedItems(semester, workshops);
}
