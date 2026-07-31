/** Placeholder applicants until volunteer applications are wired to the API. */
export type ProjectApplicant = {
  id: string;
  name: string;
  email: string;
  major: string;
  classStanding: string;
  gpa: string;
  creditsCompleted: number;
  interests: string[];
  appliedAt: string;
  appliedAtISO: string;
  status: "Pending" | "Accepted" | "Declined";
};

export const dummyProjectApplicants: ProjectApplicant[] = [
  {
    id: "app-1",
    name: "Layla Haddad",
    email: "lhh01@mail.aub.edu",
    major: "Electrical & Computer Engineering",
    classStanding: "Junior",
    gpa: "3.72",
    creditsCompleted: 78,
    interests: ["Antennas", "RF systems", "Signal processing"],
    appliedAt: "Mar 12, 2026",
    appliedAtISO: "2026-03-12",
    status: "Pending",
  },
  {
    id: "app-2",
    name: "Karim Nassar",
    email: "knn04@mail.aub.edu",
    major: "Computer Science",
    classStanding: "Sophomore",
    gpa: "3.45",
    creditsCompleted: 48,
    interests: ["Machine learning", "Data analysis", "Signal processing"],
    appliedAt: "Mar 14, 2026",
    appliedAtISO: "2026-03-14",
    status: "Pending",
  },
  {
    id: "app-3",
    name: "Maya Khoury",
    email: "mak22@mail.aub.edu",
    major: "Biology",
    classStanding: "Senior",
    gpa: "3.91",
    creditsCompleted: 102,
    interests: ["Lab methods", "Scientific writing", "Data analysis"],
    appliedAt: "Mar 18, 2026",
    appliedAtISO: "2026-03-18",
    status: "Accepted",
  },
  {
    id: "app-4",
    name: "Omar Fakhry",
    email: "orf15@mail.aub.edu",
    major: "Electrical & Computer Engineering",
    classStanding: "Senior",
    gpa: "3.58",
    creditsCompleted: 96,
    interests: ["Antennas", "Embedded systems"],
    appliedAt: "Mar 20, 2026",
    appliedAtISO: "2026-03-20",
    status: "Pending",
  },
  {
    id: "app-5",
    name: "Nour Abou Jaoude",
    email: "naj08@mail.aub.edu",
    major: "Computer Science",
    classStanding: "Junior",
    gpa: "3.21",
    creditsCompleted: 66,
    interests: ["Machine learning", "Scientific writing"],
    appliedAt: "Mar 21, 2026",
    appliedAtISO: "2026-03-21",
    status: "Declined",
  },
  {
    id: "app-6",
    name: "Rami Sleiman",
    email: "rss33@mail.aub.edu",
    major: "Mechanical Engineering",
    classStanding: "Junior",
    gpa: "3.67",
    creditsCompleted: 72,
    interests: ["RF systems", "Embedded systems", "Lab methods"],
    appliedAt: "Mar 22, 2026",
    appliedAtISO: "2026-03-22",
    status: "Pending",
  },
];

export function applicantMajors(applicants: ProjectApplicant[]): string[] {
  return [...new Set(applicants.map((a) => a.major))].sort((a, b) =>
    a.localeCompare(b),
  );
}

export function applicantInterests(applicants: ProjectApplicant[]): string[] {
  return [...new Set(applicants.flatMap((a) => a.interests))].sort((a, b) =>
    a.localeCompare(b),
  );
}
