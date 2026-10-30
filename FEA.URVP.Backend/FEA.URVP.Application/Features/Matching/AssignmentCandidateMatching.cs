using System.Text.RegularExpressions;

namespace FEA.URVP.Application.Matching;

/// <summary>
/// Compares a project's research areas and written qualifications with a student profile.
/// </summary>
public static class AssignmentCandidateMatching
{
    public sealed record Fit(
        IReadOnlyList<string> MatchedResearchTopics,
        bool QualificationsMentionMajor,
        bool QualificationsMentionFaculty)
    {
        public bool IsRecommended =>
            MatchedResearchTopics.Count > 0
            || QualificationsMentionMajor
            || QualificationsMentionFaculty;

        public int Score =>
            MatchedResearchTopics.Count * 10
            + (QualificationsMentionMajor ? 3 : 0)
            + (QualificationsMentionFaculty ? 2 : 0);
    }

    public static Fit Evaluate(
        IEnumerable<string> researchAreas,
        string? minQualifications,
        IEnumerable<string>? researchTopics,
        string? major,
        string? faculty)
    {
        var areas = new HashSet<string>(
            researchAreas.Select(Normalize).Where(area => area.Length > 0),
            StringComparer.OrdinalIgnoreCase);

        var matched = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var topic in researchTopics ?? [])
        {
            var label = topic.Trim();
            if (label.Length == 0 || !areas.Contains(label) || !seen.Add(label))
            {
                continue;
            }

            matched.Add(label);
        }

        return new Fit(
            matched,
            Mentions(minQualifications, major),
            Mentions(minQualifications, faculty));
    }

    private static bool Mentions(string? qualifications, string? phrase)
    {
        if (string.IsNullOrWhiteSpace(qualifications) || string.IsNullOrWhiteSpace(phrase))
        {
            return false;
        }

        var needle = phrase.Trim();
        if (needle.Length < 3)
        {
            return false;
        }

        return Regex.IsMatch(
            qualifications,
            $@"(?<!\p{{L}}){Regex.Escape(needle)}(?!\p{{L}})",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static string Normalize(string value) => value.Trim();
}
