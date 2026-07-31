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

/** Placeholder catalog for design — replace with API later. */
export const catalogProjects: CatalogProject[] = [
  {
    id: "1",
    title: "Wireless Sensing for Structural Health Monitoring",
    researchArea: "Engineering & Architecture",
    activityType: "Laboratory-based",
    volunteersRequired: 2,
    volunteersFilled: 0,
    status: "Open",
    postedAt: "Jul 12, 2026",
    postedAtISO: "2026-07-12",
    facultyName: "Joseph Costantine",
    affiliation: "Maroun Semaan Faculty of Engineering & Architecture",
    description:
      "Design and test low-power wireless sensor nodes for monitoring vibration and strain in civil structures. Volunteers will help assemble prototypes, run lab calibrations, and organize measurement logs.",
    minQualifications:
      "Interest in circuits or signals; basic Python or MATLAB helpful but not required.",
    additionalComments:
      "Lab sessions mainly afternoons on campus. Expect 6–8 hours per week.",
    irbStage: "Not required",
  },
  {
    id: "2",
    title: "Arabic NLP for Educational Assessment",
    researchArea: "Computer Science & AI",
    activityType: "Computational / data analysis",
    volunteersRequired: 3,
    volunteersFilled: 1,
    status: "Matching",
    postedAt: "Jul 8, 2026",
    postedAtISO: "2026-07-08",
    facultyName: "Wassim El-Hajj",
    affiliation: "Faculty of Arts & Sciences — Computer Science",
    description:
      "Build and evaluate Arabic-language models that support fairer educational assessment. Volunteers will annotate datasets, run experiments, and document error patterns across dialects.",
    minQualifications:
      "Comfortable with Python; prior ML coursework preferred. Arabic reading proficiency required.",
    additionalComments: "Remote-friendly with weekly team sync on campus.",
    irbStage: "Exempt",
  },
  {
    id: "3",
    title: "Urban Green Spaces and Community Well-being",
    researchArea: "Social Sciences",
    activityType: "Survey / interview",
    volunteersRequired: 1,
    volunteersFilled: 1,
    status: "Closed",
    postedAt: "Jun 22, 2026",
    postedAtISO: "2026-06-22",
    facultyName: "Mona Fawaz",
    affiliation: "Faculty of Arts & Sciences — Sociology",
    description:
      "Study how neighborhood green spaces relate to reported well-being in Greater Beirut. Volunteers support survey outreach, interview transcription, and thematic coding.",
    minQualifications: "Strong writing skills; Arabic and English bilingual preferred.",
    irbStage: "Approved",
  },
  {
    id: "4",
    title: "Antimicrobial Resistance Surveillance in Clinical Isolates",
    researchArea: "Health Sciences",
    activityType: "Laboratory-based",
    volunteersRequired: 2,
    volunteersFilled: 0,
    status: "Open",
    postedAt: "Jul 18, 2026",
    postedAtISO: "2026-07-18",
    facultyName: "Ghassan Matar",
    affiliation: "Faculty of Medicine — Microbiology",
    description:
      "Support culture work and susceptibility testing for surveillance of antimicrobial resistance patterns. Volunteers learn sterile technique, record phenotypes, and help maintain strain inventories.",
    minQualifications:
      "Completed introductory biology lab; willingness to follow biosafety protocols.",
    additionalComments: "Morning lab blocks preferred. PPE provided.",
    irbStage: "Approved",
  },
  {
    id: "5",
    title: "Climate-Resilient Crop Phenotyping with Drone Imagery",
    researchArea: "Agriculture & Food Sciences",
    activityType: "Fieldwork",
    volunteersRequired: 3,
    volunteersFilled: 1,
    status: "Open",
    postedAt: "Jul 15, 2026",
    postedAtISO: "2026-07-15",
    facultyName: "Isam Bashour",
    affiliation: "Faculty of Agricultural & Food Sciences",
    description:
      "Collect and process drone imagery to score crop stress under varying irrigation regimes. Volunteers assist with field campaigns, ground-truthing, and basic image pipelines.",
    minQualifications:
      "Ability to work outdoors; GIS or remote-sensing interest is a plus.",
    additionalComments: "Some weekend field days during peak season.",
    irbStage: "Not required",
  },
  {
    id: "6",
    title: "Ottoman Beirut: Digitizing Archival Maps",
    researchArea: "Humanities",
    activityType: "Literature review / archival",
    volunteersRequired: 2,
    volunteersFilled: 0,
    status: "Open",
    postedAt: "Jul 5, 2026",
    postedAtISO: "2026-07-05",
    facultyName: "Alexis Wick",
    affiliation: "Faculty of Arts & Sciences — History",
    description:
      "Digitize, georeference, and catalog late Ottoman maps of Beirut. Volunteers will work with library collections, write metadata, and prepare public-facing map notes.",
    minQualifications:
      "Careful attention to detail; reading knowledge of Arabic or Ottoman Turkish helpful.",
    additionalComments: "On-site work in University Libraries; flexible weekday hours.",
    irbStage: "Not required",
  },
  {
    id: "7",
    title: "Behavioral Insights for Student Financial Decisions",
    researchArea: "Business & Economics",
    activityType: "Survey / interview",
    volunteersRequired: 2,
    volunteersFilled: 0,
    status: "Matching",
    postedAt: "Jul 10, 2026",
    postedAtISO: "2026-07-10",
    facultyName: "Casandra Diamond",
    affiliation: "Suliman S. Olayan School of Business",
    description:
      "Design and field short surveys on how undergraduates plan spending and savings. Volunteers help recruit participants, clean survey data, and draft preliminary charts.",
    minQualifications: "Introductory statistics; Excel or R comfort preferred.",
    irbStage: "Pending review",
  },
  {
    id: "8",
    title: "Accessible Campus Wayfinding Prototype",
    researchArea: "Interdisciplinary / Other",
    activityType: "Design / prototyping",
    volunteersRequired: 2,
    volunteersFilled: 0,
    status: "Open",
    postedAt: "Jul 20, 2026",
    postedAtISO: "2026-07-20",
    facultyName: "Howayda Al-Harithy",
    affiliation: "Maroun Semaan Faculty of Engineering & Architecture",
    description:
      "Co-design inclusive wayfinding cues for campus visitors with mobility or sensory needs. Volunteers run user interviews, sketch prototypes, and prepare a short exhibition board.",
    minQualifications:
      "Interest in inclusive design; sketching or Figma experience welcome.",
    additionalComments: "Collaborative studio sessions twice weekly.",
    irbStage: "Not yet submitted",
  },
  {
    id: "9",
    title: "Single-Cell Transcriptomics Pipeline Optimization",
    researchArea: "Natural Sciences",
    activityType: "Computational / data analysis",
    volunteersRequired: 1,
    volunteersFilled: 0,
    status: "Open",
    postedAt: "Jul 14, 2026",
    postedAtISO: "2026-07-14",
    facultyName: "Pierre Karam",
    affiliation: "Faculty of Arts & Sciences — Chemistry",
    description:
      "Improve preprocessing scripts for single-cell RNA-seq datasets used in ongoing wet-lab collaborations. Volunteers benchmark QC metrics and document reproducible notebooks.",
    minQualifications:
      "Python or R experience; exposure to genomics is helpful but not mandatory.",
    irbStage: "Not required",
  },
  {
    id: "10",
    title: "Patient Education Materials for Chronic Care Clinics",
    researchArea: "Health Sciences",
    activityType: "Mixed methods",
    volunteersRequired: 2,
    volunteersFilled: 2,
    status: "Closed",
    postedAt: "Jun 10, 2026",
    postedAtISO: "2026-06-10",
    facultyName: "Gladys Honein",
    affiliation: "Rafic Hariri School of Nursing",
    description:
      "Develop and evaluate plain-language education leaflets for chronic care clinics. Volunteers support literature scans, patient feedback sessions, and revision cycles.",
    minQualifications: "Clear writing; interest in health communication.",
    irbStage: "Approved",
  },
];

export function getProjectById(id: string): CatalogProject | undefined {
  return catalogProjects.find((project) => project.id === id);
}

export function openingsLeft(project: CatalogProject): number {
  return Math.max(0, project.volunteersRequired - project.volunteersFilled);
}

export { researchAreas, researchActivityTypes };
