import {
  researchActivityTypes,
  researchAreas,
  type MyProjectStatus,
} from "./project-form";

export type CatalogProject = {
  id: string;
  title: string;
  researchArea: string;
  activityType: string;
  volunteersRequired: number;
  volunteersFilled: number;
  status: MyProjectStatus;
  postedAt: string;
  postedAtISO: string;
  facultyName: string;
  affiliation: string;
  description: string;
  minQualifications?: string;
  additionalComments?: string;
  irbStage: string;
};

export const projectsIntro =
  "Browse faculty-posted research opportunities across AUB. Filter by area, activity type, and openings — then open a listing to learn more and express interest.";

export function openingsLeft(project: CatalogProject): number {
  return Math.max(0, project.volunteersRequired - project.volunteersFilled);
}

export { researchAreas, researchActivityTypes };
