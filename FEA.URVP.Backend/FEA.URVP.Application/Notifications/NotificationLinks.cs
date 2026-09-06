namespace FEA.URVP.Application.Notifications;

internal static class NotificationLinks
{
    public static string Project(Guid projectId) => $"/projects/detail?id={projectId}";

    public static string FacultyProject(Guid ownerUserId, Guid projectId) =>
        $"/my-projects/project?user={ownerUserId}&project={projectId}";

    public static string FacultyProjects(Guid ownerUserId) => $"/my-projects?user={ownerUserId}";

    public static string AdminProject(Guid projectId) => $"/admin/projects/detail?id={projectId}";

    public static string NewsArticle(string slug) => $"/news/article?slug={Uri.EscapeDataString(slug)}";

    public const string Workshops = "/workshops";

    public const string StudentProjects = "/student/projects";

    public const string Projects = "/projects";

    public const string AdminUsers = "/admin/users";
}
