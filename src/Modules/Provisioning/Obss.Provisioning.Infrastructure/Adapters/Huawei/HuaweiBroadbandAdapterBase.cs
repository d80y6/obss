using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Adapters.Common;

namespace Obss.Provisioning.Infrastructure.Adapters.Huawei;

public abstract class HuaweiBroadbandAdapterBase
{
    private readonly ILogger _logger;
    private readonly HuaweiAdapterConfig _config;

    protected HuaweiBroadbandAdapterBase(ILogger logger, HuaweiAdapterConfig config)
    {
        _logger = logger;
        _config = config;
    }

    public abstract string AdapterName { get; }

    protected HuaweiAdapterConfig Config => _config;

    protected async Task<AdapterResult> ExecuteWithRetryAsync(
        string operationName,
        string correlationId,
        Func<Task<AdapterResult>> operation,
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        var maxRetries = _config.MaxRetries;
        var delay = _config.RetryDelayMs;

        while (true)
        {
            attempt++;
            var sw = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation(
                    "[Huawei:{Adapter}] Executing {Operation} (attempt {Attempt}/{MaxRetries})",
                    AdapterName, operationName, attempt, maxRetries);

                var result = await operation();

                sw.Stop();
                var enrichedResult = result with
                {
                    Duration = sw.Elapsed,
                    AdapterName = AdapterName,
                    CorrelationId = correlationId
                };

                _logger.LogInformation(
                    "[Huawei:{Adapter}] {Operation} completed in {DurationMs}ms with state {State}",
                    AdapterName, operationName, sw.ElapsedMilliseconds, enrichedResult.State);

                return enrichedResult;
            }
            catch (OperationCanceledException ex)
            {
                sw.Stop();
                _logger.LogWarning(ex,
                    "[Huawei:{Adapter}] {Operation} was cancelled after {DurationMs}ms",
                    AdapterName, operationName, sw.ElapsedMilliseconds);

                return AdapterResult.Fail(
                    $"Operation {operationName} was cancelled",
                    sw.Elapsed,
                    AdapterName,
                    correlationId);
            }
            catch (HttpRequestException ex) when (attempt <= maxRetries)
            {
                sw.Stop();
                _logger.LogWarning(
                    ex,
                    "[Huawei:{Adapter}] {Operation} failed (attempt {Attempt}/{MaxRetries}), retrying in {Delay}ms",
                    AdapterName, operationName, attempt, maxRetries, delay);

                if (attempt >= maxRetries)
                {
                    return AdapterResult.Fail(
                        $"Operation {operationName} failed after {maxRetries} retries: {ex.Message}",
                        sw.Elapsed,
                        AdapterName,
                        correlationId);
                }

                await Task.Delay(delay, cancellationToken);
                delay *= 2;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(
                    ex,
                    "[Huawei:{Adapter}] {Operation} failed with unexpected error",
                    AdapterName, operationName);

                return AdapterResult.Fail(
                    $"Unexpected error in {operationName}: {ex.Message}",
                    sw.Elapsed,
                    AdapterName,
                    correlationId);
            }
        }
    }

        protected static string SerializeResult<T>(T data)
        {
            return JsonSerializer.Serialize(data);
        }
}
