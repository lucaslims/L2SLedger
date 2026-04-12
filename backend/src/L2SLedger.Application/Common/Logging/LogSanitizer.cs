using System.Text;
using System.Text.RegularExpressions;

namespace L2SLedger.Application.Common.Logging;

public static partial class LogSanitizer
{
    private const int DefaultMaxLength = 256;
    private const string TruncatedSuffix = "...[truncated]";

    public static string Sanitize(object? value, int maxLength = DefaultMaxLength, bool maskEmail = false)
    {
        if (value is null)
        {
            return string.Empty;
        }

        var normalizedValue = Normalize(value.ToString() ?? string.Empty);

        if (maskEmail)
        {
            normalizedValue = EmailRegex().Replace(normalizedValue, m => MaskEmail(m.Value));
        }

        return Truncate(normalizedValue, maxLength);
    }

    public static string SanitizeExceptionMessage(string? message, int maxLength = DefaultMaxLength)
    {
        return Sanitize(message, maxLength);
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var stringBuilder = new StringBuilder(value.Length);

        foreach (var character in value)
        {
            if (character is '\r' or '\n' or '\t')
            {
                stringBuilder.Append(' ');
                continue;
            }

            if (!char.IsControl(character))
            {
                stringBuilder.Append(character);
            }
        }

        return MultipleWhitespaceRegex().Replace(stringBuilder.ToString(), " ").Trim();
    }

    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || maxLength <= 0)
        {
            return string.Empty;
        }

        if (value.Length <= maxLength)
        {
            return value;
        }

        if (maxLength <= TruncatedSuffix.Length)
        {
            return value[..maxLength];
        }

        var contentLength = maxLength - TruncatedSuffix.Length;
        return value[..contentLength] + TruncatedSuffix;
    }

    private static string MaskEmail(string email)
    {
        var parts = email.Split('@', 2);
        if (parts.Length != 2)
        {
            return email;
        }

        var localPart = parts[0];
        var domainPart = parts[1];

        if (string.IsNullOrWhiteSpace(localPart))
        {
            return $"***@{domainPart}";
        }

        var visiblePrefixLength = Math.Min(2, localPart.Length);
        var visiblePrefix = localPart[..visiblePrefixLength];
        return $"{visiblePrefix}***@{domainPart}";
    }

    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
    private static partial Regex MultipleWhitespaceRegex();

    [GeneratedRegex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex EmailRegex();
}