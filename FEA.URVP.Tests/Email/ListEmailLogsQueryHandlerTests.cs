using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Queries.EmailLogs.List;
using FEA.URVP.Domain.Entities.Notifications;
using NSubstitute;

namespace FEA.URVP.Tests.Email;

public sealed class ListEmailLogsQueryHandlerTests
{
    [Fact]
    public async Task Maps_log_fields_newest_first()
    {
        var createdOn = new DateTime(2026, 9, 10, 11, 4, 0, DateTimeKind.Utc);
        var logs = new List<EmailLog>
        {
            new()
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                From = "urvp-system@aub.edu.lb",
                To = "student@urvp.com",
                Cc = "faculty@urvp.com",
                Body = "<p>Welcome</p>",
                Exception = "SMTP send failed",
                Success = false,
                CreatedOn = createdOn,
            },
        };

        var repo = Substitute.For<IEmailLogRepository>();
        repo.ListAsync(1, 50, Arg.Any<CancellationToken>()).Returns((logs, logs.Count));

        var handler = new ListEmailLogsQueryHandler(repo);
        var (items, totalCount) = await handler.Handle(new ListEmailLogsQuery(1, 50), CancellationToken.None);

        Assert.Equal(1, totalCount);
        var dto = Assert.Single(items);
        Assert.Equal("urvp-system@aub.edu.lb", dto.From);
        Assert.Equal("student@urvp.com", dto.To);
        Assert.Equal("faculty@urvp.com", dto.Cc);
        Assert.Equal("SMTP send failed", dto.Exception);
        Assert.False(dto.Success);
        Assert.Equal(createdOn, dto.CreatedOn);
    }
}
