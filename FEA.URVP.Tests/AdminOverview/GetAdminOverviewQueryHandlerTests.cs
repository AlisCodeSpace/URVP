using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Queries.AdminOverview;
using NSubstitute;

namespace FEA.URVP.Tests.AdminOverview;

public sealed class GetAdminOverviewQueryHandlerTests
{
    [Fact]
    public async Task Maps_the_repository_snapshot_through_the_assembler()
    {
        var snapshot = new AdminOverviewSnapshot
        {
            UtcNow = new DateTime(2026, 9, 6, 16, 0, 0, DateTimeKind.Utc),
            Students = 12,
            StudentProfiles = 9,
            OpenProjects = 3,
            ConfirmedPlacements = 2,
        };

        var repo = Substitute.For<IAdminOverviewReadRepository>();
        repo.GetSnapshotAsync(Arg.Any<CancellationToken>()).Returns(snapshot);

        var handler = new GetAdminOverviewQueryHandler(repo);
        var dto = await handler.Handle(new GetAdminOverviewQuery(), CancellationToken.None);

        Assert.Equal(12, dto.Accounts.Students);
        Assert.Equal(9, dto.Pipeline.Single(s => s.Id == "profiles").Count);
        Assert.Equal(3, dto.Projects.Open);
        Assert.Equal(2, dto.Matching.ConfirmedPlacements);
        await repo.Received(1).GetSnapshotAsync(Arg.Any<CancellationToken>());
    }
}
