using System.Text.Json;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Services;

public sealed class ZteCdrParser : IZteCdrParser
{
    public Result<NormalizedCdrData> Parse(string rawPayload)
    {
        if (string.IsNullOrWhiteSpace(rawPayload))
        {
            return Result.Failure<NormalizedCdrData>(Error.Validation("Raw payload is empty"));
        }

        try
        {
            using var doc = JsonDocument.Parse(rawPayload);
            var root = doc.RootElement;

            var callingNumber = root.TryGetProperty("callingNumber", out var cn) ? cn.GetString() ?? string.Empty : string.Empty;
            var calledNumber = root.TryGetProperty("calledNumber", out var cdn) ? cdn.GetString() ?? string.Empty : string.Empty;
            var duration = root.TryGetProperty("duration", out var dur) ? dur.GetInt64() : 0L;
            var startTime = root.TryGetProperty("startTime", out var st) ? st.GetDateTime() : DateTime.MinValue;
            var callType = root.TryGetProperty("callType", out var ct) ? ct.GetString() : null;
            var trunkId = root.TryGetProperty("trunkId", out var ti) ? ti.GetString() : null;
            var billingParty = root.TryGetProperty("billingParty", out var bp) ? bp.GetString() : null;

            if (string.IsNullOrWhiteSpace(callingNumber))
            {
                return Result.Failure<NormalizedCdrData>(Error.Validation("Missing callingNumber in ZTE CDR"));
            }

            var subscriberId = callingNumber;

            var data = new NormalizedCdrData(
                subscriberId, string.Empty, 0, 0, duration,
                startTime, startTime.AddSeconds(duration), null, null,
                callingNumber, calledNumber, callType, trunkId, billingParty);

            return Result.Success(data);
        }
        catch (JsonException ex)
        {
            return Result.Failure<NormalizedCdrData>(Error.Validation($"Invalid ZTE CDR JSON: {ex.Message}"));
        }
    }
}
