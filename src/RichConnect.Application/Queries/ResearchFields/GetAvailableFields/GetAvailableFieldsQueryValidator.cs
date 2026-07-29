using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.ResearchFields.GetAvailableFields
{
    public class GetAvailableFieldsQueryValidator : AbstractValidator<GetAvailableFieldsQuery>
    {
        public GetAvailableFieldsQueryValidator()
        {
            RuleFor(query => query.UserId)
                .NotEqual(Guid.Empty)
                .WithMessage("A valid user ID is required.");
        }
    }
}

