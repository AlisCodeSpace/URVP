export const researchDayIntro =
  "URVP Research Day brings together undergraduate volunteers and faculty mentors to share project outcomes, celebrate research across AUB, and look ahead to the next matching cycle.";

export const researchDayBanner = {
  eyebrow: "Annual showcase · AY 2025–26",
  title: "Research Day",
  subtitle:
    "Celebrate undergraduate research and look ahead to the next cycle.",
  dateLabel: "Date to be announced",
  locationLabel: "American University of Beirut",
} as const;

export type ResearchDayDeadline = {
  id: string;
  label: string;
  date: string;
  detail: string;
};

export const researchDayDeadlines: ResearchDayDeadline[] = [
  {
    id: "poster",
    label: "Poster Submission",
    date: "TBA",
    detail:
      "Submit a poster about your URVP project for consideration on the Research Day program.",
  },
  {
    id: "registration",
    label: "Participant registration",
    date: "TBA",
    detail:
      "Register to attend Research Day — open to URVP volunteers, mentors, and the AUB community.",
  },
  {
    id: "program-confirmation",
    label: "Program confirmation",
    date: "TBA",
    detail:
      "Presenters confirm participation and presentation format - Agenda will be published.",
  },
  {
    id: "best-poster",
    label: "Best Poster Competition",
    date: "TBA",
    detail:
      "A jury will evaluate the posters presented - winning projects will be rewarded",
  },
];

export type ResearchDayUpdate = {
  id: string;
  title: string;
  date: string;
  body: string;
};

export const researchDayUpdates: ResearchDayUpdate[] = [
  {
    id: "schedule-pending",
    title: "Program schedule forthcoming",
    date: "Coming soon",
    body: "The full Research Day agenda — keynotes, student presentations, and networking — will be published here once confirmed.",
  },
  {
    id: "call-for-abstracts",
    title: "Call for abstracts",
    date: "Coming soon",
    body: "Eligible URVP volunteers will be invited to submit abstracts. Watch this page and your AUB email for the official call.",
  },
];