namespace FEA.URVP.Domain.Catalog;

/// <summary>
/// Allowed research activity-type labels for project posting (max 6 per project).
/// </summary>
public static class ResearchActivityTypeCatalog
{
    public const int MaxSelections = 6;

    public static readonly IReadOnlySet<string> Allowed = new HashSet<string>(StringComparer.Ordinal)
    {
        "3D Modelling",
        "Achiral Research",
        "Archeological Field Work",
        "Cataloging",
        "Coding",
        "Conducting a Case Study",
        "Conducting a Survey",
        "Data Collection",
        "Data Entry",
        "Data Visualization",
        "Data analysis - geospatial",
        "Data analysis - qualitative",
        "Data analysis - quantitative",
        "Data management (includes data documentation)",
        "Device development",
        "Experimental/Wet Lab work",
        "Field work/Data Collection",
        "Image Search",
        "Instrumentation",
        "Interview Transcriptions",
        "Literature Search",
        "Logo Design",
        "Manuscript Writing",
        "Meta-analysis",
        "Online Research on Databases",
        "Participatory-Action research",
        "Photography",
        "Poster Preparation",
        "Programming",
        "Project Management",
        "Proposal Writing",
        "Reports Writing",
        "Research Dissemination (Manuscript Writing; Conference Presentation)",
        "Research dissemination - creating presentations",
        "Researching and evaluating software",
        "Researching methodologies",
        "Researching theories and conceptual frameworks",
        "Simulation",
        "Statistical analysis",
        "Systematic review",
        "Theater",
        "Theoretical Work",
        "Transcription",
        "Translating",
        "Writing a Literature Review",
        "Writing a Research Proposal",
        "digital communication",
        "web design",
        "website development",
    };
}
