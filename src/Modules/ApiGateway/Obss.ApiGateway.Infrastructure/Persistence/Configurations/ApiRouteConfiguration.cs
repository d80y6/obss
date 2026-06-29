using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Obss.ApiGateway.Domain.Entities;

namespace Obss.ApiGateway.Infrastructure.Persistence.Configurations;

public sealed class ApiRouteConfiguration : IEntityTypeConfiguration<ApiRoute>
{
    public void Configure(EntityTypeBuilder<ApiRoute> builder)
    {
        builder.ToTable("api_routes");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Path)
            .HasColumnName("path")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.Method)
            .HasColumnName("method")
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(r => r.TargetModule)
            .HasColumnName("target_module")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(r => r.TargetPath)
            .HasColumnName("target_path")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(r => r.RequireAuthentication)
            .HasColumnName("require_authentication")
            .IsRequired();

        builder.Property(r => r.RequiredPermissions)
            .HasColumnName("required_permissions")
            .HasColumnType("jsonb")
            .HasConversion(new ListStringConverter())
            .IsRequired();

        builder.Property(r => r.RateLimitPerMinute)
            .HasColumnName("rate_limit_per_minute")
            .IsRequired();

        builder.Property(r => r.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.HasIndex(r => new { r.Path, r.Method })
            .HasDatabaseName("ix_api_routes_path_method")
            .IsUnique();
    }
}

public sealed class ListStringConverter : ValueConverter<List<string>, string>
{
    public ListStringConverter()
        : base(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
    {
    }
}
