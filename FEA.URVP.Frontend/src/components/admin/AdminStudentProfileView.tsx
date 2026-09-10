"use client";

import { useEffect, useState } from "react";
import { AdminPageHeader } from "@/components/admin/AdminPlaceholder";
import { StudentProfileReadonly } from "@/components/student/StudentProfileReadonly";
import { BackLink } from "@/components/ui/BackLink";
import { ProfileFormSkeleton } from "@/components/ui/SectionSkeletons";
import { ApiError } from "@/lib/api";
import { adminProjectHref } from "@/lib/auth";
import {
  getStudentProfile,
  toStudentProfileValues,
} from "@/lib/student-profile-api";
import type { StudentProfileValues } from "@/lib/student-profile";

export function AdminStudentProfileView({
  projectId,
  studentUserId,
}: {
  projectId: string;
  studentUserId: string;
}) {
  const [values, setValues] = useState<StudentProfileValues | null>(null);
  const [exists, setExists] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    void (async () => {
      try {
        const dto = await getStudentProfile(studentUserId);
        if (cancelled) return;
        setExists(dto.exists);
        setValues(toStudentProfileValues(dto));
      } catch (err) {
        if (cancelled) return;
        setError(
          err instanceof ApiError
            ? err.message
            : "Could not load this student profile.",
        );
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [studentUserId]);

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
