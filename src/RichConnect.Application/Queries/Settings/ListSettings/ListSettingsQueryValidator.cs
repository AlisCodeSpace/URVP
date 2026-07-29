using FluentValidation;

namespace RICHConnect.Backend.Application.Queries.Settings.ListSettings
{
    /// <summary>
    /// Optional validator for ListSettingsQuery (e.g. category max length).
    /// </summary>
    public class ListSettingsQueryValidator : AbstractValidator<ListSettingsQuery>
    {
        private const int CategoryMaxLength = 128;

        public ListSettingsQueryValidator()
        {
            RuleFor(x => x.Category)
                .MaximumLength(CategoryMaxLength).WithMessage($"Category filter cannot exceed {CategoryMaxLength} characters.")
                .When(x => !string.IsNullOrEmpty(x.Category));
        }
    }
}
