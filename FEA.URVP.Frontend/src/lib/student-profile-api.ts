import { apiFetch } from "@/lib/api";
import {
  emptyStudentProfile,
  mergeAvailabilityFromApi,
  type DayAvailability,
  type StudentProfileValues,
  type TimeSlot,
  type Weekday,
} from "@/lib/student-profile";

export type StudentProfileDto = {
  id: string;
  userId: string;
  exists: boolean;
  firstName: string;
  lastName: string;
  email: string;
  gender?: string | null;
  mobileNumber?: string | null;
  degree?: string | null;
  expectedGraduationYear?: number | null;
  languages: string[];
  otherLanguages?: string | null;
  completedCredits?: boolean | null;
  cumulativeAverage?: number | null;
  researchTopics: string[];
  publications?: string | null;
  transcriptFileId?: string | null;
  transcriptFileName?: string | null;
  citiFileId?: string | null;
  citiFileName?: string | null;
  availability: { day: string; slots: string[] }[];
  createdAt?: string | null;
  updatedAt?: string | null;
};

export type FileMetadataDto = {
  id: string;
  entityType: string;
  entityId: string;
  fileCategory: string;
  fileName: string;
  mimeType: string;
  fileSize: number;
  uploadedAt: string;
};

export type StudentProfileUpsertPayload = {
  gender: string;
  mobileNumber: string;
  degree: string;
  expectedGraduationYear: number;
  languages: string[];
  otherLanguages?: string | null;
  completedCredits: boolean;
  cumulativeAverage: number;
  researchTopics: string[];
  publications?: string | null;
  transcriptFileId: string;
  citiFileId?: string | null;
  availability: { day: string; slots: string[] }[];
};

export function toStudentProfileValues(
  dto: StudentProfileDto,
): StudentProfileValues {
  const base = emptyStudentProfile(
    `${dto.firstName} ${dto.lastName}`.trim(),
    dto.email,
  );

  if (!dto.exists) {
    return {
      ...base,
      firstName: dto.firstName || base.firstName,
      lastName: dto.lastName || base.lastName,
      email: dto.email || base.email,
    };
  }

  return {
    firstName: dto.firstName,
    lastName: dto.lastName,
    email: dto.email,
    gender: dto.gender ?? "",
    mobileNumber: dto.mobileNumber ?? "",
    degree: dto.degree ?? "",
    expectedGraduationYear: dto.expectedGraduationYear
      ? String(dto.expectedGraduationYear)
      : "",
    languages: [...(dto.languages ?? [])],
    otherLanguages: dto.otherLanguages ?? "",
    completedCredits:
      dto.completedCredits == null ? "" : dto.completedCredits ? "yes" : "no",
    cumulativeAverage:
      dto.cumulativeAverage == null ? "" : String(dto.cumulativeAverage),
    researchTopics: [...(dto.researchTopics ?? [])],
    publications: dto.publications ?? "",
    transcriptFileId: dto.transcriptFileId ?? null,
    transcriptFileName: dto.transcriptFileName ?? null,
    citiFileId: dto.citiFileId ?? null,
    citiFileName: dto.citiFileName ?? null,
    availability: mergeAvailabilityFromApi(dto.availability ?? []),
  };
}

export function toUpsertPayload(
  values: StudentProfileValues,
): StudentProfileUpsertPayload {
  if (!values.transcriptFileId) {
    throw new Error("Transcript file is required.");
  }

  return {
    gender: values.gender,
    mobileNumber: values.mobileNumber.trim(),
    degree: values.degree,
    expectedGraduationYear: Number(values.expectedGraduationYear),
    languages: values.languages,
    otherLanguages: values.otherLanguages.trim() || null,
    completedCredits: values.completedCredits === "yes",
    cumulativeAverage: Number(values.cumulativeAverage),
    researchTopics: values.researchTopics,
    publications: values.publications.trim() || null,
    transcriptFileId: values.transcriptFileId,
    citiFileId: values.citiFileId,
    availability: values.availability
      .filter((d) => d.slots.length > 0)
      .map((d) => ({ day: d.day, slots: d.slots })),
  };
}

export async function getMyStudentProfile(): Promise<StudentProfileDto> {
  return apiFetch<StudentProfileDto>("/api/student-profiles/me");
}

export async function upsertMyStudentProfile(
  payload: StudentProfileUpsertPayload,
): Promise<StudentProfileDto> {
  return apiFetch<StudentProfileDto>("/api/student-profiles/me", {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export async function uploadStudentDocument(
  userId: string,
  category: "Transcript" | "CitiCertification",
  file: File,
): Promise<FileMetadataDto> {
  const body = new FormData();
  body.append("file", file);
  body.append("entityType", "StudentProfile");
  body.append("entityId", userId);
  body.append("fileCategory", category);

  return apiFetch<FileMetadataDto>("/api/files", {
    method: "POST",
    body,
  });
}

/** Unused helpers kept for typed availability editing. */
export type { DayAvailability, TimeSlot, Weekday };
