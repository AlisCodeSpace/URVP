using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Queries.Users.Export;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Enums;
using NSubstitute;

namespace FEA.URVP.Tests.Users;

public sealed class ExportUsersQueryHandlerTests
{
    [Fact]
    public async Task Pdf_export_returns_a_pdf_file()
    {
        var file = await Handle("pdf");

        Assert.Equal(ExportUsersQueryHandler.PdfMime, file.MimeType);
        Assert.EndsWith(".pdf", file.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(file.Content), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("xlsx")]
    [InlineData("excel")]
    [InlineData("Excel")]
    public async Task Excel_export_returns_a_workbook(string format)
    {
        var file = await Handle(format);

        Assert.Equal(ExportUsersQueryHandler.ExcelMime, file.MimeType);
        Assert.EndsWith(".xlsx", file.FileName, StringComparison.OrdinalIgnoreCase);
        Assert.Equal((byte)'P', file.Content[0]);
        Assert.Equal((byte)'K', file.Content[1]);
    }

    [Fact]
    public async Task Unknown_format_is_rejected()
    {
        var handler = new ExportUsersQueryHandler(Users());

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => handler.Handle(new ExportUsersQuery("csv"), CancellationToken.None));

        Assert.Contains("pdf or excel", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Search_and_role_filters_are_forwarded_to_the_repository()
    {
        var users = Users();
        var handler = new ExportUsersQueryHandler(users);

        await handler.Handle(
            new ExportUsersQuery(
                "xlsx",
                "ada",
                UserRole.Student,
                UserSortField.Email,
                SortDirection.Desc),
            CancellationToken.None);

        await users.Received(1).ListAllAsync(
            "ada",
            UserRole.Student,
            UserSortField.Email,
            SortDirection.Desc,
            Arg.Any<CancellationToken>());
    }

    private static async Task<FEA.URVP.Application.DTOs.Users.UserExportFileDto> Handle(string format)
    {
        var handler = new ExportUsersQueryHandler(Users());
        return await handler.Handle(new ExportUsersQuery(format), CancellationToken.None);
    }

    private static IUserRepository Users()
    {
        var users = Substitute.For<IUserRepository>();
        users.ListAllAsync(
            Arg.Any<string?>(),
            Arg.Any<UserRole?>(),
            Arg.Any<UserSortField>(),
            Arg.Any<SortDirection>(),
            Arg.Any<CancellationToken>()).Returns([
            new User
            {
                Email = "aa624@mail.aub.edu",
                Name = "Ada",
                UserName = "aa624",
                Affiliation = "AUB",
                Role = UserRole.Student,
                RegisteredAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ]);
        return users;
    }
}
