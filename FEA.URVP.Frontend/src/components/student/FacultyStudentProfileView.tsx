"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { Text } from "@radix-ui/themes";
import { RequireAuth } from "@/components/auth/RequireAuth";
import { PageHeader } from "@/components/layout/PageHeader";
import { StudentProfileReadonly } from "@/components/student/StudentProfileReadonly";
import { ApiError } from "@/lib/api";
import { FACULTY_PORTAL_ROLES, viewProjectHref } from "@/lib/auth";
import {
  getStudentProfile,
  toStudentProfileValues,
} from "@/lib/student-profile-api";
import type { StudentProfileValues } from "@/lib/student-profile";

export function FacultyStudentProfileView({
  userId,
  projectId,
  studentUserId,
}: {
  userId: string;
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
    <RequireAuth userId={userId} roles={FACULTY_PORTAL_ROLES}>
      <main className="flex-1 bg-background">
        <PageHeader
          eyebrow="Faculty portal"
          title={error ? "Student profile" : displayName}
          description="Review this student's qualifications, research interests, and attached documents."
        >
          <Link
            href={viewProjectHref(userId, projectId)}
            className="inline-flex items-center gap-2 text-sm text-white/65 transition hover:text-secondary"
          >
            <span aria-hidden>←</span>
            Back to project
          </Link>
        </PageHeader>

        <section className="site-container py-14 sm:py-16">
          {error ? (
            <Text
              as="p"
              size="3"
              role="alert"
              className="rounded-md bg-red-50 px-3 py-2 !text-red-800"
            >
              {error}
            </Text>
          ) : values == null ? (
            <Text as="p" size="3" className="!text-muted">
              Loading profile…
            </Text>
          ) : (
            <>
              {!exists ? (
                <Text
                  as="p"
                  size="3"
                  className="mb-6 rounded-md border border-primary/12 bg-surface px-4 py-3 !text-muted"
                >
                  This student has not completed their profile yet. Name and
                  email are shown from their account.
                </Text>
              ) : null}
              <StudentProfileReadonly values={values} />
            </>
          )}
        </section>
      </main>
    </RequireAuth>
  );
}
