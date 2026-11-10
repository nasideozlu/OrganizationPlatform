using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace OrganizationPlatform.Helpers;

public static class ReviewModerationHelper
{
    private static readonly string[] RestrictedWords =
    {
        "amk",
        "aq",
        "orospu",
        "pic",
        "siktir",
        "yarrak",
        "kahpe"
    };

    public static bool ContainsRestrictedWord(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var normalizedText = NormalizeText(text);

        return RestrictedWords.Any(word =>
        {
            var normalizedWord = NormalizeText(word);

            return Regex.IsMatch(
                normalizedText,
                $@"(?<![a-z0-9]){Regex.Escape(normalizedWord)}(?![a-z0-9])"
            );
        });
    }

    private static string NormalizeText(string text)
    {
        text = text.ToLower(new CultureInfo("tr-TR"));

        text = text
            .Replace("ı", "i")
            .Replace("ş", "s")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ö", "o")
            .Replace("ç", "c");

        return Regex.Replace(text, @"[^a-z0-9]+", " ");
    }
}