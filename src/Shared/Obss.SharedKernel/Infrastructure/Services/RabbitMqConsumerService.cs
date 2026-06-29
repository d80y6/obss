using System.Text;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.Events;
using Obss.SharedKernel.Infrastructure.EventBus;
using Obss.SharedKernel.Infrastructure.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Obss.SharedKernel.Infrastructure.Services;

public sealed class RabbitMqConsumerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<RabbitMqConfiguration> _rabbitMqConfiguration;
    private readonly ILogger<RabbitMqConsumerService> _logger;
    private readonly Dictionary<string, Type> _eventTypes;

    public RabbitMqConsumerService(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqConfiguration> rabbitMqConfiguration,
        ILogger<RabbitMqConsumerService> logger)
    {
        _scopeFactory = scopeFactory;
        _rabbitMqConfiguration = rabbitMqConfiguration;
        _logger = logger;
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

        var queueName = $"obss_integration_events_{Guid.NewGuid():N}";

        var queueDeclareOk = await channel.QueueDeclareAsync(
            queue: queueName,
            durable: false,
            exclusive: false,
            autoDelete: true,
            cancellationToken: stoppingToken);

        _logger.LogInformation("Declared queue {QueueName}", queueDeclareOk.QueueName);

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
                queue: queueDeclareOk.QueueName,
                exchange: exchangeName,
                routingKey: eventTypeName,
                cancellationToken: stoppingToken);

            _logger.LogDebug("Bound queue {QueueName} to exchange {Exchange}", queueDeclareOk.QueueName, exchangeName);
        }

        _logger.LogInformation(
            "Listening on {ExchangeCount} exchanges for {EventCount} event types",
            _eventTypes.Count, _eventTypes.Count);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += (_, ea) => ProcessMessageAsync(ea, channel, stoppingToken);

        await channel.BasicConsumeAsync(
            queue: queueDeclareOk.QueueName,
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
        if (!_eventTypes.TryGetValue(eventTypeName, out var eventType))
        {
            _logger.LogWarning("Unknown event type: {EventType}", eventTypeName);
            await TryAckAsync(ea, channel);
            return;
        }

        var body = Encoding.UTF8.GetString(ea.Body.Span);
        var integrationEvent = (IntegrationEvent?)SystemTextJsonSerializer.Deserialize(body, eventType);

        if (integrationEvent is null)
        {
            _logger.LogError("Failed to deserialize event {EventType}: {Body}", eventTypeName, body);
            await TryAckAsync(ea, channel);
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

        try
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(integrationEvent, cancellationToken);

            await inboxService.MarkAsProcessedAsync(messageId, handlerName, cancellationToken);

            _logger.LogInformation(
                "Processed integration event {EventType}/{MessageId}",
                eventTypeName, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to process integration event {EventType}/{MessageId}",
                eventTypeName, messageId);
        }

        await TryAckAsync(ea, channel);
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
