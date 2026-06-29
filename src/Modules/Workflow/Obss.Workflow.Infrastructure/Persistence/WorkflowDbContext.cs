using Microsoft.EntityFrameworkCore;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Infrastructure.Persistence;
using Obss.Workflow.Domain.Entities;

namespace Obss.Workflow.Infrastructure.Persistence;

public class WorkflowDbContext : EfDbContext
{
    public WorkflowDbContext(
        DbContextOptions<WorkflowDbContext> options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options, currentTenant, domainEventDispatcher)
    {
    }

    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowStep> WorkflowSteps => Set<WorkflowStep>();
    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();
    public DbSet<WorkflowTaskInstance> WorkflowTaskInstances => Set<WorkflowTaskInstance>();
    public DbSet<SlaDefinition> SlaDefinitions => Set<SlaDefinition>();
    public DbSet<WorkflowSla> WorkflowSlas => Set<WorkflowSla>();
    public DbSet<WorkflowMetric> WorkflowMetrics => Set<WorkflowMetric>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkflowDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
