using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.EventBus;
using Obss.SharedKernel.Infrastructure.Serialization;
using RabbitMQ.Client;

namespace Obss.SharedKernel.Infrastructure.Services;

public sealed class EventBusService : IEventBus, IAsyncDisposable
{
    private readonly RabbitMqConfiguration _configuration;
    private readonly ILogger<EventBusService> _logger;
    private IConnection? _connection;
    private bool _disposed;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    public EventBusService(
        IOptions<RabbitMqConfiguration> configuration,
        ILogger<EventBusService> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : INotification
    {
        ArgumentNullException.ThrowIfNull(@event);

        var connection = await GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var eventType = @event.GetType().Name;
        var exchangeName = $"integration.{eventType}";

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var body = SystemTextJsonSerializer.Serialize(@event);

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = Guid.NewGuid().ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Headers = new Dictionary<string, object?>
            {
                ["event-type"] = eventType,
                ["content-type"] = "application/json"
            }
        };

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: eventType,
            mandatory: false,
            basicProperties: properties,
            body: Encoding.UTF8.GetBytes(body),
            cancellationToken: cancellationToken);

        _logger.LogDebug(
            "Published event {EventType} to exchange {Exchange} with routing key {RoutingKey}",
            eventType, exchangeName, eventType);
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _configuration.Host,
                Port = _configuration.Port,
                UserName = _configuration.Username,
                Password = _configuration.Password,
                VirtualHost = _configuration.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                TopologyRecoveryEnabled = true,
                RequestedHeartbeat = TimeSpan.FromSeconds(60),
                ContinuationTimeout = TimeSpan.FromSeconds(20)
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);
            _logger.LogInformation(
                "Connected to RabbitMQ at {Host}:{Port} (vhost: {VirtualHost})",
                _configuration.Host, _configuration.Port, _configuration.VirtualHost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to RabbitMQ at {Host}:{Port}",
                _configuration.Host, _configuration.Port);
            throw;
        }
        finally
        {
            _connectionLock.Release();
        }

        return _connection;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_connection is not null)
        {
            try
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error while disposing RabbitMQ connection");
            }
        }

        _connectionLock.Dispose();
    }
}
