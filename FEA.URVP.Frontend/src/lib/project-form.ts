export const programDescription =
  "University Research Volunteer Program provides research opportunities from across AUB faculties, fields, and disciplines. Students will be matched with projects based on their areas of interest. Students will also be provided research opportunities that are multi-disciplinary, hosted within faculties, centers, and institutes.";

export const researchAreas = [
  "Engineering & Architecture",
  "Natural Sciences",
  "Health Sciences",
  "Social Sciences",
  "Humanities",
  "Business & Economics",
  "Computer Science & AI",
  "Agriculture & Food Sciences",
  "Interdisciplinary / Other",
] as const;

export const irbStages = [
  "Not required",
  "Not yet submitted",
  "Pending review",
  "Approved",
  "Exempt",
] as const;

export const researchActivityTypes = [
  "Laboratory-based",
  "Computational / data analysis",
  "Fieldwork",
  "Literature review / archival",
  "Survey / interview",
  "Design / prototyping",
  "Clinical / patient-related",
  "Mixed methods",
] as const;

export type MyProjectStatus = "Open" | "Matching" | "Closed";

export type MyProject = {
  id: string;
  title: string;
  researchArea: string;
  activityType: string;
  volunteersRequired: number;
  status: MyProjectStatus;
  updatedAt: string;
};

/** Placeholder data for design — replace with API later. */
export const sampleMyProjects: MyProject[] = [
  {
    id: "1",
    title: "Wireless Sensing for Structural Health Monitoring",
    researchArea: "Engineering & Architecture",
    activityType: "Laboratory-based",
    volunteersRequired: 2,
    status: "Open",
    updatedAt: "Jul 12, 2026",
  },
  {
    id: "2",
    title: "Arabic NLP for Educational Assessment",
    researchArea: "Computer Science & AI",
    activityType: "Computational / data analysis",
    volunteersRequired: 3,
    status: "Matching",
    updatedAt: "Jul 8, 2026",
  },
  {
    id: "3",
    title: "Urban Green Spaces and Community Well-being",
    researchArea: "Social Sciences",
    activityType: "Survey / interview",
    volunteersRequired: 1,
    status: "Closed",
    updatedAt: "Jun 22, 2026",
  },
];
