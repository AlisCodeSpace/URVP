namespace RICHConnect.Backend.Application.DTOs.Auth
{
    /// <summary>
    /// DTO for authentication status response
    /// </summary>
    public class AuthStatusResponseDto
    {
        public bool IsAuthenticated { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? Name { get; set; }
        public int? Role { get; set; }
        public string? ProfileImageUrl { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public string? AuthenticationScheme { get; set; }
        public string? Error { get; set; }
    }
}
