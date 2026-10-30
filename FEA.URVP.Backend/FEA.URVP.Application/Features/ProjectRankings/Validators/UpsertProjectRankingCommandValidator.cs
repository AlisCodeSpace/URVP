using FEA.URVP.Application.Features.ProjectRankings.Commands.Upsert;
using FEA.URVP.Domain.Entities.ProjectRankings;
using FluentValidation;

namespace FEA.URVP.Application.Features.ProjectRankings.Validators;

public sealed class UpsertProjectRankingCommandValidator
    : AbstractValidator<UpsertProjectRankingCommand>
{
    public UpsertProjectRankingCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project is required.");

        RuleFor(x => x.Rank)
            .InclusiveBetween(ProjectRanking.MinRank, ProjectRanking.MaxRank)
            .WithMessage($"Rank must be between {ProjectRanking.MinRank} and {ProjectRanking.MaxRank}.");
    }
}
