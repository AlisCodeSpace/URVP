import { workshops as workshopCatalog } from "./workshops";
import { newsArticles } from "./news";

export { navLinks } from "./site";

export const testimonials = [
  {
    quote:
      "URVP opened doors I didn’t know existed — I joined a lab in my sophomore year and never looked back.",
    name: "Former URVP Volunteer",
    role: "AUB Undergraduate",
  },
  {
    quote:
      "Matching students early transforms how they see research. The portal makes collaboration seamless.",
    name: "Faculty Mentor",
    role: "Principal Investigator",
  },
  {
    quote:
      "Over 800 students matched since 2019 — this cycle is your chance to be next.",
    name: "URVP Office",
    role: "Office of the Provost",
  },
  {
    quote:
      "Eight hours a week changed how I think critically and work in a team.",
    name: "URVP Alumnus",
    role: "Research Track",
  },
] as const;

/** Home marquee — derived from the News catalog. */
export const newsItems = newsArticles.map((article) => ({
  title: article.title,
  detail: article.ticker,
  href: `/news/${article.slug}`,
}));

/** Home teaser — derived from the Workshops page catalog. */
export const workshops = workshopCatalog.map((workshop) => ({
  title: workshop.title,
  date: workshop.date,
  blurb: workshop.description,
  href: "/workshops",
}));

export const featuredItems = [
  {
    kind: "Deadline",
    title: "Student profiles",
    detail: "Open Aug 25 – Sep 30, 2025 to create or update your profile.",
    accent: "secondary" as const,
  },
  {
    kind: "Cycle",
    title: "URVP main cycle",
    detail: "Oct 13, 2025 – Aug 21, 2026. Contact jc14@aub.edu.lb.",
    accent: "primary" as const,
  },
  {
    kind: "Workshop",
    title: "Profile writing clinic",
    detail: "Sep 5, 2025 — prepare before matching opens.",
    accent: "secondary" as const,
  },
] as const;
