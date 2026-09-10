namespace FEA.URVP.Domain.Catalog;

public static class FileStorageCatalog
{
    public const string EntityStudentProfile = "StudentProfile";
    public const string EntityWorkshop = "Workshop";
    public const string EntityNewsArticle = "NewsArticle";

    public const string CategoryTranscript = "Transcript";
    public const string CategoryCv = "Cv";
    public const string CategoryPoster = "Poster";
    public const string CategoryNewsImage = "NewsImage";

    public const int MaxNewsImages = 12;

    public const long MaxDocumentBytes = 10 * 1024 * 1024; // 10 MB (SQL check constraint)
    public const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB (SQL check constraint)
    public const long MaxTotalSizeBytes = 25 * 1024 * 1024; // 25 MB — keep aligned with FileStorage:MaxTotalSizeBytes
    public const long MaxUploadBytes = MaxTotalSizeBytes;

    public static readonly IReadOnlySet<string> EntityTypes = new HashSet<string>(StringComparer.Ordinal)
    {
        EntityStudentProfile,
        EntityWorkshop,
        EntityNewsArticle,
    };

    public static readonly IReadOnlySet<string> DocumentCategories = new HashSet<string>(StringComparer.Ordinal)
    {
        CategoryTranscript,
        CategoryCv,
    };

    public static readonly IReadOnlySet<string> ImageCategories = new HashSet<string>(StringComparer.Ordinal)
    {
        CategoryPoster,
        CategoryNewsImage,
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
        ".gif",
    };

    public static readonly IReadOnlySet<string> AllowedImageMimeTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/gif",
    };

    public static bool IsImageCategory(string category) =>
        ImageCategories.Contains(category);

    public static bool IsPublicFile(string entityType, string fileCategory) =>
        (entityType == EntityWorkshop && fileCategory == CategoryPoster)
        || (entityType == EntityNewsArticle && fileCategory == CategoryNewsImage);
}
