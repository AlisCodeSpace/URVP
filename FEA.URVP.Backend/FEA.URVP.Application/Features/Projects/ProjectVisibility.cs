using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Entities.Semesters;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Projects;

public static class ProjectVisibility
{
    public static bool IsOnCurrentCycle(Project project, Semester? active, DateTime utcNow) =>
        active is not null
        && active.IsCycleActive(utcNow)
        && project.SemesterId == active.Id
        && project.Status != ProjectStatus.Inactive;
}
