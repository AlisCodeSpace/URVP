using FEA.URVP.Application.Commands.FacultyCandidateRankings.Upsert;
using FEA.URVP.Domain.Entities.FacultyCandidateRankings;
using FluentValidation;

namespace FEA.URVP.Application.Validators.FacultyCandidateRankings;

public sealed class UpsertFacultyCandidateRankingCommandValidator
    : AbstractValidator<UpsertFacultyCandidateRankingCommand>
{
    public UpsertFacultyCandidateRankingCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("Project is required.");

        RuleFor(x => x.StudentUserId)
            .NotEmpty()
            .WithMessage("Student is required.");

        RuleFor(x => x.Rank)
            .InclusiveBetween(FacultyCandidateRanking.MinRank, FacultyCandidateRanking.MaxRank)
            .WithMessage($"Rank must be between {FacultyCandidateRanking.MinRank} and {FacultyCandidateRanking.MaxRank}.");
    }
}
