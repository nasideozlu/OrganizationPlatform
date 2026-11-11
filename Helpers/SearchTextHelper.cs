using System.Globalization;
using System.Text;

namespace OrganizationPlatform.Helpers;

public static class SearchTextHelper
{
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value
            .ToLower(new CultureInfo("tr-TR"))
            .Normalize(NormalizationForm.FormD);

        var result = new StringBuilder();

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);

            if (category != UnicodeCategory.NonSpacingMark)
                result.Append(character);
        }

        return result.ToString().Replace("ı", "i");
    }
}