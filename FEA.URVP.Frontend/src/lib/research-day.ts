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
    id: "abstract",
    label: "Abstract submission",
    date: "TBA",
    detail:
      "Submit a short abstract of your URVP project for consideration on the Research Day program.",
  },
  {
    id: "registration",
    label: "Participant registration",
    date: "TBA",
    detail:
      "Register to attend Research Day — open to URVP volunteers, mentors, and the AUB community.",
  },
  {
    id: "presenter",
    label: "Presenter confirmation",
    date: "TBA",
    detail:
      "Selected presenters confirm participation and presentation format (poster or short talk).",
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

export type ResearchDayForm = {
  id: string;
  title: string;
  description: string;
  /** Google Form registration / application URL */
  href: string;
  cta: string;
};

export const researchDayForms: ResearchDayForm[] = [
  {
    id: "apply-present",
    title: "Apply to present",
    description:
      "Submit your project abstract and preferred presentation format for Research Day.",
    href: "https://forms.gle/urvp-research-day-present-placeholder",
    cta: "Open application form",
  },
  {
    id: "register-attend",
    title: "Register to attend",
    description:
      "Reserve your place for Research Day. Mentors, volunteers, and guests are welcome.",
    href: "https://forms.gle/urvp-research-day-attend-placeholder",
    cta: "Open registration form",
  },
  {
    id: "updates-interest",
    title: "Get updates",
    description:
      "Leave your contact details to receive Research Day announcements and deadline reminders.",
    href: "https://forms.gle/urvp-research-day-updates-placeholder",
    cta: "Sign up for updates",
  },
];
