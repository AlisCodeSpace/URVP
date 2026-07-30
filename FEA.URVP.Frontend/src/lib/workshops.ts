export type Workshop = {
  id: string;
  title: string;
  date: string;
  time?: string;
  location?: string;
  description: string;
  /** Google Form (or other) registration URL */
  registrationUrl: string;
  /** Optional poster image under /public */
  posterSrc?: string;
  posterAlt?: string;
};

/**
 * Workshops are finalized at the start of each semester.
 * Keep this list as the single source for the Workshops page and home teaser.
 */
export const workshops: Workshop[] = [
  {
    id: "profile-writing",
    title: "How to Write a Strong Research Profile",
    date: "Sep 5, 2025",
    time: "4:00 – 5:00 PM",
    location: "Online · Zoom",
    description:
      "Learn how to present your interests, skills, and experience so faculty can quickly see why you’re a strong match for their project.",
    registrationUrl: "https://forms.gle/urvp-workshop-profile-placeholder",
  },
  {
    id: "meeting-pi",
    title: "Meeting Your PI: First Steps",
    date: "Sep 12, 2025",
    time: "4:00 – 5:00 PM",
    location: "West Hall · Auditorium B",
    description:
      "What to expect in your first lab meeting, how to set a weekly cadence, and how to ask productive questions once you’re matched.",
    registrationUrl: "https://forms.gle/urvp-workshop-pi-placeholder",
  },
  {
    id: "ethics-mentorship",
    title: "Research Ethics & Mentorship",
    date: "Oct 3, 2025",
    time: "3:30 – 5:00 PM",
    location: "Online · Zoom",
    description:
      "Core practices for responsible research, authorship conversations, and building a healthy mentor–mentee relationship during your placement.",
    registrationUrl: "https://forms.gle/urvp-workshop-ethics-placeholder",
  },
];

export const workshopsIntro =
  "Short sessions to help you build a strong profile and thrive once you join a research team. Schedules are confirmed at the beginning of each semester — registration opens when each session is announced.";
