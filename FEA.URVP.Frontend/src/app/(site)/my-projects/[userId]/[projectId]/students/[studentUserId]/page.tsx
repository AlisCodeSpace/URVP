import type { Metadata } from "next";
import { FacultyStudentProfileView } from "@/components/student/FacultyStudentProfileView";

export const metadata: Metadata = {
  title: "Student Profile | URVP",
  description:
    "Review a student who ranked a research project you posted in the URVP portal.",
};

type FacultyStudentProfilePageProps = {
  params: Promise<{
    userId: string;
    projectId: string;
    studentUserId: string;
  }>;
};

export default async function FacultyStudentProfilePage({
  params,
}: FacultyStudentProfilePageProps) {
  const { userId, projectId, studentUserId } = await params;
  return (
    <FacultyStudentProfileView
      userId={userId}
      projectId={projectId}
      studentUserId={studentUserId}
    />
  );
}
