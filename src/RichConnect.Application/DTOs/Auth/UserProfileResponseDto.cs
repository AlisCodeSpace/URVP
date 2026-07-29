namespace RICHConnect.Backend.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for user profile response
    /// </summary>
    public class UserProfileResponseDto
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public int Role { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? AuthenticationScheme { get; set; }
        public bool HasProfile { get; set; }
        public int? ProfileStatus { get; set; }
        public string? Error { get; set; }
    }
}
