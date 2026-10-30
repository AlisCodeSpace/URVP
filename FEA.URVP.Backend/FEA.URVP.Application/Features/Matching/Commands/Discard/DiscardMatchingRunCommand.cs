using FEA.URVP.Application.Features.Matching.DTOs;
using MediatR;

namespace FEA.URVP.Application.Features.Matching.Commands.Discard;

/// <summary>Rejects a draft run; its proposed placements are voided.</summary>
public sealed record DiscardMatchingRunCommand(Guid RunId) : IRequest<MatchingRunDto>;
