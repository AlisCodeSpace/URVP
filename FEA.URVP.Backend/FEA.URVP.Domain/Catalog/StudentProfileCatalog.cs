namespace FEA.URVP.Domain.Catalog;

/// <summary>
/// Allowed option catalogs for student profile fields.
/// </summary>
public static class StudentProfileCatalog
{
    public const int MaxLanguages = 8;
    public const int MaxResearchTopics = 6;

    public static readonly IReadOnlySet<string> Genders = new HashSet<string>(StringComparer.Ordinal)
    {
        "Female",
        "Male",
    };

    public static readonly IReadOnlySet<string> Degrees = new HashSet<string>(StringComparer.Ordinal)
    {
        "BA",
        "BS",
        "BBA",
        "BEng",
        "BArch",
        "Other",
    };

    /// <summary>
    /// Faculties and schools listed on aub.edu.lb/academics/Pages/faculties.aspx.
    /// </summary>
    public static readonly IReadOnlySet<string> Faculties = new HashSet<string>(StringComparer.Ordinal)
    {
        "Faculty of Agricultural and Food Sciences (FAFS)",
        "Faculty of Arts and Sciences (FAS)",
        "Faculty of Health Sciences (FHS)",
        "Faculty of Medicine (FM)",
        "Rafic Hariri School of Nursing (HSON)",
        "Maroun Semaan Faculty of Engineering and Architecture (MSFEA)",
        "School of Computing and Data Sciences (SCDS)",
        "Suliman S. Olayan School of Business (OSB)",
    };

    public static readonly IReadOnlySet<string> Languages = new HashSet<string>(StringComparer.Ordinal)
    {
        "Arabic",
        "English",
        "French",
        "Armenian",
        "German",
        "Spanish",
        "Italian",
        "Turkish",
        "Persian",
        "Russian",
        "Chinese",
        "Japanese",
        "Korean",
        "Portuguese",
        "Hindi",
    };

    public static readonly IReadOnlySet<string> Weekdays = new HashSet<string>(StringComparer.Ordinal)
    {
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday",
        "Sunday",
    };

    public static readonly IReadOnlySet<string> TimeSlots = new HashSet<string>(StringComparer.Ordinal)
    {
        "Morning (8:00–12:00)",
        "Afternoon (12:00–16:00)",
        "Evening (16:00–20:00)",
    };
}
