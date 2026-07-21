using Microsoft.EntityFrameworkCore;
using Obss.Rating.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Rating.Infrastructure.Persistence;

public class RatingDbContext : EfDbContext
{
    public RatingDbContext(
        DbContextOptions<RatingDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<UsageRecord> UsageRecords => Set<UsageRecord>();
    public DbSet<RatingRule> RatingRules => Set<RatingRule>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PromotionRule> PromotionRules => Set<PromotionRule>();
    public DbSet<CdrRecord> CdrRecords => Set<CdrRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RatingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
