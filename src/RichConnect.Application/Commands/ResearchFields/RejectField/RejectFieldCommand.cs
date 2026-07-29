using MediatR;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.RejectField
{
    public class RejectFieldCommand : IRequest<bool>
    {
        public Guid FieldId { get; }
        public Guid RejectedBy { get; }
        public string RejectionReason { get; }

        public RejectFieldCommand(Guid fieldId, Guid rejectedBy, string rejectionReason)
        {
            FieldId = fieldId;
            RejectedBy = rejectedBy;
            RejectionReason = rejectionReason;
        }
    }
}

