namespace FEA.URVP.Domain.Catalog;

public static class WorkshopSeedCatalog
{
    public sealed record SeedWorkshop(
        string Title,
        string Date,
        string? Time,
        string? Location,
        string Description,
        string RegistrationUrl);

    public static readonly IReadOnlyList<SeedWorkshop> Items =
    [
        new(
            "How to Write a Strong Research Profile",
            "Sep 5, 2025",
            "4:00 – 5:00 PM",
            "Online · Zoom",
            "Learn how to present your interests, skills, and experience so faculty can quickly see why you’re a strong match for their project.",
            "https://forms.gle/urvp-workshop-profile-placeholder"),
        new(
            "Meeting Your PI: First Steps",
            "Sep 12, 2025",
            "4:00 – 5:00 PM",
            "West Hall · Auditorium B",
            "What to expect in your first lab meeting, how to set a weekly cadence, and how to ask productive questions once you’re matched.",
            "https://forms.gle/urvp-workshop-pi-placeholder"),
        new(
            "Research Ethics & Mentorship",
            "Oct 3, 2025",
            "3:30 – 5:00 PM",
            "Online · Zoom",
            "Core practices for responsible research, authorship conversations, and building a healthy mentor–mentee relationship during your placement.",
            "https://forms.gle/urvp-workshop-ethics-placeholder"),
    ];
}
