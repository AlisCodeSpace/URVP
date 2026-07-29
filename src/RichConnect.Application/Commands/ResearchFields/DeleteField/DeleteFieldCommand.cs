using MediatR;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.DeleteField
{
    public class DeleteFieldCommand : IRequest<bool>
    {
        public Guid FieldId { get; }
        public Guid DeletedBy { get; }

        public DeleteFieldCommand(Guid fieldId, Guid deletedBy)
        {
            FieldId = fieldId;
            DeletedBy = deletedBy;
        }
    }
}

