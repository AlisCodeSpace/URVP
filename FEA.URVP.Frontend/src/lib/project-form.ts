import { formatAppDate } from "@/lib/datetime";
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
  { value: "DoesNotNeedIrbApproval", label: "IRB Not Needed" },
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

export function projectStatusClass(status: MyProjectStatus): string {
  if (status === "Open") return "is-active";
  if (status === "Matching") return "is-matching";
  return "";
}

export function isFacultyCandidateRankingLocked(project: {
  status: MyProjectStatus;
  volunteersFilled: number;
}): boolean {
  return project.status !== "Open" || project.volunteersFilled > 0;
}

export function isFacultyProjectEditable(project: {
  isEditableByFaculty?: boolean;
  status?: MyProjectStatus;
  volunteersFilled?: number;
}): boolean {
  if (typeof project.isEditableByFaculty === "boolean") {
    return project.isEditableByFaculty;
  }

  return !isFacultyCandidateRankingLocked({
    status: project.status ?? "Open",
    volunteersFilled: project.volunteersFilled ?? 0,
  });
}

export function facultyProjectEditLockMessage(project: {
  facultyEditLockReason?: string | null;
}): string {
  return (
    project.facultyEditLockReason?.trim() ||
    "This project can no longer be edited."
  );
}

export type MyProject = {
  id: string;
  title: string;
  researchAreas: string[];
  activityTypes: string[];
  description: string;
  volunteersRequired: number;
  volunteersFilled: number;
  status: MyProjectStatus;
  updatedAt: string;
  isEditableByFaculty: boolean;
  facultyEditLockReason?: string | null;
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
  return formatAppDate(iso);
}
