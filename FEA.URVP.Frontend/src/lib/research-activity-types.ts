/** Canonical research activity-type labels (deduped, case-normalized). */
export const RESEARCH_ACTIVITY_TYPES = [
  "3D Modelling",
  "Achiral Research",
  "Archeological Field Work",
  "Cataloging",
  "Coding",
  "Conducting a Case Study",
  "Conducting a Survey",
  "Data Collection",
  "Data Entry",
  "Data Visualization",
  "Data analysis - geospatial",
  "Data analysis - qualitative",
  "Data analysis - quantitative",
  "Data management (includes data documentation)",
  "Device development",
  "Experimental/Wet Lab work",
  "Field work/Data Collection",
  "Image Search",
  "Instrumentation",
  "Interview Transcriptions",
  "Literature Search",
  "Logo Design",
  "Manuscript Writing",
  "Meta-analysis",
  "Online Research on Databases",
  "Participatory-Action research",
  "Photography",
  "Poster Preparation",
  "Programming",
  "Project Management",
  "Proposal Writing",
  "Reports Writing",
  "Research Dissemination (Manuscript Writing; Conference Presentation)",
  "Research dissemination - creating presentations",
  "Researching and evaluating software",
  "Researching methodologies",
  "Researching theories and conceptual frameworks",
  "Simulation",
  "Statistical analysis",
  "Systematic review",
  "Theater",
  "Theoretical Work",
  "Transcription",
  "Translating",
  "Writing a Literature Review",
  "Writing a Research Proposal",
  "digital communication",
  "web design",
  "website development",
] as const;

export type ResearchActivityTypeLabel = (typeof RESEARCH_ACTIVITY_TYPES)[number];

export const MAX_RESEARCH_ACTIVITY_TYPES = 6;

const allowSet = new Set<string>(RESEARCH_ACTIVITY_TYPES);

export function isAllowedResearchActivityType(value: string): boolean {
  return allowSet.has(value);
}
