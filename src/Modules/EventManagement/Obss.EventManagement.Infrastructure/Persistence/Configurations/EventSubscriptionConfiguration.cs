using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.EventManagement.Domain.Entities;
using Obss.EventManagement.Domain.ValueObjects;

namespace Obss.EventManagement.Infrastructure.Persistence.Configurations;

public sealed class EventSubscriptionConfiguration : IEntityTypeConfiguration<EventSubscription>
{
    public void Configure(EntityTypeBuilder<EventSubscription> builder)
    {
        builder.ToTable("event_subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.CallbackUrl)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(x => x.Query)
            .HasMaxLength(4000);

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.OwnsMany(x => x.Filters, f =>
        {
            f.WithOwner().HasForeignKey("EventSubscriptionId");
            f.ToTable("event_subscription_filters");
            f.HasKey("Id");
            f.Property<int>("Id").ValueGeneratedOnAdd();
            f.Property(x => x.EventType).HasMaxLength(200).IsRequired();
            f.Property(x => x.FilterCriteria).HasMaxLength(4000);
        });
    }
}
