namespace FEA.URVP.Domain.Catalog;

public static class FileStorageCatalog
{
    public const string EntityStudentProfile = "StudentProfile";
    public const string EntityWorkshop = "Workshop";

    public const string CategoryTranscript = "Transcript";
    public const string CategoryCitiCertification = "CitiCertification";
    public const string CategoryPoster = "Poster";

    public const long MaxDocumentBytes = 10 * 1024 * 1024; // 10 MB
    public const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB
    public const long MaxUploadBytes = MaxDocumentBytes;

    public static readonly IReadOnlySet<string> EntityTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        EntityStudentProfile,
        EntityWorkshop,
    };

    public static readonly IReadOnlySet<string> DocumentCategories = new HashSet<string>(StringComparer.Ordinal)
    {
        CategoryTranscript,
        CategoryCitiCertification,
    };

    public static readonly IReadOnlySet<string> ImageCategories = new HashSet<string>(StringComparer.Ordinal)
    {
        CategoryPoster,
    };

    public static readonly IReadOnlySet<string> AllowedPdfExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf",
    };

    public static readonly IReadOnlySet<string> AllowedPdfMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
    };

    public static readonly IReadOnlySet<string> AllowedImageExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp",
    };

    public static readonly IReadOnlySet<string> AllowedImageMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp",
    };

    public static bool IsImageCategory(string category) =>
        ImageCategories.Contains(category);

    public static bool IsPublicFile(string entityType, string fileCategory) =>
        entityType == EntityWorkshop && fileCategory == CategoryPoster;
}
