using System.Text.Json;

namespace Obss.Provisioning.Infrastructure.Transports.Restconf;

public sealed class RestconfErrorParser
{
    public sealed record ParsedError(string ErrorType, string ErrorTag, string? ErrorPath, string? ErrorMessage)
    {
        public override string ToString() => $"[{ErrorType}] {ErrorTag} at {ErrorPath}: {ErrorMessage}";
    }

#pragma warning disable S2325 // Method doesn't use instance state; kept instance for consistency
    public ParsedError? ParseError(string rawData, string contentType)
#pragma warning restore S2325
    {
        try
        {
            using var doc = JsonDocument.Parse(rawData);
            var root = doc.RootElement;

            if (!root.TryGetProperty("ietf-restconf:errors", out var errors))
                return null;

            if (!errors.TryGetProperty("error", out var errorArr) || errorArr.GetArrayLength() == 0)
                return null;

            var first = errorArr[0];
            return new ParsedError(
                GetString(first, "error-type") ?? "application",
                GetString(first, "error-tag") ?? "unknown",
                GetString(first, "error-path"),
                GetString(first, "error-message"));
        }
        catch
        {
            return null;
        }
    }

    private static string? GetString(JsonElement el, string property)
        => el.TryGetProperty(property, out var val) ? val.GetString() : null;
}