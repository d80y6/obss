using System.Text.Json;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Rating.Application.Services;

public sealed class HuaweiCdrParser : IHuaweiCdrParser
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

            var subscriberId = root.TryGetProperty("subscriberId", out var sid) ? sid.GetString() ?? string.Empty : string.Empty;
            var sessionId = root.TryGetProperty("sessionId", out var sess) ? sess.GetString() ?? string.Empty : string.Empty;
            var bytesUp = root.TryGetProperty("bytesUp", out var bUp) ? bUp.GetInt64() : 0L;
            var bytesDown = root.TryGetProperty("bytesDown", out var bDown) ? bDown.GetInt64() : 0L;
            var duration = root.TryGetProperty("duration", out var dur) ? dur.GetInt64() : 0L;
            var startTime = root.TryGetProperty("startTime", out var st) ? st.GetDateTime() : DateTime.MinValue;
            var endTime = root.TryGetProperty("endTime", out var et) ? et.GetDateTime() : DateTime.MinValue;
            var apn = root.TryGetProperty("apn", out var a) ? a.GetString() : null;
            var qos = root.TryGetProperty("qos", out var q) ? q.GetString() : null;

            if (string.IsNullOrWhiteSpace(subscriberId))
            {
                return Result.Failure<NormalizedCdrData>(Error.Validation("Missing subscriberId in Huawei CDR"));
            }

            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return Result.Failure<NormalizedCdrData>(Error.Validation("Missing sessionId in Huawei CDR"));
            }

            var data = new NormalizedCdrData(
                subscriberId, sessionId, bytesUp, bytesDown, duration,
                startTime, endTime, apn, qos, null, null, null, null, null);

            return Result.Success(data);
        }
        catch (JsonException ex)
        {
            return Result.Failure<NormalizedCdrData>(Error.Validation($"Invalid Huawei CDR JSON: {ex.Message}"));
        }
    }
}
