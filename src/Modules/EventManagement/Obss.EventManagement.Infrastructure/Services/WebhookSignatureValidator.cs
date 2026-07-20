using System.Security.Cryptography;
using System.Text;

namespace Obss.EventManagement.Infrastructure.Services;

public sealed class WebhookSignatureValidator
{
    private readonly Dictionary<string, string> _activeSecrets;
    private readonly Dictionary<string, string> _previousSecrets;

    public WebhookSignatureValidator(Dictionary<string, string> activeSecrets, Dictionary<string, string>? previousSecrets = null)
    {
        _activeSecrets = activeSecrets;
        _previousSecrets = previousSecrets ?? new();
    }

    public ValidationResult ValidateSignature(string payload, string signatureHeader, string timestampHeader, string keyId, int maxAgeSeconds = 300)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader))
            return ValidationResult.Failure("Missing signature header.");

        if (string.IsNullOrWhiteSpace(timestampHeader))
            return ValidationResult.Failure("Missing timestamp header.");

        if (!long.TryParse(timestampHeader, out var unixTimestamp))
            return ValidationResult.Failure("Invalid timestamp format.");

        var timestamp = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
        var now = DateTimeOffset.UtcNow;

        if (Math.Abs((now - timestamp).TotalSeconds) > maxAgeSeconds)
            return ValidationResult.Failure("Timestamp outside allowed window (replay attack detected).");

        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var timestampBytes = Encoding.UTF8.GetBytes(timestampHeader);
        var messageBytes = new byte[payloadBytes.Length + 1 + timestampBytes.Length];
        Buffer.BlockCopy(payloadBytes, 0, messageBytes, 0, payloadBytes.Length);
        messageBytes[payloadBytes.Length] = 46;
        Buffer.BlockCopy(timestampBytes, 0, messageBytes, payloadBytes.Length + 1, timestampBytes.Length);

        if (_activeSecrets.TryGetValue(keyId, out var activeSecret))
        {
            var expectedSig = ComputeHmacSha256(activeSecret, messageBytes);
            if (CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signatureHeader),
                Encoding.UTF8.GetBytes(expectedSig)))
            {
                return ValidationResult.Success(keyId);
            }
        }

        if (_previousSecrets.TryGetValue(keyId, out var previousSecret))
        {
            var expectedSig = ComputeHmacSha256(previousSecret, messageBytes);
            if (CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(signatureHeader),
                Encoding.UTF8.GetBytes(expectedSig)))
            {
                return ValidationResult.Success(keyId);
            }
        }

        return ValidationResult.Failure("Invalid signature.");
    }

    private static string ComputeHmacSha256(string secret, byte[] message)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(message);
        return $"sha256={Convert.ToHexString(hash).ToLowerInvariant()}";
    }
}

public sealed class ValidationResult
{
    public bool IsValid { get; private set; }
    public string? Error { get; private set; }
    public string? KeyId { get; private set; }

    private ValidationResult() { }

    public static ValidationResult Success(string keyId) => new() { IsValid = true, KeyId = keyId };

    public static ValidationResult Failure(string error) => new() { IsValid = false, Error = error };
}
