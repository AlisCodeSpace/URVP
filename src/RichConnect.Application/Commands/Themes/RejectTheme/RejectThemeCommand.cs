using MediatR;
using RICHConnect.Backend.Domain.Entities.Themes;

namespace RICHConnect.Backend.Application.Commands.Themes.RejectTheme
{
    public class RejectThemeCommand : IRequest<ResearchTheme>
    {
        public Guid ThemeId { get; set; }
        public Guid RejectedBy { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
        
        public RejectThemeCommand()
        {
            // Default constructor for deserialization
        }
        
        public RejectThemeCommand(Guid themeId, Guid rejectedBy, string rejectionReason)
        {
            ThemeId = themeId;
            RejectedBy = rejectedBy;
            RejectionReason = rejectionReason;
        }
    }
}
