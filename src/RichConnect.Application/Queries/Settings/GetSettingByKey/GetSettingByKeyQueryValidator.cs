using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Settings.GetSettingByKey
{
    /// <summary>
    /// Validator for GetSettingByKeyQuery.
    /// </summary>
    public class GetSettingByKeyQueryValidator : AbstractValidator<GetSettingByKeyQuery>
    {
        public GetSettingByKeyQueryValidator()
        {
            RuleFor(x => x.Key)
                .NotEmpty().WithMessage("Key is required.");
        }
    }
}
