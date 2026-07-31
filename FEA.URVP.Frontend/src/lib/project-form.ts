import { RESEARCH_ACTIVITY_TYPES } from "@/lib/research-activity-types";
import { RESEARCH_AREAS } from "@/lib/research-areas";

export const programDescription =
  "University Research Volunteer Program provides research opportunities from across AUB faculties, fields, and disciplines. Students will be matched with projects based on their areas of interest. Students will also be provided research opportunities that are multi-disciplinary, hosted within faculties, centers, and institutes.";

export type SelectOption = { value: string; label: string };

export const irbStageOptions: readonly SelectOption[] = [
  { value: "IrbApproved", label: "IRB Approved" },
  {
    value: "IrbApplicationInPreparation",
    label: "IRB Application in Preparation",
  },
  { value: "IrbApplicationSubmitted", label: "IRB Application Submitted" },
  { value: "DoesNotNeedIrbApproval", label: "Does not need IRB Approval" },
] as const;

export const projectStatusOptions: readonly SelectOption[] = [
  { value: "Open", label: "Open" },
  { value: "Matching", label: "Matching" },
  { value: "Closed", label: "Closed" },
] as const;

/** Labels for catalog filters. */
export const researchAreas = [...RESEARCH_AREAS];
export const irbStages = irbStageOptions.map((o) => o.label);
export const researchActivityTypes = [...RESEARCH_ACTIVITY_TYPES];

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

export type ProjectFormValues = {
  affiliation: string;
  userName: string;
  title: string;
  researchAreas: string[];
  irbStage: string;
  briefDescription: string;
  activityTypes: string[];
  volunteersRequired: string;
  minQualifications: string;
  additionalComments: string;
  status: MyProjectStatus;
};

export const emptyProjectFormValues: ProjectFormValues = {
  affiliation: "",
  userName: "",
  title: "",
  researchAreas: [],
  irbStage: "",
  briefDescription: "",
  activityTypes: [],
  volunteersRequired: "",
  minQualifications: "",
  additionalComments: "",
  status: "Open",
};

export function formatProjectDate(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return iso;
  return date.toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
}
