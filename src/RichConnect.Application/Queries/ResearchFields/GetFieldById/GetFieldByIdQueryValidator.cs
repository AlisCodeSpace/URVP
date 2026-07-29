using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.ResearchFields.GetFieldById
{
    public class GetFieldByIdQueryValidator : AbstractValidator<GetFieldByIdQuery>
    {
        public GetFieldByIdQueryValidator()
        {
            RuleFor(query => query.FieldId)
                .NotEqual(Guid.Empty)
                .WithMessage("A valid research field ID is required.");
        }
    }
}

