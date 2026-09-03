using Microsoft.AspNetCore.Http;

namespace FEA.URVP.Tests.Files;

internal static class FormFileFactory
{
    public static IFormFile Create(
        byte[] content,
        string fileName,
        string contentType = "application/octet-stream")
    {
        var stream = new MemoryStream(content, writable: false);
        return new FormFile(stream, 0, content.LongLength, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };
    }

    public static IFormFile CreateWithDeclaredLength(
        string fileName,
        long length,
        string contentType = "application/octet-stream")
    {
        var stream = new MemoryStream([0x01]);
        return new FormFile(stream, 0, length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType,
        };
    }
}

internal static class SampleFiles
{
    public static readonly byte[] Pdf = "%PDF-1.4\ntrailer\n%%EOF\n"u8.ToArray();

    public static readonly byte[] Jpeg =
    [
        0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x00,
    ];

    public static readonly byte[] Png =
    [
        0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
    ];

    public static readonly byte[] Gif =
    [
        0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00,
    ];

    public static readonly byte[] Exe =
    [
        0x4D, 0x5A, 0x90, 0x00, 0x03, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
    ];

    public static readonly byte[] UnknownBinary =
    [
        0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09,
    ];
}
