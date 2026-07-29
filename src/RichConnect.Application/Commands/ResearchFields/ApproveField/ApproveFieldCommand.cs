using MediatR;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.ApproveField
{
    public class ApproveFieldCommand : IRequest<bool>
    {
        public Guid FieldId { get; }
        public Guid ApprovedBy { get; }

        public ApproveFieldCommand(Guid fieldId, Guid approvedBy)
        {
            FieldId = fieldId;
            ApprovedBy = approvedBy;
        }
    }
}

