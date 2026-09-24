"use client";

import { useCancellableQuery } from "@/hooks/useCancellableQuery";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { StudentProfileReadonly } from "@/components/student/StudentProfileReadonly";
import { BackLink } from "@/components/ui/BackLink";
import { ProfileFormSkeleton } from "@/components/ui/SectionSkeletons";
import { adminProjectHref } from "@/lib/auth";
import {
  getStudentProfile,
  toStudentProfileValues,
} from "@/lib/student-profile-api";

export function AdminStudentProfileView({
  projectId,
  studentUserId,
}: {
  projectId: string;
  studentUserId: string;
}) {
  const profileQuery = useCancellableQuery(
    async () => {
      const dto = await getStudentProfile(studentUserId);
      return { exists: dto.exists, values: toStudentProfileValues(dto) };
    },
    [studentUserId],
    { fallbackError: "Could not load this student profile." },
  );
  const values = profileQuery.data?.values ?? null;
  const exists = profileQuery.data?.exists ?? true;
  const error = profileQuery.error;

  const displayName = values
    ? `${values.firstName} ${values.lastName}`.trim() || "Student profile"
    : "Student profile";

  return (
    <div className="admin-panel admin-panel--wide">
      <div className="admin-detail-back">
        <BackLink href={adminProjectHref(projectId)}>Back to project</BackLink>
      </div>

      {error ? (
        <>
          <AdminPageHeader
            title="Student profile"
            description="This profile could not be loaded."
          />
          <p className="admin-users-banner is-error" role="alert">
            {error}
          </p>
        </>
      ) : values == null ? (
        <>
          <AdminPageHeader
            title="Student profile"
            description="Loading student details."
          />
          <ProfileFormSkeleton />
        </>
      ) : (
        <>
          <AdminPageHeader
            title={displayName}
            description="Qualifications, research interests, and documents on this student's URVP profile."
          />
          {!exists ? (
            <p className="admin-users-banner" role="status">
              This student has not completed their profile yet. Name and email
              are shown from their account.
            </p>
          ) : null}
          <StudentProfileReadonly values={values} />
        </>
      )}
    </div>
  );
}
