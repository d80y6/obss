using Xunit;
using FluentAssertions;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Domain.Tests;

public class TaxExemptionTests
{
    [Fact]
    public void Create_ShouldSetProperties()
    {
        var customerId = Guid.NewGuid();
        var taxRuleId = Guid.NewGuid();
        var validFrom = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var validTo = new DateTime(2025, 12, 31, 0, 0, 0, DateTimeKind.Utc);

        var exemption = TaxExemption.Create(
            "tenant-1",
            customerId,
            taxRuleId,
            "CERT-001",
            0.5m,
            validFrom,
            validTo,
            "admin@example.com");

        exemption.Id.Should().NotBeEmpty();
        exemption.TenantId.Should().Be("tenant-1");
        exemption.CustomerId.Should().Be(customerId);
        exemption.TaxRuleId.Should().Be(taxRuleId);
        exemption.ExemptionCertificate.Should().Be("CERT-001");
        exemption.ExemptionRate.Should().Be(0.5m);
        exemption.ValidFrom.Should().Be(validFrom);
        exemption.ValidTo.Should().Be(validTo);
        exemption.ApprovedBy.Should().Be("admin@example.com");
    }

    [Fact]
    public void IsValid_WhenWithinRange_ShouldReturnTrue()
    {
        var exemption = TaxExemption.Create(
            "tenant-1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CERT-002",
            0.5m,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30),
            "admin");

        var result = exemption.IsValid();

        result.Should().BeTrue();
    }

    [Fact]
    public void IsValid_WhenBeforeValidFrom_ShouldReturnFalse()
    {
        var exemption = TaxExemption.Create(
            "tenant-1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CERT-003",
            0.5m,
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(60),
            "admin");

        var result = exemption.IsValid();

        result.Should().BeFalse();
    }

    [Fact]
    public void IsValid_WhenAfterValidTo_ShouldReturnFalse()
    {
        var exemption = TaxExemption.Create(
            "tenant-1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CERT-004",
            0.5m,
            DateTime.UtcNow.AddDays(-60),
            DateTime.UtcNow.AddDays(-30),
            "admin");

        var result = exemption.IsValid();

        result.Should().BeFalse();
    }

    [Fact]
    public void GetEffectiveRate_ShouldReduceOriginalRate()
    {
        var exemption = TaxExemption.Create(
            "tenant-1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CERT-005",
            0.5m,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30),
            "admin");

        var effectiveRate = exemption.GetEffectiveRate(0.15m);

        effectiveRate.Should().Be(0.075m);
    }

    [Fact]
    public void GetEffectiveRate_WithZeroExemption_ShouldReturnOriginal()
    {
        var exemption = TaxExemption.Create(
            "tenant-1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CERT-006",
            0m,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30),
            "admin");

        var effectiveRate = exemption.GetEffectiveRate(0.15m);

        effectiveRate.Should().Be(0.15m);
    }

    [Fact]
    public void GetEffectiveRate_WithFullExemption_ShouldReturnZero()
    {
        var exemption = TaxExemption.Create(
            "tenant-1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            "CERT-007",
            1m,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(30),
            "admin");

        var effectiveRate = exemption.GetEffectiveRate(0.15m);

        effectiveRate.Should().Be(0m);
    }
}
