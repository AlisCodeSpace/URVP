import { workshops as workshopCatalog, type Workshop } from "./workshops";
import { newsArticles, type NewsArticle } from "./news";

export { navLinks } from "./site";

export const testimonials = [
  {
    quote:
      "URVP showed me what real research involves, from reviewing over 100 papers to exploring unanswered questions. It gave me the clarity to choose my future path with confidence.",
    name: "Mufaro Dube",
    role: "Student, Mechanical Engineering",
  },
  {
    quote:
      "URVP challenged me to turn theory into working code through MagNavSim, a flight simulation built with real magnetic data. Turning months of technical work into a first-place poster made the experience even more rewarding.",
    name: "Nadjib Tebal",
    role: "Student, Computer Science",
  },
  {
    quote:
      "URVP took me beyond the classroom, giving me hands-on experience in MRI image segmentation and practical research skills. With my professor’s guidance, I helped turn complex findings into an accessible, visually engaging poster that earned first place.",
    name: "Hanane Halabi",
    role: "Student, Medical Imaging Sciences",
  },
  {
    quote:
      "URVP transformed how I see research, showing me that it can be as enjoyable as it is challenging. With Dr. Paul Gharzouzi’s guidance, the experience contributed to my acceptance into the STEEM master’s program at École Polytechnique and inspired me to continue my research.",
    name: "Clara Abdallah",
    role: "Student, Mechanical Engineering",
  },
] as const;

export type NewsTickerItem = {
  title: string;
  detail: string;
  href: string;
};

export type WorkshopTeaser = {
  title: string;
  date: string;
  blurb: string;
  href: string;
};

export function toNewsTickerItems(articles: NewsArticle[]): NewsTickerItem[] {
  return articles.map((article) => ({
    title: article.title,
    detail: article.ticker,
    href: `/news/${article.slug}`,
  }));
}

export function toWorkshopTeasers(items: Workshop[]): WorkshopTeaser[] {
  return items.map((workshop) => ({
    title: workshop.title,
    date: workshop.date,
    blurb: workshop.description,
    href: "/workshops",
  }));
}

/** Home marquee — derived from the News catalog. */
export const newsItems = toNewsTickerItems(newsArticles);

/** Home teaser — derived from the Workshops page catalog. */
export const workshops = toWorkshopTeasers(workshopCatalog);

export const introEyebrow = "Welcome · AY 2026–27";

export const introHeadline = "Research starts earlier than you think.";

export const introParagraphs = [
  "Now in its eighth year at AUB, the Undergraduate Research Volunteer Program (URVP) is an initiative hosted under the Student Success Unit that offers undergraduate students the opportunity to engage in research early in their academic journey. Since its launch in 2019, the URVP has matched more than 950 students with research projects across various disciplines and continues to expand undergraduate research opportunities at AUB.",
  "AUB students from all undergraduate majors who have completed at least 24 sophomore credits and have a cumulative GPA of 3.0 or above may apply for volunteer research opportunities.",
] as const;

export const introKeyFacts = [
  "Create or update your profile between August 25 and September 30.",
  "Commit at least 8 hours per week for a minimum of six months.",
  "Complete your profile carefully. Matching depends on available opportunities and is not guaranteed.",
  "Incomplete profiles will not be considered.",
  "Not matched this cycle? We encourage you to apply again in a future cycle.",
] as const;

export const featuredItems = [
  {
    kind: "Deadline",
    title: "Student profiles",
    detail: "Open Aug 25 – Sep 30, 2026 to create or update your profile.",
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
