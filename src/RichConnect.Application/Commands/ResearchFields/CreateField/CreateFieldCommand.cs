using MediatR;
using RICHConnect.Backend.Application.DTOs.Themes;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.CreateField
{
    public class CreateFieldCommand : IRequest<ResearchFieldDto>
    {
        // Required properties
        public string Name { get; set; } = string.Empty;
        public Guid SubmittedBy { get; set; }
        
        // Optional properties
        public string? Category { get; set; }
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        
        // Admin-specific properties
        public bool IsAdminCreated { get; set; }
        
        public CreateFieldCommand()
        {
            // Default constructor for deserialization
        }
        
        public CreateFieldCommand(string name, Guid submittedBy, string? category = null, 
            int displayOrder = 0, bool isActive = true, bool isAdminCreated = false)
        {
            Name = name;
            SubmittedBy = submittedBy;
            Category = category;
            DisplayOrder = displayOrder;
            IsActive = isActive;
            IsAdminCreated = isAdminCreated;
        }
    }
}

