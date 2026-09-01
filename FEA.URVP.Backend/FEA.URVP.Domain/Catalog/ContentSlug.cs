using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FEA.URVP.Domain.Catalog;

public static class ContentSlug
{
    private static readonly Regex NonSlug = new("[^a-z0-9]+", RegexOptions.Compiled);

    public static string FromTitle(string title)
    {
        var normalized = title.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        var slug = NonSlug.Replace(builder.ToString().Normalize(NormalizationForm.FormC), "-")
            .Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? "item" : slug;
    }
}
