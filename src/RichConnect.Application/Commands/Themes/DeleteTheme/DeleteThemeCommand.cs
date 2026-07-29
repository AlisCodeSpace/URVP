using MediatR;

namespace RICHConnect.Backend.Application.Commands.Themes.DeleteTheme
{
    public class DeleteThemeCommand : IRequest<bool>
    {
        public Guid ThemeId { get; set; }
        public Guid DeletedBy { get; set; }
        
        public DeleteThemeCommand()
        {
            // Default constructor for deserialization
        }
        
        public DeleteThemeCommand(Guid themeId, Guid deletedBy)
        {
            ThemeId = themeId;
            DeletedBy = deletedBy;
        }
    }
}
