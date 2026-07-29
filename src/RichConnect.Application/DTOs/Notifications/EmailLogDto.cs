namespace RICHConnect.Backend.Application.DTOs.Notifications
{
    public class EmailLogDto
    {
        public Guid Id { get; set; }
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Exception { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
