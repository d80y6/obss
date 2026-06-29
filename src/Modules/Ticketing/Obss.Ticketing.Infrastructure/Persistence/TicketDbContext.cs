using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Infrastructure.Persistence;

public class TicketDbContext : EfDbContext
{
    public TicketDbContext(
        DbContextOptions<TicketDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<TicketComment> TicketComments => Set<TicketComment>();
    public DbSet<TicketAttachment> TicketAttachments => Set<TicketAttachment>();
    public DbSet<SlaDefinition> SlaDefinitions => Set<SlaDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
