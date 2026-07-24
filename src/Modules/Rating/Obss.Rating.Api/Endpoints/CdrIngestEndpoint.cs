using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Obss.Rating.Application.Services;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.Rating.Api.Endpoints;

public sealed record CdrIngestRequest(
    string CorrelationId,
    string Vendor,
    string Payload);

public sealed record CdrIngestResponse(
    int Accepted,
    int Duplicates,
    int Quarantined,
    string Message);

public static class CdrIngestEndpoint
{
    private const string HmacHeader = "X-CDR-HMAC-Signature";

    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/cdr/ingest", async (
            List<CdrIngestRequest> requests,
            ICdrMediationService mediationService,
            ILogger logger,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            if (!ValidateHmacSignature(httpContext, requests))
            {
                return Results.Unauthorized();
            }

            var records = requests.Select(r => new RawCdrRecord(
                r.CorrelationId,
                r.Vendor,
                r.Payload,
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                DateTime.UtcNow)).ToList();

            var result = await mediationService.IngestBatchAsync(records, cancellationToken);

            logger.LogInformation(
                "CDR ingest endpoint: {Accepted} accepted, {Duplicates} duplicates, {Quarantined} quarantined",
                result.Accepted, result.Duplicates, result.Quarantined);

            var response = new CdrIngestResponse(
                result.Accepted,
                result.Duplicates,
                result.Quarantined,
                $"Accepted {result.Accepted}, rejected {result.Duplicates + result.Quarantined} records");

            return Results.Accepted("/api/v1/rating/cdr/ingest", response);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.CdrIngest));

        group.MapPost("/cdr/replay", async (
            ICdrMediationService mediationService,
            ILogger logger,
            CancellationToken cancellationToken) =>
        {
            var result = await mediationService.ReplayQuarantinedAsync(cancellationToken);

            logger.LogInformation(
                "CDR replay: {Replayed} replayed, {StillInvalid} still invalid",
                result.Replayed, result.StillInvalid);

            return Results.Ok(result);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.CdrMediate));
    }

    private static bool ValidateHmacSignature(HttpContext httpContext, List<CdrIngestRequest> requests)
    {
        if (!httpContext.Request.Headers.TryGetValue(HmacHeader, out var signatureHeader))
        {
            return false;
        }

        var secretKey = Environment.GetEnvironmentVariable("CDR_HMAC_SECRET");
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            return true;
        }

        var payload = string.Join(";", requests.Select(r => $"{r.CorrelationId}|{r.Vendor}|{r.Payload}"));
        var computedHash = ComputeHmacSha256(secretKey, payload);
        var providedSignature = signatureHeader.ToString();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedHash),
            Encoding.UTF8.GetBytes(providedSignature));
    }

    private static string ComputeHmacSha256(string secretKey, string payload)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(payloadBytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
