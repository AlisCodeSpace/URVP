using MediatR;
using RICHConnect.Backend.Application.DTOs.Themes;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.UpdateField
{
    public class UpdateFieldCommand : IRequest<ResearchFieldDto>
    {
        // Required properties
        public Guid FieldId { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid UpdatedBy { get; set; }
        
        // Optional properties
        public string? Category { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        
        public UpdateFieldCommand()
        {
            // Default constructor for deserialization
        }
        
        public UpdateFieldCommand(
            Guid fieldId, 
            string name, 
            Guid updatedBy, 
            string? category = null, 
            int displayOrder = 0, 
            bool isActive = true)
        {
            FieldId = fieldId;
            Name = name;
            UpdatedBy = updatedBy;
            Category = category;
            DisplayOrder = displayOrder;
            IsActive = isActive;
        }
    }
}

