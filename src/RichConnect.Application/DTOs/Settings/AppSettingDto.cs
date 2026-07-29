namespace RICHConnect.Backend.Application.DTOs.Settings
{
    /// <summary>
    /// DTO for an application setting. Value may be masked (e.g. "********") when IsSecret and not revealing.
    /// </summary>
    public class AppSettingDto
    {
        public string Key { get; set; } = null!;

        /// <summary>
        /// Setting value, or mask (e.g. "********") when IsSecret and includeSecretValues/reveal is false.
        /// </summary>
        public string Value { get; set; } = null!;

        public bool IsSecret { get; set; }

        public string? Category { get; set; }

        public string? Description { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Optional display email of the user who last updated (when loaded with user info).
        /// </summary>
        public string? UpdatedByEmail { get; set; }
    }
}
