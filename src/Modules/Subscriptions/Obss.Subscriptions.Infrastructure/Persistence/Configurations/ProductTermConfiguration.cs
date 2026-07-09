using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Infrastructure.Persistence.Configurations;

public sealed class ProductTermConfiguration : IEntityTypeConfiguration<ProductTerm>
{
    public void Configure(EntityTypeBuilder<ProductTerm> builder)
    {
        builder.ToTable("product_terms");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(t => t.Description).HasColumnName("description").HasMaxLength(500);
        builder.Property(t => t.Duration).HasColumnName("duration").IsRequired();
        builder.Property(t => t.DurationUnit).HasColumnName("duration_unit").HasMaxLength(20).IsRequired().HasConversion<string>();
        builder.Property(t => t.StartDate).HasColumnName("start_date").IsRequired();
        builder.Property(t => t.EndDate).HasColumnName("end_date");
    }
}
