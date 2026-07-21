using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.AAA.Domain.Entities;

namespace Obss.AAA.Infrastructure.Persistence.Configurations;

public sealed class RadiusSessionConfiguration : IEntityTypeConfiguration<RadiusSession>
{
    public void Configure(EntityTypeBuilder<RadiusSession> builder)
    {
        builder.ToTable("radius_sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.TenantId).HasColumnName("tenant_id").HasMaxLength(100).IsRequired();
        builder.Property(s => s.SessionId).HasColumnName("session_id").HasMaxLength(100).IsRequired();
        builder.Property(s => s.NasId).HasColumnName("nas_id").HasMaxLength(100).IsRequired();
        builder.Property(s => s.Username).HasColumnName("username").HasMaxLength(200).IsRequired();
        builder.Property(s => s.FramedIpAddress).HasColumnName("framed_ip_address").HasMaxLength(45);
        builder.Property(s => s.CalledStationId).HasColumnName("called_station_id").HasMaxLength(100);
        builder.Property(s => s.CallingStationId).HasColumnName("calling_station_id").HasMaxLength(100);
        builder.Property(s => s.AcctSessionTime).HasColumnName("acct_session_time");
        builder.Property(s => s.InputOctets).HasColumnName("input_octets");
        builder.Property(s => s.OutputOctets).HasColumnName("output_octets");
        builder.Property(s => s.SessionStatus).HasColumnName("session_status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(s => s.StartedAt).HasColumnName("started_at").IsRequired();
        builder.Property(s => s.StoppedAt).HasColumnName("stopped_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(s => s.SessionId).IsUnique().HasDatabaseName("ix_radius_sessions_session_id");
        builder.HasIndex(s => s.Username).HasDatabaseName("ix_radius_sessions_username");
        builder.HasIndex(s => s.FramedIpAddress).HasDatabaseName("ix_radius_sessions_framed_ip");
    }
}
