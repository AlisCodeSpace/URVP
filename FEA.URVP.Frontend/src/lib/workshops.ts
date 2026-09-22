export type Workshop = {
  id: string;
  title: string;
  date: string;
  time?: string;
  location?: string;
  description: string;
  /** Google Form (or other) registration URL */
  registrationUrl: string;
  /** Poster image served by the backend file endpoint */
  posterSrc?: string;
  posterAlt?: string;
};

export const workshopsIntro =
  "Build a strong profile and thrive on a research team. Schedules confirmed each semester; register when sessions open.";
