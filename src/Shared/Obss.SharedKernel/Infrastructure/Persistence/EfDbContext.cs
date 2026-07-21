using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.Events;

namespace Obss.SharedKernel.Infrastructure.Persistence;

public class EfDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILogger<EfDbContext> _logger;

    public EfDbContext(
        DbContextOptions options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher,
        ILogger<EfDbContext>? logger = null)
        : base(options)
    {
        _currentTenant = currentTenant;
        _domainEventDispatcher = domainEventDispatcher;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<EfDbContext>.Instance;
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        ConfigureOutboxMessage(modelBuilder);
        ConfigureInboxMessage(modelBuilder);
        modelBuilder.Ignore<DomainEvent>();
        modelBuilder.Ignore<IntegrationEvent>();
        ApplyTenantQueryFilters(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            return;
        }

        optionsBuilder.UseSnakeCaseNamingConvention();
        base.OnConfiguring(optionsBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTenantId();
        var domainEvents = GetAndClearDomainEvents();
        SaveOutboxMessages();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (_domainEventDispatcher is not null && domainEvents.Count > 0)
        {
            try
            {
                await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch {DomainEventCount} domain event(s) after successful save. Database changes are committed.", domainEvents.Count);
            }
        }

        return result;
    }

    private void SetTenantId()
    {
        var tenantId = _currentTenant.TenantId;

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return;
        }

        foreach (var entity in ChangeTracker.Entries<ITenantEntity>()
            .Where(e => e.State is EntityState.Added)
            .Select(e => e.Entity))
        {
            var prop = entity.GetType().GetProperty("TenantId");
            if (prop is null || prop.PropertyType != typeof(string))
                continue;
            prop.SetValue(entity, tenantId);
        }
    }

    private List<DomainEvent> GetAndClearDomainEvents()
    {
        var domainEvents = ChangeTracker.Entries<Entity<Guid>>()
            .Select(e => e.Entity)
            .SelectMany(e =>
            {
                var events = e.DomainEvents.ToList();
                e.ClearDomainEvents();
                return events;
            })
            .ToList();

        return domainEvents;
    }

    private void SaveOutboxMessages()
    {
        var aggregateRoots = ChangeTracker.Entries<AggregateRoot<Guid>>()
            .Where(e => e.Entity.IntegrationEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        foreach (var aggregateRoot in aggregateRoots)
        {
            foreach (var integrationEvent in aggregateRoot.IntegrationEvents)
            {
                var serializedData = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType());
                var outboxMessage = new OutboxMessage(
                    Guid.NewGuid(),
                    integrationEvent.EventType,
                    serializedData,
                    integrationEvent.TenantId,
                    integrationEvent.CorrelationId);

                Add(outboxMessage);
            }

            aggregateRoot.ClearIntegrationEvents();
        }
    }

    private static void ConfigureOutboxMessage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EventType).HasMaxLength(500).IsRequired();
            entity.Property(e => e.EventData).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ProcessedAt);
            entity.Property(e => e.TenantId).HasMaxLength(100);
            entity.Property(e => e.CorrelationId).HasMaxLength(200);
            entity.Property(e => e.LastError).HasMaxLength(2000);
            entity.Property(e => e.NextAttemptAt);
            entity.Property(e => e.LockId);
            entity.Property(e => e.LockExpiresAt);
            entity.HasIndex(e => e.ProcessedAt).HasDatabaseName("ix_outbox_messages_processed_at");
            entity.HasIndex(e => e.CreatedAt).HasDatabaseName("ix_outbox_messages_created_at");
            entity.HasIndex(e => new { e.ProcessedAt, e.NextAttemptAt, e.IsDeadLettered }).HasDatabaseName("ix_outbox_messages_pending");
        });
    }

    private static void ConfigureInboxMessage(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.ToTable("inbox_messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.EventId).HasMaxLength(200).IsRequired();
            entity.Property(e => e.HandlerName).HasMaxLength(500).IsRequired();
            entity.Property(e => e.EventType).HasMaxLength(500).IsRequired();
            entity.Property(e => e.EventData).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.ReceivedAt).IsRequired();
            entity.Property(e => e.TenantId).HasMaxLength(100);
            entity.Property(e => e.CorrelationId).HasMaxLength(200);
            entity.HasIndex(e => new { e.EventId, e.HandlerName }).IsUnique().HasDatabaseName("ix_inbox_messages_event_id_handler_name");
            entity.HasIndex(e => e.ReceivedAt).HasDatabaseName("ix_inbox_messages_received_at");
        });
    }

    private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        var tenantId = _currentTenant.TenantId;

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return;
        }

        foreach (var clrType in modelBuilder.Model.GetEntityTypes()
            .Where(e => typeof(ITenantEntity).IsAssignableFrom(e.ClrType))
            .Select(e => e.ClrType))
        {
            var parameter = Expression.Parameter(clrType, "e");
            var asInterface = Expression.Convert(parameter, typeof(ITenantEntity));
            var property = Expression.Property(asInterface, nameof(ITenantEntity.TenantId));
            var value = Expression.Constant(tenantId);
            var body = Expression.Equal(property, value);
            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }
    }
}
