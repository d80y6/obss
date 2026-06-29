using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obss.SharedKernel.Infrastructure.EventBus;
using Obss.SharedKernel.Infrastructure.Serialization;
using RabbitMQ.Client;

namespace Obss.SharedKernel.Infrastructure.Persistence;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<RabbitMqConfiguration> _rabbitMqConfiguration;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _pollingInterval;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqConfiguration> rabbitMqConfiguration,
        ILogger<OutboxProcessor> logger,
        TimeSpan? pollingInterval = null)
    {
        _scopeFactory = scopeFactory;
        _rabbitMqConfiguration = rabbitMqConfiguration;
        _logger = logger;
        _pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(10);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started with polling interval {PollingInterval}", _pollingInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }

        _logger.LogInformation("OutboxProcessor stopped");
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var contexts = scope.ServiceProvider.GetServices<EfDbContext>();
        var processedIds = new HashSet<Guid>();

        var connection = await CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(
            cancellationToken: cancellationToken);

        foreach (var context in contexts)
        {
            var messages = await context.OutboxMessages
                .Where(m => m.ProcessedAt == null)
                .OrderBy(m => m.CreatedAt)
                .Take(50)
                .ToListAsync(cancellationToken);

            if (messages.Count == 0)
            {
                continue;
            }

            foreach (var message in messages)
            {
                if (processedIds.Contains(message.Id))
                {
                    message.MarkAsProcessed();
                    continue;
                }

                try
                {
                    await PublishMessageAsync(channel, message, cancellationToken);
                    processedIds.Add(message.Id);

                    message.MarkAsProcessed();
                    context.OutboxMessages.Update(message);

                    _logger.LogInformation(
                        "Published event {MessageId} of type {EventType}",
                        message.Id, message.EventType);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to process outbox message {MessageId} of type {EventType}",
                        message.Id, message.EventType);
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task PublishMessageAsync(
        IChannel channel,
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var exchangeName = $"integration.{message.EventType}";

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = message.Id.ToString(),
            Timestamp = new AmqpTimestamp(
                DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        if (!string.IsNullOrEmpty(message.CorrelationId))
        {
            properties.CorrelationId = message.CorrelationId;
        }

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: message.EventType,
            mandatory: false,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(message.EventData),
            cancellationToken: cancellationToken);
    }

    private async Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken)
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
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        return await factory.CreateConnectionAsync(cancellationToken);
    }
}
