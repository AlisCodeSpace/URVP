export type NewsArticle = {
  slug: string;
  title: string;
  excerpt: string;
  category: string;
  date: string;
  dateISO: string;
  author: string;
  featured?: boolean;
  /** Short line for home marquee */
  ticker: string;
  body: string[];
};

export const newsIntro =
  "Announcements, cycle updates, and stories from undergraduates and mentors across AUB faculties.";

export const newsArticles: NewsArticle[] = [
  {
    slug: "profile-window-open-2025",
    title: "Student profile window is open for AY 2025–26",
    excerpt:
      "Create or update your URVP student profile between August 25 and September 30, 2025 — the first step toward matching with faculty research.",
    category: "Deadline",
    date: "Aug 20, 2025",
    dateISO: "2025-08-20",
    author: "URVP Office",
    featured: true,
    ticker: "Create or update your student profile Aug 25 – Sep 30, 2025.",
    body: [
      "The Undergraduate Research Volunteer Program is opening the student profile window for Academic Year 2025–26. Between Monday, August 25 and Tuesday, September 30, 2025, undergraduates can create a new profile or refresh an existing one so faculty can discover the right match.",
      "A complete profile is essential. Incomplete submissions are not considered during matching. Include your major, completed credits, areas of interest, relevant coursework, and any prior research or lab experience.",
      "Students from all undergraduate majors who have completed at least 24 sophomore credits and hold a GPA of 3.0 or above are eligible to apply. Matching is not guaranteed; if you are unmatched this cycle, you are encouraged to apply again in future years.",
      "Questions about eligibility or the profile process can be directed to Prof. Joseph Costantine at jc14@aub.edu.lb.",
    ],
  },
  {
    slug: "urvp-cycle-begins-2025",
    title: "URVP main cycle begins October 13",
    excerpt:
      "Faculty across AUB faculties will post projects from October 13, 2025 through August 21, 2026. Mark your calendar and prepare your profile early.",
    category: "Cycle",
    date: "Sep 28, 2025",
    dateISO: "2025-09-28",
    author: "Office of the Provost",
    ticker: "Oct 13, 2025 – Aug 21, 2026 across all AUB faculties.",
    body: [
      "The URVP main research cycle for AY 2025–26 runs from Monday, October 13, 2025 to Friday, August 21, 2026. During this window, faculty mentors post projects and undergraduates are matched based on interest, preparation, and availability.",
      "Expect to commit at least eight hours per week for a minimum of six months once matched. The program is designed for experiential learning — strengthening critical thinking, teamwork, and an understanding of research beyond the curriculum.",
      "Faculty interested in posting projects can use the My Projects portal once signed in with AUB credentials. Students should ensure their profiles are complete before the cycle opens.",
    ],
  },
  {
    slug: "eight-hundred-matches",
    title: "800+ students matched since 2019",
    excerpt:
      "Now in its seventh year, URVP continues to connect undergraduates with research opportunities hosted under the Office of the Provost.",
    category: "Milestone",
    date: "Jul 15, 2025",
    dateISO: "2025-07-15",
    author: "URVP Office",
    ticker: "Seventh year of connecting undergraduates with research.",
    body: [
      "Since its launch in 2019, the Undergraduate Research Volunteer Program has matched more than 800 students with faculty-led projects spanning engineering, the sciences, humanities, health, and interdisciplinary centers.",
      "That growth reflects AUB’s commitment to bringing research into the undergraduate experience early — not only as preparation for graduate study, but as a way to deepen curiosity and belonging on campus.",
      "As we enter a seventh year, we hope to match as many students as possible in the coming cycle. Watch this page for Research Day announcements, workshop schedules, and deadline reminders.",
    ],
  },
  {
    slug: "profile-writing-clinic",
    title: "Workshop highlight: Profile writing clinic",
    excerpt:
      "Join the September 5 session on crafting a research profile that stands out when faculty review applications.",
    category: "Workshop",
    date: "Aug 28, 2025",
    dateISO: "2025-08-28",
    author: "URVP Workshops",
    ticker: "Sep 5, 2025 — prepare before matching opens.",
    body: [
      "Before the matching window opens, URVP is hosting a short workshop on writing a strong research profile. The session covers how to describe your interests clearly, highlight relevant skills, and avoid common pitfalls that make profiles hard to evaluate.",
      "The clinic takes place on September 5, 2025. Registration details are available on the Workshops page. Seats are limited; early signup is recommended.",
      "Additional workshops this term include Meeting Your PI: First Steps and Research Ethics & Mentorship — visit the Workshops page for the full list and Google Form registration links.",
    ],
  },
  {
    slug: "research-day-save-the-date",
    title: "Save the date: URVP Research Day",
    excerpt:
      "Our annual showcase of undergraduate research is taking shape. Deadlines and Google Form links will appear on the Research Day page as they are confirmed.",
    category: "Event",
    date: "Jun 10, 2025",
    dateISO: "2025-06-10",
    author: "URVP Office",
    ticker: "Program, abstracts, and registration details coming soon.",
    body: [
      "URVP Research Day brings together undergraduate volunteers and faculty mentors to share project outcomes, celebrate research across AUB, and look ahead to the next matching cycle.",
      "Abstract submission, participant registration, and presenter confirmation dates will be posted on the Research Day page. Forms for applying to present, registering to attend, and requesting updates will open via Google Forms when ready.",
      "Follow News and your AUB email for the official call for abstracts.",
    ],
  },
  {
    slug: "faculty-call-for-projects",
    title: "Faculty call: post your URVP projects",
    excerpt:
      "Mentors across faculties, centers, and institutes are invited to list research opportunities for the upcoming matching cycle.",
    category: "Faculty",
    date: "May 22, 2025",
    dateISO: "2025-05-22",
    author: "URVP Office",
    ticker: "Faculty mentors: post projects for the AY 2025–26 cycle.",
    body: [
      "Faculty and principal investigators who wish to host undergraduate volunteers can prepare project listings ahead of the October cycle. Clear titles, research areas, and volunteer expectations help students apply with confidence.",
      "Use the My Projects area of the portal to draft and submit listings. International and multi-disciplinary projects hosted within AUB faculties, centers, and institutes are welcome.",
      "For questions about posting a project, contact Prof. Joseph Costantine at jc14@aub.edu.lb.",
    ],
  },
  {
    slug: "ethics-workshop-opens",
    title: "Research Ethics & Mentorship workshop registration open",
    excerpt:
      "A core session on responsible research and mentor–mentee collaboration is open for registration ahead of the fall cycle.",
    category: "Workshop",
    date: "May 5, 2025",
    dateISO: "2025-05-05",
    author: "URVP Workshops",
    ticker: "Register for Research Ethics & Mentorship — Oct 3, 2025.",
    body: [
      "URVP’s Research Ethics & Mentorship workshop covers authorship conversations, lab norms, and how to build a productive placement. The session is designed for newly matched volunteers and returning participants alike.",
      "Registration is available through the Workshops page via Google Form. Capacity is limited.",
    ],
  },
  {
    slug: "welcome-ay-2025",
    title: "Welcome to URVP AY 2025–26",
    excerpt:
      "A new academic year begins under the Office of the Provost — with workshops, Research Day planning, and another cycle of undergraduate matching ahead.",
    category: "Announcement",
    date: "Apr 15, 2025",
    dateISO: "2025-04-15",
    author: "Office of the Provost",
    ticker: "A new URVP year begins — workshops, matching, and Research Day ahead.",
    body: [
      "As AY 2025–26 approaches, URVP invites undergraduates and faculty to prepare for another year of research collaboration. Profile windows, workshop schedules, and Research Day details will be published on this portal as they are confirmed.",
      "Whether you are posting a project or applying as a volunteer, start early: complete profiles and clear project descriptions make matching smoother for everyone.",
    ],
  },
];

