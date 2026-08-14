using FEA.URVP.Application.DTOs.Projects;
using FEA.URVP.Domain.Entities.Projects;
using FEA.URVP.Domain.Enums;

namespace FEA.URVP.Application.Mappings;

public static class ProjectMappings
{
    public static ProjectDto ToDto(this Project project) => new()
    {
        Id = project.Id,
        CreatedByUserId = project.CreatedByUserId,
        Title = project.Title,
        ResearchAreas = project.ResearchAreas.ToList(),
        IrbStage = project.IrbStage,
        IrbStageLabel = ToLabel(project.IrbStage),
        BriefDescription = project.BriefDescription,
        ActivityTypes = project.ActivityTypes.ToList(),
        VolunteersRequired = project.VolunteersRequired,
        VolunteersFilled = project.VolunteersFilled,
        MinQualifications = project.MinQualifications,
        AdditionalComments = project.AdditionalComments,
        Status = project.Status,
        FacultyName = project.FacultyNameSnapshot,
        Affiliation = project.AffiliationSnapshot,
        Email = project.EmailSnapshot,
        UserName = project.UserNameSnapshot,
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt
    };

    public static AdminProjectListItemDto ToAdminListItem(this Project project, int rankingCount) => new()
    {
        Id = project.Id,
        Title = project.Title,
        FacultyName = project.FacultyNameSnapshot,
        Affiliation = project.AffiliationSnapshot,
        Email = project.EmailSnapshot,
        Status = project.Status,
        VolunteersRequired = project.VolunteersRequired,
        VolunteersFilled = project.VolunteersFilled,
        RankingCount = rankingCount,
        CreatedAt = project.CreatedAt,
        UpdatedAt = project.UpdatedAt
    };

    public static string ToLabel(IrbStage value) => value switch
    {
        IrbStage.IrbApproved => "IRB Approved",
        IrbStage.IrbApplicationInPreparation => "IRB Application in Preparation",
        IrbStage.IrbApplicationSubmitted => "IRB Application Submitted",
        IrbStage.DoesNotNeedIrbApproval => "Does not need IRB Approval",
        _ => value.ToString()
    };
}
