using FluentValidation;
using RICHConnect.Backend.Infrastructure.Data.Repositories.ResearchFields.Interfaces;
using RICHConnect.Backend.Domain.Enums;

namespace RICHConnect.Backend.Application.Commands.ResearchFields.ApproveField
{
    public class ApproveFieldCommandValidator : AbstractValidator<ApproveFieldCommand>
    {
        private readonly IResearchFieldRepository _repository;

        public ApproveFieldCommandValidator(IResearchFieldRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));

            RuleFor(cmd => cmd.FieldId)
                .NotEqual(Guid.Empty)
                .WithMessage("A valid research field ID is required.")
                .MustAsync(async (id, cancellation) => await _repository.ExistsAsync(id))
                .WithMessage("The specified research field does not exist.");

            RuleFor(cmd => cmd.ApprovedBy)
                .NotEqual(Guid.Empty)
                .WithMessage("A valid approver ID is required.");

            RuleFor(cmd => cmd.FieldId)
                .MustAsync(async (id, cancellation) => {
                    var field = await _repository.GetByIdAsync(id);
                    return field != null && field.Status == ApprovalStatus.Pending;
                })
                .WithMessage("Only pending research fields can be approved.");
        }
    }
}

