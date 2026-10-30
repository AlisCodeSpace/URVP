using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Features.Projects.DTOs;
using FEA.URVP.Application.Features.Projects.Mappings;
using MediatR;

namespace FEA.URVP.Application.Features.Projects.Queries.ListAssistedProjects;

public sealed class ListAssistedProjectsQueryHandler
    : IRequestHandler<ListAssistedProjectsQuery, IReadOnlyList<AssistedProjectDto>>
{
    private readonly IUserRepository _users;
    private readonly IProjectResearchAssistantRepository _assistants;

    public ListAssistedProjectsQueryHandler(
        IUserRepository users,
        IProjectResearchAssistantRepository assistants)
    {
        _users = users;
        _assistants = assistants;
    }

    public async Task<IReadOnlyList<AssistedProjectDto>> Handle(
        ListAssistedProjectsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CurrentUserId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Authenticated user is required.");
        }

        var user = await _users.FindByIdAsync(request.CurrentUserId, cancellationToken)
            ?? throw new UnauthorizedAccessException("User not found.");

        var entries = await _assistants.ListByEmailAsync(user.Email, cancellationToken);
        return entries.Select(entry => entry.ToAssistedDto()).ToList();
    }
}
