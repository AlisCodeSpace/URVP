/** Student profile form options and shape. */

export const GENDER_OPTIONS = ["Female", "Male"] as const;

export const DEGREE_OPTIONS = [
  "BA",
  "BS",
  "BBA",
  "BEng",
  "BArch",
  "Other",
] as const;

export const LANGUAGE_OPTIONS = [
  "Arabic",
  "English",
  "French",
  "Armenian",
  "German",
  "Spanish",
  "Italian",
  "Turkish",
  "Persian",
  "Russian",
  "Chinese",
  "Japanese",
  "Korean",
  "Portuguese",
  "Hindi",
] as const;

export const MAX_PROFILE_LANGUAGES = 8;
export const MAX_PROFILE_RESEARCH_TOPICS = 6;

const currentYear = new Date().getFullYear();

export const GRADUATION_YEAR_OPTIONS = Array.from({ length: 8 }, (_, i) =>
  String(currentYear + i),
);

export const WEEKDAYS = [
  "Monday",
  "Tuesday",
  "Wednesday",
  "Thursday",
  "Friday",
  "Saturday",
  "Sunday",
] as const;

export type Weekday = (typeof WEEKDAYS)[number];

export const TIME_SLOTS = [
  "Morning (8:00–12:00)",
  "Afternoon (12:00–16:00)",
  "Evening (16:00–20:00)",
] as const;

export type TimeSlot = (typeof TIME_SLOTS)[number];

export type DayAvailability = {
  day: Weekday;
  slots: TimeSlot[];
};

export type StudentProfileValues = {
  firstName: string;
  lastName: string;
  email: string;
  gender: string;
  mobileNumber: string;
  degree: string;
  expectedGraduationYear: string;
  languages: string[];
  otherLanguages: string;
  completedCredits: "" | "yes" | "no";
  cumulativeAverage: string;
  researchTopics: string[];
  publications: string;
  transcriptFileId: string | null;
  transcriptFileName: string | null;
  citiFileId: string | null;
  citiFileName: string | null;
  availability: DayAvailability[];
};

export function emptyAvailability(): DayAvailability[] {
  return WEEKDAYS.map((day) => ({ day, slots: [] }));
}

export function emptyStudentProfile(
  name?: string | null,
  email?: string | null,
): StudentProfileValues {
  const parts = (name ?? "").trim().split(/\s+/).filter(Boolean);
  const firstName = parts[0] ?? "";
  const lastName = parts.length > 1 ? parts.slice(1).join(" ") : "";

  return {
    firstName,
    lastName,
    email: email?.trim() ?? "",
    gender: "",
    mobileNumber: "",
    degree: "",
    expectedGraduationYear: "",
    languages: [],
    otherLanguages: "",
    completedCredits: "",
    cumulativeAverage: "",
    researchTopics: [],
    publications: "",
    transcriptFileId: null,
    transcriptFileName: null,
    citiFileId: null,
    citiFileName: null,
    availability: emptyAvailability(),
  };
}

export function toggleAvailabilitySlot(
  availability: DayAvailability[],
  day: Weekday,
  slot: TimeSlot,
): DayAvailability[] {
  return availability.map((entry) => {
    if (entry.day !== day) return entry;
    const has = entry.slots.includes(slot);
    return {
      ...entry,
      slots: has
        ? entry.slots.filter((s) => s !== slot)
        : [...entry.slots, slot],
    };
  });
}

export function cloneStudentProfile(
  values: StudentProfileValues,
): StudentProfileValues {
  return {
    ...values,
    languages: [...values.languages],
    researchTopics: [...values.researchTopics],
    availability: values.availability.map((entry) => ({
      ...entry,
      slots: [...entry.slots],
    })),
  };
}

export function mergeAvailabilityFromApi(
  entries: { day: string; slots: string[] }[],
): DayAvailability[] {
  const byDay = new Map(
    entries.map((e) => [e.day, e.slots.filter((s) => TIME_SLOTS.includes(s as TimeSlot)) as TimeSlot[]]),
  );
  return WEEKDAYS.map((day) => ({
    day,
    slots: byDay.get(day) ?? [],
  }));
}
