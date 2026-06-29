using System.Security.Cryptography;
using System.Text;
using Obss.ApiGateway.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.ApiGateway.Domain.Entities;

public class ApiKey : AggregateRoot<Guid>
{
    private ApiKey() { }

    private ApiKey(
        Guid id,
        string tenantId,
        Guid? partnerId,
        string name,
        string key,
        ApiKeyStatus status,
        List<string> permissions,
        List<string> allowedIPs,
        int rateLimitPerMinute,
        DateTime? expiresAt)
        : base(id)
    {
        TenantId = tenantId;
        PartnerId = partnerId;
        Name = name;
        Key = key;
        Status = status;
        Permissions = permissions;
        AllowedIPs = allowedIPs;
        RateLimitPerMinute = rateLimitPerMinute;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public Guid? PartnerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Key { get; private set; } = string.Empty;
    public ApiKeyStatus Status { get; private set; }
    public List<string> Permissions { get; private set; } = [];
    public List<string> AllowedIPs { get; private set; } = [];
    public int RateLimitPerMinute { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    public static ApiKey Create(
        string tenantId,
        string name,
        List<string>? permissions = null,
        List<string>? allowedIPs = null,
        int rateLimitPerMinute = 60,
        Guid? partnerId = null,
        DateTime? expiresAt = null)
    {
        var rawKey = GenerateKey();
        var hashedKey = HashKey(rawKey);

        return new ApiKey(
            Guid.NewGuid(),
            tenantId,
            partnerId,
            name,
            hashedKey,
            ApiKeyStatus.Active,
            permissions ?? [],
            allowedIPs ?? [],
            rateLimitPerMinute,
            expiresAt);
    }

    public void Revoke()
    {
        if (Status == ApiKeyStatus.Revoked)
            return;

        Status = ApiKeyStatus.Revoked;
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsValid()
    {
        if (Status != ApiKeyStatus.Active)
            return false;

        if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow)
        {
            Status = ApiKeyStatus.Expired;
            return false;
        }

        return true;
    }

    private static string GenerateKey()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("/", "_")
            .Replace("+", "-")
            .TrimEnd('=');
    }

    public static string HashKey(string rawKey)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawKey));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
