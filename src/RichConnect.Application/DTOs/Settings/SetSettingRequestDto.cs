namespace RICHConnect.Backend.Application.DTOs.Settings
{
    /// <summary>
    /// Request body for PUT api/Settings/{key}. Key is supplied in the route.
    /// </summary>
    public class SetSettingRequestDto
    {
        public string Value { get; set; } = null!;
        public bool IsSecret { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
    }
}
