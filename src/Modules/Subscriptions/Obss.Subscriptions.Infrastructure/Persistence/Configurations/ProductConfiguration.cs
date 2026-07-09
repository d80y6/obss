using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Subscriptions.Domain.Entities;

namespace Obss.Subscriptions.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(p => p.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200);
        builder.Property(p => p.Description).HasColumnName("description").HasMaxLength(1000);
        builder.Property(p => p.ProductSpecificationId).HasColumnName("product_specification_id");
        builder.Property(p => p.ProductOfferingId).HasColumnName("product_offering_id");
        builder.Property(p => p.Status).HasColumnName("status").HasMaxLength(50).IsRequired().HasConversion<string>();
        builder.Property(p => p.ActivationDate).HasColumnName("activation_date");
        builder.Property(p => p.TerminationDate).HasColumnName("termination_date");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").IsRequired();

        builder.OwnsOne(p => p.BillingAccount, ba =>
        {
            ba.Property(b => b.AccountId).HasColumnName("billing_account_id").HasMaxLength(100);
            ba.Property(b => b.Href).HasColumnName("billing_account_href").HasMaxLength(500);
        });

        builder.OwnsOne(p => p.Place, pl =>
        {
            pl.Property(x => x.Id).HasColumnName("place_id").HasMaxLength(100);
            pl.Property(x => x.Role).HasColumnName("place_role").HasMaxLength(50);
            pl.Property(x => x.Name).HasColumnName("place_name").HasMaxLength(200);
            pl.Property(x => x.Street).HasColumnName("place_street").HasMaxLength(200);
            pl.Property(x => x.City).HasColumnName("place_city").HasMaxLength(100);
            pl.Property(x => x.State).HasColumnName("place_state").HasMaxLength(50);
            pl.Property(x => x.Zip).HasColumnName("place_zip").HasMaxLength(20);
            pl.Property(x => x.Country).HasColumnName("place_country").HasMaxLength(50);
        });

        builder.OwnsOne(p => p.Agreement, ag =>
        {
            ag.Property(a => a.AgreementId).HasColumnName("agreement_id").HasMaxLength(100);
            ag.Property(a => a.Name).HasColumnName("agreement_name").HasMaxLength(200);
            ag.Property(a => a.Href).HasColumnName("agreement_href").HasMaxLength(500);
        });

        builder.HasMany(p => p.Relationships).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Characteristics).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Prices).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Terms).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.RealizingServices).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.RealizingResources).WithOne().HasForeignKey("ProductId").OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.TenantId).HasDatabaseName("ix_products_tenant_id");
        builder.HasIndex(p => p.CustomerId).HasDatabaseName("ix_products_customer_id");
        builder.HasIndex(p => p.Status).HasDatabaseName("ix_products_status");

        builder.Navigation(p => p.Relationships).AutoInclude();
        builder.Navigation(p => p.Characteristics).AutoInclude();
        builder.Navigation(p => p.Prices).AutoInclude();
        builder.Navigation(p => p.Terms).AutoInclude();
        builder.Navigation(p => p.RealizingServices).AutoInclude();
        builder.Navigation(p => p.RealizingResources).AutoInclude();
    }
}
