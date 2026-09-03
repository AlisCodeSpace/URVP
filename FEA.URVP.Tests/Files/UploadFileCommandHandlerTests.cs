using FEA.URVP.Application.Abstractions.Files;
using FEA.URVP.Application.Abstractions.Persistence;
using FEA.URVP.Application.Commands.Files.Upload;
using FEA.URVP.Application.Files;
using FEA.URVP.Application.Options;
using FEA.URVP.Domain.Catalog;
using FEA.URVP.Domain.Entities.Files;
using FEA.URVP.Domain.Entities.Users;
using FEA.URVP.Domain.Entities.Workshops;
using FEA.URVP.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace FEA.URVP.Tests.Files;

public sealed class UploadFileCommandHandlerTests
{
    [Fact]
    public async Task Detects_mime_once_and_stores_detected_type_not_client_content_type()
    {
        var userId = Guid.NewGuid();
        var file = FormFileFactory.Create(SampleFiles.Jpeg, "poster.jpg", "application/pdf");
        var mime = Substitute.For<IMimeTypeValidator>();
        mime.DetectMimeTypeAsync(Arg.Any<IFormFile>()).Returns(Task.FromResult<string?>("image/jpeg"));

        FileStorage? stored = null;
        var files = Substitute.For<IFileStorageRepository>();
        files.Add(Arg.Do<FileStorage>(item => stored = item));

        var workshops = Substitute.For<IWorkshopRepository>();
        workshops.FindByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(new Workshop
            {
                Title = "Workshop",
                Date = "2026-09-01",
                Description = "Desc",
                RegistrationUrl = "https://example.com",
            });

        var handler = CreateHandler(userId, UserRole.Admin, mime, files, workshops);

        var result = await handler.Handle(
            new UploadFileCommand
            {
                CurrentUserId = userId,
                EntityType = FileStorageCatalog.EntityWorkshop,
                EntityId = Guid.NewGuid(),
                FileCategory = FileStorageCatalog.CategoryPoster,
                File = file,
            },
            CancellationToken.None);

        Assert.Equal("image/jpeg", result.MimeType);
        Assert.Equal("image/jpeg", stored?.MimeType);
        Assert.NotEqual(file.ContentType, stored?.MimeType, StringComparer.OrdinalIgnoreCase);
        await mime.Received(1).DetectMimeTypeAsync(file);
        await mime.DidNotReceive().IsPdfAsync(Arg.Any<IFormFile>());
        await mime.DidNotReceive().IsImageAsync(Arg.Any<IFormFile>());
        await mime.DidNotReceive().ValidateMimeTypeAsync(Arg.Any<IFormFile>(), Arg.Any<string[]>());
    }

    [Fact]
    public async Task Real_detector_reads_content_after_a_single_detection()
    {
        var userId = Guid.NewGuid();
        var file = FormFileFactory.Create(SampleFiles.Pdf, "transcript.pdf", "text/plain");
        var files = Substitute.For<IFileStorageRepository>();
        FileStorage? stored = null;
        files.Add(Arg.Do<FileStorage>(item => stored = item));

        var handler = CreateHandler(userId, UserRole.Student, new MimeTypeValidator(), files);

        var result = await handler.Handle(
            new UploadFileCommand
            {
                CurrentUserId = userId,
                EntityType = FileStorageCatalog.EntityStudentProfile,
                EntityId = userId,
                FileCategory = FileStorageCatalog.CategoryTranscript,
                File = file,
            },
            CancellationToken.None);

        Assert.Equal("application/pdf", result.MimeType);
        Assert.Equal(SampleFiles.Pdf.Length, stored?.Content.Length);
        Assert.Equal(SampleFiles.Pdf, stored?.Content);
        Assert.NotEqual(file.ContentType, stored?.MimeType, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Rejects_pdf_whose_signature_is_an_executable()
    {
        var userId = Guid.NewGuid();
        var file = FormFileFactory.Create(SampleFiles.Exe, "transcript.pdf", "application/pdf");
        var mime = Substitute.For<IMimeTypeValidator>();
        mime.DetectMimeTypeAsync(Arg.Any<IFormFile>()).Returns(Task.FromResult<string?>("application/x-msdownload"));

        var handler = CreateHandler(userId, UserRole.Student, mime);

        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(
            new UploadFileCommand
            {
                CurrentUserId = userId,
                EntityType = FileStorageCatalog.EntityStudentProfile,
                EntityId = userId,
                FileCategory = FileStorageCatalog.CategoryTranscript,
                File = file,
            },
            CancellationToken.None));

        await mime.Received(1).DetectMimeTypeAsync(file);
    }

    private static UploadFileCommandHandler CreateHandler(
        Guid userId,
        UserRole role,
        IMimeTypeValidator mime,
        IFileStorageRepository? files = null,
        IWorkshopRepository? workshops = null)
    {
        files ??= Substitute.For<IFileStorageRepository>();
        workshops ??= Substitute.For<IWorkshopRepository>();
        var users = Substitute.For<IUserRepository>();
        users.FindByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(new User
        {
            Id = userId,
            Email = "student@aub.edu.lb",
            Name = "Student",
            UserName = "student",
            Affiliation = "FEA",
            Role = role,
        });

        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        return new UploadFileCommandHandler(
            NullLogger<UploadFileCommandHandler>.Instance,
            unitOfWork,
            files,
            users,
            workshops,
            mime,
            Microsoft.Extensions.Options.Options.Create(new FileStorageOptions()));
    }
}
