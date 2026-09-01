using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Projects.Update;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Enums;
using FluentValidation;

namespace FEA.URVP.Application.Validators.Projects;

public sealed class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator(IValueListRepository valueLists)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.ResearchAreas)
            .NotEmpty().WithMessage("Select at least one research area.")
            .Must(areas => areas.Count <= ResearchAreaCatalog.MaxSelections)
            .WithMessage($"Select at most {ResearchAreaCatalog.MaxSelections} research areas.")
            .Must(areas => areas.Distinct(StringComparer.Ordinal).Count() == areas.Count)
            .WithMessage("Research areas must be unique.")
            .MustAsync(async (areas, ct) =>
            {
                var allowed = await valueLists.GetActiveNamesAsync(ValueListKind.ResearchInterest, ct);
                return areas.All(allowed.Contains);
            })
            .WithMessage("One or more research areas are not allowed.");

        RuleFor(x => x.IrbStage)
            .IsInEnum();

        RuleFor(x => x.BriefDescription)
            .NotEmpty().WithMessage("Brief description is required.")
            .MaximumLength(4000);

        RuleFor(x => x.ActivityTypes)
            .NotEmpty().WithMessage("Select at least one research activity type.")
            .Must(types => types.Count <= ResearchActivityTypeCatalog.MaxSelections)
            .WithMessage($"Select at most {ResearchActivityTypeCatalog.MaxSelections} activity types.")
            .Must(types => types.Distinct(StringComparer.Ordinal).Count() == types.Count)
            .WithMessage("Activity types must be unique.")
            .MustAsync(async (types, ct) =>
            {
                var allowed = await valueLists.GetActiveNamesAsync(
                    ValueListKind.ResearchActivityType,
                    ct);
                return types.All(allowed.Contains);
            })
            .WithMessage("One or more activity types are not allowed.");

        RuleFor(x => x.VolunteersRequired)
            .GreaterThanOrEqualTo(1);

        RuleFor(x => x.MinQualifications)
            .MaximumLength(2000)
            .When(x => x.MinQualifications is not null);

        RuleFor(x => x.AdditionalComments)
            .MaximumLength(2000)
            .When(x => x.AdditionalComments is not null);

        RuleFor(x => x.Affiliation)
            .MaximumLength(256)
            .When(x => x.Affiliation is not null);

        RuleFor(x => x.UserName)
            .MaximumLength(64)
            .When(x => x.UserName is not null);

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
