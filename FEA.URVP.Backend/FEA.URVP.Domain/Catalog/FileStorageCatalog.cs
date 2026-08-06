namespace FEA.URVP.Domain.Catalog;

public static class FileStorageCatalog
{
    public const string EntityStudentProfile = "StudentProfile";

    public const string CategoryTranscript = "Transcript";
    public const string CategoryCitiCertification = "CitiCertification";

    public const long MaxDocumentBytes = 10 * 1024 * 1024; // 10 MB

    public static readonly IReadOnlySet<string> EntityTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        EntityStudentProfile,
    };

    public static readonly IReadOnlySet<string> DocumentCategories = new HashSet<string>(StringComparer.Ordinal)
    {
        CategoryTranscript,
        CategoryCitiCertification,
    };

    public static readonly IReadOnlySet<string> AllowedPdfExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
    };

    public static readonly IReadOnlySet<string> AllowedPdfMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
    };
}
