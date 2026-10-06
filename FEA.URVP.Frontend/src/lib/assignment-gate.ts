import type { ProjectDto } from "@/lib/projects-api";
import type { SemesterDto } from "@/lib/semesters-api";

/** Whether admins may assign students to a project right now, and why not. */
export function getAssignmentGate(
  project: ProjectDto,
  semester: SemesterDto | null,
) {
  const applicationsOpen = Boolean(semester?.isApplicationWindowOpen);
  const registrationOpen = Boolean(semester?.isRegistrationWindowOpen);
  const onCurrentCycle =
    Boolean(semester) &&
    semester?.id === project.semesterId &&
    project.status !== "Inactive";
  const seatsOpen = onCurrentCycle && project.status !== "Closed";
  const assignmentAllowed =
    seatsOpen && Boolean(semester) && !applicationsOpen && !registrationOpen;
  const blockedReason = applicationsOpen
    ? "Close the student application window before assigning students."
    : registrationOpen
      ? "Close the registration window before assigning students."
      : !semester
        ? "Start a cycle before assigning students."
        : project.status === "Closed"
          ? "Closed projects cannot accept new assignments."
          : project.status === "Inactive"
            ? "Inactive projects cannot accept assignments until they are reactivated."
            : undefined;

  return { applicationsOpen, assignmentAllowed, blockedReason };
}
