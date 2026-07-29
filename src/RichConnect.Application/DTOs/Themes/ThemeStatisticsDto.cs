using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.DTOs.Themes
{
    /// <summary>
    /// DTO for theme statistics used in admin dashboard
    /// </summary>
    public class ThemeStatisticsDto
    {
        public int TotalThemes { get; set; }
        public int PendingThemes { get; set; }
        public int ApprovedThemes { get; set; }
        public int RejectedThemes { get; set; }
        public int ThemesThisMonth { get; set; }
        public int ThemesThisWeek { get; set; }
        public Dictionary<ApprovalStatus, int> StatusCounts { get; set; } = new();
        public Dictionary<Guid, int> ThemesByResearchField { get; set; } = new();
        public Dictionary<Guid, int> ThemesByUser { get; set; } = new();
        public List<ResearchThemeDto> RecentThemes { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
