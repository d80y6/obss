using System.Text;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.Events;
using Obss.SharedKernel.Infrastructure.Diagnostics;
using Obss.SharedKernel.Infrastructure.EventBus;
using Obss.SharedKernel.Infrastructure.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Obss.SharedKernel.Infrastructure.Services;

public sealed class RabbitMqConsumerService : BackgroundService
{
    private const int _maxRetryCount = 3;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<RabbitMqConfiguration> _rabbitMqConfiguration;
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly RabbitMqMetrics _metrics;
    private readonly Dictionary<string, Type> _eventTypes;

    public RabbitMqConsumerService(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqConfiguration> rabbitMqConfiguration,
        ILogger<RabbitMqConsumerService> logger,
        RabbitMqMetrics metrics)
    {
        _scopeFactory = scopeFactory;
        _rabbitMqConfiguration = rabbitMqConfiguration;
        _logger = logger;
        _metrics = metrics;
        _eventTypes = DiscoverEventTypes();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("RabbitMqConsumerService starting...");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ consumer connection lost. Reconnecting in 10 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("RabbitMqConsumerService stopped");
    }

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        var config = _rabbitMqConfiguration.Value;
        var factory = new ConnectionFactory
        {
            HostName = config.Host,
            Port = config.Port,
            UserName = config.Username,
            Password = config.Password,
            VirtualHost = config.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
            TopologyRecoveryEnabled = true,
            RequestedHeartbeat = TimeSpan.FromSeconds(60),
            ContinuationTimeout = TimeSpan.FromSeconds(20)
        };

        await using var connection = await factory.CreateConnectionAsync(stoppingToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        var mainQueueName = "obss_integration_events";
        var dlxExchangeName = "obss_integration_events.dlx";
        var dlxQueueName = "obss_integration_events.dlq";

        await channel.ExchangeDeclareAsync(
            exchange: dlxExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: dlxQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object?>(),
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: dlxQueueName,
            exchange: dlxExchangeName,
            routingKey: "dead-letter",
            cancellationToken: stoppingToken);

        _logger.LogInformation("Declared dead-letter queue {DlxQueueName}", dlxQueueName);

        var mainQueueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", dlxExchangeName },
            { "x-dead-letter-routing-key", "dead-letter" }
        };

        await channel.QueueDeclareAsync(
            queue: mainQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: mainQueueArgs,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Declared main queue {MainQueueName} with DLX", mainQueueName);

        foreach (var (eventTypeName, _) in _eventTypes)
        {
            var exchangeName = $"integration.{eventTypeName}";

            await channel.ExchangeDeclareAsync(
                exchange: exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                cancellationToken: stoppingToken);

            await channel.QueueBindAsync(
                queue: mainQueueName,
                exchange: exchangeName,
                routingKey: eventTypeName,
                cancellationToken: stoppingToken);

            _logger.LogDebug("Bound queue {QueueName} to exchange {Exchange}", mainQueueName, exchangeName);
        }

        _logger.LogInformation(
            "Listening on {ExchangeCount} exchanges for {EventCount} event types",
            _eventTypes.Count, _eventTypes.Count);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, ea) => ProcessMessageAsync(ea, channel, stoppingToken);

        await channel.BasicConsumeAsync(
            queue: mainQueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        _logger.LogInformation("RabbitMqConsumerService started successfully");

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            // Expected on shutdown
        }
    }

    private async Task ProcessMessageAsync(
        BasicDeliverEventArgs ea,
        IChannel channel,
        CancellationToken cancellationToken)
    {
        var eventTypeName = ea.RoutingKey;
        var messageId = ea.BasicProperties?.MessageId ?? Guid.NewGuid().ToString();
        var retryCount = ExtractRetryCount(ea);

        if (!_eventTypes.TryGetValue(eventTypeName, out var eventType))
        {
            _logger.LogWarning("Unknown event type: {EventType} - sending to dead-letter", eventTypeName);
            await TryNackAsync(ea, channel, requeue: false);
            return;
        }

        var body = Encoding.UTF8.GetString(ea.Body.Span);
        var integrationEvent = (IntegrationEvent?)SystemTextJsonSerializer.Deserialize(body, eventType);

        if (integrationEvent is null)
        {
            _logger.LogError("Failed to deserialize event {EventType} - sending to dead-letter", eventTypeName);
            await TryNackAsync(ea, channel, requeue: false);
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var inboxService = scope.ServiceProvider.GetRequiredService<IInboxService>();
        var handlerName = integrationEvent.EventType;

        var alreadyProcessed = await inboxService.IsProcessedAsync(messageId, handlerName, cancellationToken);
        if (alreadyProcessed)
        {
            _logger.LogDebug("Skipping already processed event {EventType}/{MessageId}", eventTypeName, messageId);
            await TryAckAsync(ea, channel);
            return;
        }

        _metrics.MessageConsumed();

        try
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(integrationEvent, cancellationToken);

            await inboxService.MarkAsProcessedAsync(messageId, handlerName, cancellationToken);

            _logger.LogInformation(
                "Processed integration event {EventType}/{MessageId}",
                eventTypeName, messageId);

            await TryAckAsync(ea, channel);
        }
        catch (Exception ex)
        {
            _metrics.MessageFailed();

            _logger.LogError(ex,
                "Failed to process integration event {EventType}/{MessageId} (retry {RetryCount}/{MaxRetryCount})",
                eventTypeName, messageId, retryCount, _maxRetryCount);

            if (retryCount >= _maxRetryCount)
            {
                _metrics.MessageDeadLettered();

                _logger.LogWarning(
                    "Event {EventType}/{MessageId} exceeded max retries - sending to dead-letter",
                    eventTypeName, messageId);

                await TryNackAsync(ea, channel, requeue: false);
            }
            else
            {
                _metrics.MessageRetried();

                await TryNackAsync(ea, channel, requeue: true);
            }
        }
    }

    private static int ExtractRetryCount(BasicDeliverEventArgs ea)
    {
        if (ea.BasicProperties?.Headers is not null &&
            ea.BasicProperties.Headers.TryGetValue("x-retry-count", out var retryObj) &&
            retryObj is byte[] retryBytes &&
            int.TryParse(Encoding.UTF8.GetString(retryBytes), out var count))
        {
            return count;
        }

        return 0;
    }

    private static async Task TryAckAsync(BasicDeliverEventArgs ea, IChannel channel)
    {
        try
        {
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to ack message: {ex.Message}");
        }
    }

    private static async Task TryNackAsync(BasicDeliverEventArgs ea, IChannel channel, bool requeue)
    {
        try
        {
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: requeue);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to nack message: {ex.Message}");
        }
    }

    private static Dictionary<string, Type> DiscoverEventTypes()
    {
        var eventTypes = new Dictionary<string, Type>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic)
                continue;

            foreach (var type in assembly.GetTypes())
            {
                if (type is { IsClass: true, IsAbstract: false } && typeof(IntegrationEvent).IsAssignableFrom(type))
                {
                    eventTypes[type.Name] = type;
                }
            }
        }

        return eventTypes;
    }
}
