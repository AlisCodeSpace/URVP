using System.Text.Json.Serialization;
using FEA.URVP.Application.DTOs.Matching;
using MediatR;

namespace FEA.URVP.Application.Commands.Matching.Run;

/// <summary>
/// Executes a dry run of the matching algorithm and stores it as a draft.
/// Any earlier draft for the same semester is discarded.
/// </summary>
public sealed class RunMatchingCommand : IRequest<MatchingRunDetailDto>
{
    [JsonIgnore]
    public Guid CurrentUserId { get; set; }

    /// <summary>Semester to match for; defaults to the active semester.</summary>
    public Guid? SemesterId { get; init; }

    /// <summary>Lottery seed for reproducibility; a random seed is used when omitted.</summary>
    public int? Seed { get; init; }
}
