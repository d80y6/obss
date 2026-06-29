using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Infrastructure.Persistence.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.Property(t => t.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(t => t.TicketNumber)
            .HasColumnName("ticket_number")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired();

        builder.Property(t => t.CustomerName)
            .HasColumnName("customer_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Subject)
            .HasColumnName("subject")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(t => t.Source)
            .HasColumnName("source")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.AssignedTo)
            .HasColumnName("assigned_to")
            .HasMaxLength(100);

        builder.Property(t => t.AssignedGroup)
            .HasColumnName("assigned_group")
            .HasMaxLength(100);

        builder.Property(t => t.Resolution)
            .HasColumnName("resolution")
            .HasMaxLength(4000);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(t => t.ClosedAt)
            .HasColumnName("closed_at");

        builder.Property(t => t.FirstResponseAt)
            .HasColumnName("first_response_at");

        builder.Property(t => t.SlaDeadline)
            .HasColumnName("sla_deadline");

        builder.Property(t => t.SlaResponseDeadline)
            .HasColumnName("sla_response_deadline");

        builder.Property(t => t.SlaBreachedAt)
            .HasColumnName("sla_breached_at");

        builder.Property(t => t.SlaDefinitionId)
            .HasColumnName("sla_definition_id");

        builder.HasIndex(t => t.SlaDefinitionId)
            .HasDatabaseName("ix_tickets_sla_definition_id");

        builder.HasMany(t => t.Comments)
            .WithOne(c => c.Ticket)
            .HasForeignKey(c => c.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Attachments)
            .WithOne(a => a.Ticket)
            .HasForeignKey(a => a.TicketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => t.TicketNumber)
            .HasDatabaseName("ix_tickets_ticket_number")
            .IsUnique();

        builder.HasIndex(t => t.TenantId)
            .HasDatabaseName("ix_tickets_tenant_id");

        builder.HasIndex(t => t.CustomerId)
            .HasDatabaseName("ix_tickets_customer_id");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("ix_tickets_status");

        builder.HasIndex(t => t.AssignedTo)
            .HasDatabaseName("ix_tickets_assigned_to");

        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("ix_tickets_created_at");

        builder.HasIndex(t => new { t.TenantId, t.Status })
            .HasDatabaseName("ix_tickets_tenant_id_status");

        builder.HasIndex(t => new { t.TenantId, t.AssignedTo, t.Status })
            .HasDatabaseName("ix_tickets_tenant_assigned_status");

        builder.Navigation(t => t.Comments)
            .AutoInclude();

        builder.Navigation(t => t.Attachments)
            .AutoInclude();
    }
}
