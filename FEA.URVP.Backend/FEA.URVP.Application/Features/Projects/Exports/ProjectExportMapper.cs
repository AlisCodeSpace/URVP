using System.Globalization;
using FEA.URVP.Application.Mappings;
using FEA.URVP.Domain.Entities.Matching;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Exports;

public static class ProjectExportMapper
{
    public static IReadOnlyList<ProjectExportRow> Build(
        IReadOnlyList<Project> projects,
        IReadOnlyList<Placement> placements)
    {
        var matchedByProject = placements
            .Where(placement =>
                placement.Status == PlacementStatus.Confirmed &&
                placement.StudentUser is not null)
            .GroupBy(placement => placement.ProjectId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(placement => placement.StudentUser!.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList());

        var rows = new List<ProjectExportRow>();
        foreach (var project in projects)
        {
            if (!matchedByProject.TryGetValue(project.Id, out var students) || students.Count == 0)
            {
                rows.Add(ToRow(project, null));
                continue;
            }

            foreach (var placement in students)
            {
                rows.Add(ToRow(project, placement));
            }
        }

        return rows;
    }

    private static ProjectExportRow ToRow(Project project, Placement? placement) =>
        new(
            project.Title,
            project.Status.ToString(),
            project.Semester?.Name ?? "",
            Join(project.ResearchAreas),
            ProjectMappings.ToLabel(project.IrbStage),
            project.BriefDescription,
            Join(project.ActivityTypes),
            project.VolunteersRequired.ToString(CultureInfo.InvariantCulture),
            project.VolunteersFilled.ToString(CultureInfo.InvariantCulture),
            project.MinQualifications ?? "",
            project.AdditionalComments ?? "",
            project.CreatedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            project.FacultyNameSnapshot,
            project.EmailSnapshot,
            project.AffiliationSnapshot,
            project.UserNameSnapshot ?? "",
            placement?.StudentUser.Name ?? "",
            placement?.StudentUser.Email ?? "",
            placement?.StudentUser.UserName ?? "");

    private static string Join(IEnumerable<string>? values) =>
        values is null
            ? ""
            : string.Join(", ", values.Where(value => !string.IsNullOrWhiteSpace(value)));
}
