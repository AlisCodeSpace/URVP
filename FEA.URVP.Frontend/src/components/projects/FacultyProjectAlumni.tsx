"use client";

import { Heading, Text } from "@/components/ui/Typography";
import { Button } from "@/components/ui/Button";
import { RankingsListSkeleton } from "@/components/ui/SectionSkeletons";
import { viewRankedStudentHref } from "@/lib/auth";
import { optionalRankLabel } from "@/lib/project-rankings-api";
import type { ProjectAlumniDto } from "@/lib/projects-api";
import type { MyProjectStatus } from "@/lib/project-form";

function groupByCycle(alumni: ProjectAlumniDto[]) {
  const groups: { semesterId: string; semesterName: string; students: ProjectAlumniDto[] }[] = [];
  for (const student of alumni) {
    const current = groups.find((group) => group.semesterId === student.semesterId);
    if (current) {
      current.students.push(student);
      continue;
    }
    groups.push({
      semesterId: student.semesterId,
      semesterName: student.semesterName,
      students: [student],
    });
  }
  return groups;
}

export function FacultyProjectAlumni({
  userId,
  projectId,
  projectStatus,
  alumni,
  loading,
  error,
}: {
  userId: string;
  projectId: string;
  projectStatus: MyProjectStatus;
  alumni: ProjectAlumniDto[] | null;
  loading: boolean;
  error: string | null;
}) {
  if (!error && !loading && alumni != null && alumni.length === 0 && projectStatus !== "Inactive") {
    return null;
  }

  const groups = alumni ? groupByCycle(alumni) : [];

  return (
    <section className="form-section" aria-labelledby="previous-students-heading">
      <Heading
        as="h2"
        size="5"
        weight="medium"
        id="previous-students-heading"
        className="!font-[family-name:var(--font-display)] !text-primary"
      >
        Previous students
      </Heading>
      <Text as="p" size="2" mt="2" className="!text-muted">
        Students from earlier academic cycles. Reactivating a project starts a new roster and keeps this history.
      </Text>

      {error ? (
        <Text as="p" size="3" mt="5" role="alert" className="!text-red-800">
          {error}
        </Text>
      ) : loading || alumni == null ? (
        <RankingsListSkeleton />
      ) : alumni.length === 0 ? (
        <Text as="p" size="3" mt="5" className="!text-muted">
          No students were stored from previous cycles.
        </Text>
      ) : (
        <div className="mt-6 flex flex-col gap-8">
          {groups.map((group) => (
            <div key={group.semesterId}>
              <Text
                as="p"
                size="2"
                weight="medium"
                className="!uppercase !tracking-[0.14em] !text-secondary-deep"
              >
                {group.semesterName}
              </Text>
              <ul className="ranked-students-list">
                {group.students.map((student) => {
                  const studentChoice = optionalRankLabel(student.studentRank ?? 0);
                  const facultyChoice = optionalRankLabel(student.facultyRank ?? 0);
                  return (
                    <li key={`${group.semesterId}-${student.studentUserId}`} className="ranked-students-row">
                      <div className="ranked-students-badges">
                        <span className={`rank-badge${student.wasConfirmed ? " is-matched" : ""}`}>
                          {student.wasConfirmed ? "Confirmed" : "Applied"}
                        </span>
                        {studentChoice ? (
                          <span className="rank-badge">Student {studentChoice}</span>
                        ) : null}
                        {facultyChoice ? (
                          <span className="rank-badge is-faculty">Your {facultyChoice}</span>
                        ) : null}
                      </div>
                      <p className="ranked-students-name">{student.studentName}</p>
                      <p className="ranked-students-meta">{student.studentEmail || "—"}</p>
                      <div className="ranked-students-actions">
                        <Button
                          href={viewRankedStudentHref(userId, projectId, student.studentUserId)}
                          variant="outline-secondary"
                          size="sm"
                        >
                          View profile
                        </Button>
                      </div>
                    </li>
                  );
                })}
              </ul>
            </div>
          ))}
        </div>
      )}
    </section>
  );
}
