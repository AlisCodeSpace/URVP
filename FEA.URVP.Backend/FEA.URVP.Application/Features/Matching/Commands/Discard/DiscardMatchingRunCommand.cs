using FEA.URVP.Application.DTOs.Matching;
using MediatR;

namespace FEA.URVP.Application.Commands.Matching.Discard;

/// <summary>Rejects a draft run; its proposed placements are voided.</summary>
public sealed record DiscardMatchingRunCommand(Guid RunId) : IRequest<MatchingRunDto>;
