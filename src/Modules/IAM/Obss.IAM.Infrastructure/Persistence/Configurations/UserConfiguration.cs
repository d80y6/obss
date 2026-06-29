using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.IAM.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.TenantId)
            .HasColumnName("tenant_id")
            .HasConversion<TenantIdValueConverter>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasConversion<EmailValueConverter>()
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(c => c.Email)
            .HasDatabaseName("ix_users_email")
            .IsUnique();

        builder.Property(u => u.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PhoneNumber)
            .HasColumnName("phone_number")
            .HasConversion<PhoneNumberValueConverter>()
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .IsRequired();

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        builder.Property(u => u.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(200);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(u => u.Username)
            .HasDatabaseName("ix_users_username");

        builder.HasIndex(u => u.ExternalId)
            .HasDatabaseName("ix_users_external_id");

        builder.Navigation(u => u.UserRoles)
            .AutoInclude();
    }
}
