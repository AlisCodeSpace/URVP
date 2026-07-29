namespace RICHConnect.Backend.Application.DTOs.Matching
{
    /// <summary>
    /// DTO for finalizing challenge matching
    /// </summary>
    public class MatchFinalizeDto
    {
        public Guid ChallengeId { get; set; }
        public List<Guid> MatchedFacultySpecialistIds { get; set; } = new();
        public int TotalMatchesCreated { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
