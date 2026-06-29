using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Serialization;
using RabbitMQ.Client;

namespace Obss.SharedKernel.Infrastructure.EventBus;

public sealed class RabbitMqEventBus : IEventBus, IAsyncDisposable
{
    private readonly RabbitMqConfiguration _configuration;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private IConnection? _connection;
    private bool _disposed;

    public RabbitMqEventBus(
        IOptions<RabbitMqConfiguration> configuration,
        ILogger<RabbitMqEventBus> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : INotification
    {
        var connection = await GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(
            cancellationToken: cancellationToken);

        var eventType = @event.GetType().Name;
        var exchangeName = $"integration.{eventType}";

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var body = SystemTextJsonSerializer.Serialize(@event);
        var routingKey = eventType;

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
        };

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(body),
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Published event {EventType} to exchange {Exchange} with routing key {RoutingKey}",
            eventType, exchangeName, routingKey);
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true })
        {
            return _connection;
        }

        var factory = new ConnectionFactory
        {
            HostName = _configuration.Host,
            Port = _configuration.Port,
            UserName = _configuration.Username,
            Password = _configuration.Password,
            VirtualHost = _configuration.VirtualHost,
            AutomaticRecoveryEnabled = true,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
        };

        _connection = await factory.CreateConnectionAsync(cancellationToken);
        return _connection;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}
