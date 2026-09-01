namespace FEA.URVP.Domain.Catalog;

public static class NewsSeedCatalog
{
    public sealed record SeedArticle(
        string Slug,
        string Title,
        string Excerpt,
        string Category,
        string Author,
        string Ticker,
        DateTime PublishedAt,
        bool Featured,
        IReadOnlyList<string> Body);

    public static readonly IReadOnlyList<SeedArticle> Articles =
    [
        new(
            "profile-window-open-2025",
            "Student profile window is open for AY 2025–26",
            "Create or update your URVP student profile between August 25 and September 30, 2025 — the first step toward matching with faculty research.",
            "Deadline",
            "URVP Office",
            "Create or update your student profile Aug 25 – Sep 30, 2025.",
            new DateTime(2025, 8, 20, 0, 0, 0, DateTimeKind.Utc),
            true,
            [
                "The Undergraduate Research Volunteer Program is opening the student profile window for Academic Year 2025–26. Between Monday, August 25 and Tuesday, September 30, 2025, undergraduates can create a new profile or refresh an existing one so faculty can discover the right match.",
                "A complete profile is essential. Incomplete submissions are not considered during matching. Include your major, completed credits, areas of interest, relevant coursework, and any prior research or lab experience.",
                "Students from all undergraduate majors who have completed at least 24 sophomore credits and hold a GPA of 3.0 or above are eligible to apply. Matching is not guaranteed; if you are unmatched this cycle, you are encouraged to apply again in future years.",
                "Questions about eligibility or the profile process can be directed to Prof. Joseph Costantine at jc14@aub.edu.lb.",
            ]),
        new(
            "urvp-cycle-begins-2025",
            "URVP main cycle begins October 13",
            "Faculty across AUB faculties will post projects from October 13, 2025 through August 21, 2026. Mark your calendar and prepare your profile early.",
            "Cycle",
            "Office of the Provost",
            "Oct 13, 2025 – Aug 21, 2026 across all AUB faculties.",
            new DateTime(2025, 9, 28, 0, 0, 0, DateTimeKind.Utc),
            false,
            [
                "The URVP main research cycle for AY 2025–26 runs from Monday, October 13, 2025 to Friday, August 21, 2026. During this window, faculty mentors post projects and undergraduates are matched based on interest, preparation, and availability.",
                "Expect to commit at least eight hours per week for a minimum of six months once matched. The program is designed for experiential learning — strengthening critical thinking, teamwork, and an understanding of research beyond the curriculum.",
                "Faculty interested in posting projects can use the My Projects portal once signed in with AUB credentials. Students should ensure their profiles are complete before the cycle opens.",
            ]),
        new(
            "eight-hundred-matches",
            "800+ students matched since 2019",
            "Now in its seventh year, URVP continues to connect undergraduates with research opportunities hosted under the Office of the Provost.",
            "Milestone",
            "URVP Office",
            "Seventh year of connecting undergraduates with research.",
            new DateTime(2025, 7, 15, 0, 0, 0, DateTimeKind.Utc),
            false,
            [
                "Since its launch in 2019, the Undergraduate Research Volunteer Program has matched more than 800 students with faculty-led projects spanning engineering, the sciences, humanities, health, and interdisciplinary centers.",
                "That growth reflects AUB’s commitment to bringing research into the undergraduate experience early — not only as preparation for graduate study, but as a way to deepen curiosity and belonging on campus.",
                "As we enter a seventh year, we hope to match as many students as possible in the coming cycle. Watch this page for Research Day announcements, workshop schedules, and deadline reminders.",
            ]),
        new(
            "profile-writing-clinic",
            "Workshop highlight: Profile writing clinic",
            "Join the September 5 session on crafting a research profile that stands out when faculty review applications.",
            "Workshop",
            "URVP Workshops",
            "Sep 5, 2025 — prepare before matching opens.",
            new DateTime(2025, 8, 28, 0, 0, 0, DateTimeKind.Utc),
            false,
            [
                "Before the matching window opens, URVP is hosting a short workshop on writing a strong research profile. The session covers how to describe your interests clearly, highlight relevant skills, and avoid common pitfalls that make profiles hard to evaluate.",
                "The clinic takes place on September 5, 2025. Registration details are available on the Workshops page. Seats are limited; early signup is recommended.",
                "Additional workshops this term include Meeting Your PI: First Steps and Research Ethics & Mentorship — visit the Workshops page for the full list and Google Form registration links.",
            ]),
        new(
            "research-day-save-the-date",
            "Save the date: URVP Research Day",
            "Our annual showcase of undergraduate research is taking shape. Deadlines and Google Form links will appear on the Research Day page as they are confirmed.",
            "Event",
            "URVP Office",
            "Program, abstracts, and registration details coming soon.",
            new DateTime(2025, 6, 10, 0, 0, 0, DateTimeKind.Utc),
            false,
            [
                "URVP Research Day brings together undergraduate volunteers and faculty mentors to share project outcomes, celebrate research across AUB, and look ahead to the next matching cycle.",
                "Abstract submission, participant registration, and presenter confirmation dates will be posted on the Research Day page. Forms for applying to present, registering to attend, and requesting updates will open via Google Forms when ready.",
                "Follow News and your AUB email for the official call for abstracts.",
            ]),
        new(
            "faculty-call-for-projects",
            "Faculty call: post your URVP projects",
            "Mentors across faculties, centers, and institutes are invited to list research opportunities for the upcoming matching cycle.",
            "Faculty",
            "URVP Office",
            "Faculty mentors: post projects for the AY 2025–26 cycle.",
            new DateTime(2025, 5, 22, 0, 0, 0, DateTimeKind.Utc),
            false,
            [
                "Faculty and principal investigators who wish to host undergraduate volunteers can prepare project listings ahead of the October cycle. Clear titles, research areas, and volunteer expectations help students apply with confidence.",
                "Use the My Projects area of the portal to draft and submit listings. International and multi-disciplinary projects hosted within AUB faculties, centers, and institutes are welcome.",
                "For questions about posting a project, contact Prof. Joseph Costantine at jc14@aub.edu.lb.",
            ]),
        new(
            "ethics-workshop-opens",
            "Research Ethics & Mentorship workshop registration open",
            "A core session on responsible research and mentor–mentee collaboration is open for registration ahead of the fall cycle.",
            "Workshop",
            "URVP Workshops",
            "Register for Research Ethics & Mentorship — Oct 3, 2025.",
            new DateTime(2025, 5, 5, 0, 0, 0, DateTimeKind.Utc),
            false,
            [
                "URVP’s Research Ethics & Mentorship workshop covers authorship conversations, lab norms, and how to build a productive placement. The session is designed for newly matched volunteers and returning participants alike.",
                "Registration is available through the Workshops page via Google Form. Capacity is limited.",
            ]),
        new(
            "welcome-ay-2025",
            "Welcome to URVP AY 2025–26",
            "A new academic year begins under the Office of the Provost — with workshops, Research Day planning, and another cycle of undergraduate matching ahead.",
            "Announcement",
            "Office of the Provost",
            "A new URVP year begins — workshops, matching, and Research Day ahead.",
            new DateTime(2025, 4, 15, 0, 0, 0, DateTimeKind.Utc),
            false,
            [
                "As AY 2025–26 approaches, URVP invites undergraduates and faculty to prepare for another year of research collaboration. Profile windows, workshop schedules, and Research Day details will be published on this portal as they are confirmed.",
                "Whether you are posting a project or applying as a volunteer, start early: complete profiles and clear project descriptions make matching smoother for everyone.",
            ]),
    ];
}