export function getNewsBySlug(slug: string): NewsArticle | undefined {
  return newsArticles.find((article) => article.slug === slug);
}

export function getFeaturedNews(): NewsArticle {
  return newsArticles.find((article) => article.featured) ?? newsArticles[0];
}

export const NEWS_PAGE_SIZE = 3;

export function getListNews(): NewsArticle[] {
  const featured = getFeaturedNews();
  return newsArticles.filter((article) => article.slug !== featured.slug);
}

export function getNewsPage(page: number): {
  items: NewsArticle[];
  page: number;
  totalPages: number;
  total: number;
} {
  const list = getListNews();
  const total = list.length;
  const totalPages = Math.max(1, Math.ceil(total / NEWS_PAGE_SIZE));
  const safePage = Math.min(Math.max(1, page), totalPages);
  const start = (safePage - 1) * NEWS_PAGE_SIZE;

  return {
    items: list.slice(start, start + NEWS_PAGE_SIZE),
    page: safePage,
    totalPages,
    total,
  };
}

export function getNewsNeighbors(slug: string): {
  previous: NewsArticle | null;
  next: NewsArticle | null;
} {
  const index = newsArticles.findIndex((article) => article.slug === slug);
  if (index === -1) {
    return { previous: null, next: null };
  }
  return {
    previous: index > 0 ? newsArticles[index - 1] : null,
    next: index < newsArticles.length - 1 ? newsArticles[index + 1] : null,
  };
}
