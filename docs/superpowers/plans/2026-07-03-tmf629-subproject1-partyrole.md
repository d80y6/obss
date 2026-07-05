# TMF629 Customer Entity Alignment Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Align Customer entity with TMF629 PartyRole model — add Individual/Organization engaged parties, Characteristics, CreditProfile, RelatedParty, TimePeriod, KYC documents, and PATCH support.

**Architecture:** Additive domain model with new entities (Individual, Organization, IdentityDocument, CreditProfile) and value objects (CharValue, RelatedParty, TimePeriod). Customer gets polymorphic FK to Individual or Organization. All new collections are owned/child entities with cascade delete. New MediatR commands and handlers follow existing patterns. Frontend gets new detail sections and form fields.

**Tech Stack:** .NET 9, EF Core/Npgsql, MediatR, Mapster, FluentValidation, React 19, zod

---

### Task 1: SharedKernel TimePeriod value object

**Files:**
- Create: `src/Shared/Obss.SharedKernel/Domain/ValueObjects/TimePeriod.cs`

- [ ] **Create TimePeriod record**

```csharp
namespace Obss.SharedKernel.Domain.ValueObjects;

public sealed record TimePeriod(DateTime? StartDateTime, DateTime? EndDateTime)
{
    public bool IsActive()
    {
        var now = DateTime.UtcNow;
        if (StartDateTime.HasValue && now < StartDateTime.Value)
            return false;
        if (EndDateTime.HasValue && now > EndDateTime.Value)
            return false;
        return true;
    }
}
```

- [ ] **Build to verify**

Run: `dotnet build src/Shared/Obss.SharedKernel/Obss.SharedKernel.csproj --configuration Release`
Expected: Build succeeded, 0 errors

---

### Task 2: Domain value objects (CharValue, RelatedParty, enums)

**Files:**
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/CharValue.cs`
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/RelatedParty.cs`
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/KycStatus.cs`
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/DocumentType.cs`
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/CompanyType.cs`
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/RiskRating.cs`

- [ ] **Create CharValue record**

```csharp
namespace Obss.CRM.Domain.ValueObjects;

public sealed record CharValue(string Key, string Value, string ValueType);
```

- [ ] **Create RelatedParty record**

```csharp
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Domain.ValueObjects;

public sealed record RelatedParty(string Name, string Role, Guid ReferredId, string ReferredType);
```

- [ ] **Create KycStatus enum**

```csharp
namespace Obss.CRM.Domain.ValueObjects;

public enum KycStatus
{
    NotStarted,
    Pending,
    Verified,
    Rejected
}
```

- [ ] **Create DocumentType enum**

```csharp
namespace Obss.CRM.Domain.ValueObjects;

public enum DocumentType
{
    NationalId,
    Passport,
    DriverLicense,
    ResidencePermit,
    Other
}
```

- [ ] **Create CompanyType enum**

```csharp
namespace Obss.CRM.Domain.ValueObjects;

public enum CompanyType
{
    Llc,
    Corporation,
    Partnership,
    SoleProprietorship,
    Government,
    NonProfit
}
```

- [ ] **Create RiskRating enum**

```csharp
namespace Obss.CRM.Domain.ValueObjects;

public enum RiskRating
{
    Low,
    Medium,
    High
}
```

---

### Task 3: Individual entity + IdentityDocument entity

**Files:**
- Create: `src/Modules/CRM/Obss.CRM.Domain/Entities/Individual.cs`
- Create: `src/Modules/CRM/Obss.CRM.Domain/Entities/IdentityDocument.cs`

- [ ] **Create IdentityDocument entity**

```csharp
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Entities;

public class IdentityDocument : Entity<Guid>
{
    private IdentityDocument() { }

    internal IdentityDocument(
        Guid id,
        Guid individualId,
        DocumentType documentType,
        string documentNumber,
        string? issuingAuthority,
        string? issuingCountry,
        DateTime? issuedDate,
        DateTime? expiryDate)
        : base(id)
    {
        IndividualId = individualId;
        DocumentType = documentType;
        DocumentNumber = documentNumber;
        IssuingAuthority = issuingAuthority;
        IssuingCountry = issuingCountry;
        IssuedDate = issuedDate;
        ExpiryDate = expiryDate;
        IsVerified = false;
    }

    public Guid IndividualId { get; private set; }
    public DocumentType DocumentType { get; private set; }
    public string DocumentNumber { get; private set; } = string.Empty;
    public string? IssuingAuthority { get; private set; }
    public string? IssuingCountry { get; private set; }
    public DateTime? IssuedDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public bool IsVerified { get; private set; }

    public void MarkVerified() => IsVerified = true;
}
```

- [ ] **Create Individual aggregate root**

```csharp
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Entities;

public class Individual : AggregateRoot<Guid>
{
    private readonly List<IdentityDocument> _documents = [];

    private Individual() { }

    private Individual(
        Guid id,
        string firstName,
        string lastName,
        string? middleName,
        string? salutation,
        string? title,
        DateTime? birthDate,
        string? nationality,
        string? gender)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        Salutation = salutation;
        Title = title;
        BirthDate = birthDate;
        Nationality = nationality;
        Gender = gender;
        KycStatus = KycStatus.NotStarted;
        RiskRating = RiskRating.Low;
    }

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? MiddleName { get; private set; }
    public string? Salutation { get; private set; }
    public string? Title { get; private set; }
    public DateTime? BirthDate { get; private set; }
    public string? Nationality { get; private set; }
    public string? Gender { get; private set; }
    public KycStatus KycStatus { get; private set; }
    public DateTime? KycVerifiedAt { get; private set; }
    public string? KycVerifiedBy { get; private set; }
    public RiskRating RiskRating { get; private set; }

    public IReadOnlyCollection<IdentityDocument> Documents => _documents.AsReadOnly();

    public static Individual Create(
        string firstName,
        string lastName,
        string? middleName = null,
        string? salutation = null,
        string? title = null,
        DateTime? birthDate = null,
        string? nationality = null,
        string? gender = null)
    {
        return new Individual(
            Guid.NewGuid(), firstName, lastName, middleName,
            salutation, title, birthDate, nationality, gender);
    }

    public void UpdateDetails(
        string firstName, string lastName, string? middleName,
        string? salutation, string? title, DateTime? birthDate,
        string? nationality, string? gender)
    {
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        Salutation = salutation;
        Title = title;
        BirthDate = birthDate;
        Nationality = nationality;
        Gender = gender;
    }

    public void AddDocument(IdentityDocument document)
    {
        _documents.Add(document);
    }

    public void RemoveDocument(Guid documentId)
    {
        var doc = _documents.FirstOrDefault(d => d.Id == documentId);
        if (doc is not null)
            _documents.Remove(doc);
    }

    public void VerifyKyc(string verifiedBy)
    {
        KycStatus = KycStatus.Verified;
        KycVerifiedAt = DateTime.UtcNow;
        KycVerifiedBy = verifiedBy;
    }

    public void RejectKyc()
    {
        KycStatus = KycStatus.Rejected;
    }

    public void SetRiskRating(RiskRating rating)
    {
        RiskRating = rating;
    }
}
```

---

### Task 4: Organization entity

**Files:**
- Create: `src/Modules/CRM/Obss.CRM.Domain/Entities/Organization.cs`

- [ ] **Create Organization aggregate root**

```csharp
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Entities;

public class Organization : AggregateRoot<Guid>
{
    private Organization() { }

    private Organization(
        Guid id,
        string tradingName,
        CompanyType companyType,
        string? industry,
        string? registrationNumber,
        string? taxNumber,
        string? countryOfRegistration)
        : base(id)
    {
        TradingName = tradingName;
        CompanyType = companyType;
        Industry = industry;
        RegistrationNumber = registrationNumber;
        TaxNumber = taxNumber;
        CountryOfRegistration = countryOfRegistration;
        KycStatus = KycStatus.NotStarted;
    }

    public string TradingName { get; private set; } = string.Empty;
    public CompanyType CompanyType { get; private set; }
    public string? Industry { get; private set; }
    public string? RegistrationNumber { get; private set; }
    public string? TaxNumber { get; private set; }
    public string? CountryOfRegistration { get; private set; }
    public KycStatus KycStatus { get; private set; }
    public DateTime? KycVerifiedAt { get; private set; }
    public string? KycVerifiedBy { get; private set; }

    public static Organization Create(
        string tradingName,
        CompanyType companyType,
        string? industry = null,
        string? registrationNumber = null,
        string? taxNumber = null,
        string? countryOfRegistration = null)
    {
        return new Organization(
            Guid.NewGuid(), tradingName, companyType,
            industry, registrationNumber, taxNumber, countryOfRegistration);
    }

    public void UpdateDetails(
        string tradingName, CompanyType companyType, string? industry,
        string? registrationNumber, string? taxNumber, string? countryOfRegistration)
    {
        TradingName = tradingName;
        CompanyType = companyType;
        Industry = industry;
        RegistrationNumber = registrationNumber;
        TaxNumber = taxNumber;
        CountryOfRegistration = countryOfRegistration;
    }

    public void VerifyKyc(string verifiedBy)
    {
        KycStatus = KycStatus.Verified;
        KycVerifiedAt = DateTime.UtcNow;
        KycVerifiedBy = verifiedBy;
    }

    public void RejectKyc()
    {
        KycStatus = KycStatus.Rejected;
    }
}
```

---

### Task 5: CreditProfile entity

**Files:**
- Create: `src/Modules/CRM/Obss.CRM.Domain/Entities/CreditProfile.cs`

- [ ] **Create CreditProfile entity**

```csharp
using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Domain.Entities;

public class CreditProfile : Entity<Guid>
{
    private CreditProfile() { }

    internal CreditProfile(
        Guid id,
        Guid customerId,
        int score,
        string scoreType,
        TimePeriod validFor,
        string? riskRating)
        : base(id)
    {
        CustomerId = customerId;
        Score = score;
        ScoreType = scoreType;
        ValidFor = validFor;
        RiskRating = riskRating;
    }

    public Guid CustomerId { get; private set; }
    public int Score { get; private set; }
    public string ScoreType { get; private set; } = string.Empty;
    public TimePeriod ValidFor { get; private set; } = new(null, null);
    public string? RiskRating { get; private set; }
}
```

---

### Task 6: Update Customer entity with PartyRole fields

**Files:**
- Modify: `src/Modules/CRM/Obss.CRM.Domain/Entities/Customer.cs`

- [ ] **Read current Customer.cs**
- [ ] **Add new fields and methods**

Add after existing fields:

```csharp
    public Guid? IndividualId { get; private set; }
    public Guid? OrganizationId { get; private set; }
    public Individual? Individual { get; private set; }
    public Organization? Organization { get; private set; }
    public string? Description { get; private set; }
    public string? StatusReason { get; private set; }
    public string? ExternalId { get; private set; }
    public string? Href { get; private set; }
    public TimePeriod? ValidFor { get; private set; }

    private readonly List<CharValue> _characteristics = [];
    private readonly List<CreditProfile> _creditProfiles = [];
    private readonly List<RelatedParty> _relatedParties = [];

    public IReadOnlyCollection<CharValue> Characteristics => _characteristics.AsReadOnly();
    public IReadOnlyCollection<CreditProfile> CreditProfiles => _creditProfiles.AsReadOnly();
    public IReadOnlyCollection<RelatedParty> RelatedParties => _relatedParties.AsReadOnly();
```

Add after existing methods:

```csharp
    public void SetEngagedParty(Individual individual)
    {
        Individual = individual;
        IndividualId = individual.Id;
        Organization = null;
        OrganizationId = null;
    }

    public void SetEngagedParty(Organization organization)
    {
        Organization = organization;
        OrganizationId = organization.Id;
        Individual = null;
        IndividualId = null;
    }

    public void UpdateTmfDetails(
        string? description = null,
        string? statusReason = null,
        string? externalId = null,
        string? href = null,
        TimePeriod? validFor = null)
    {
        if (description is not null) Description = description;
        if (statusReason is not null) StatusReason = statusReason;
        if (externalId is not null) ExternalId = externalId;
        if (href is not null) Href = href;
        if (validFor is not null) ValidFor = validFor;
    }

    public void SetStatus(CustomerStatus newStatus, string? reason = null)
    {
        if (newStatus == Status)
            return;

        Status = newStatus;
        if (reason is not null)
            StatusReason = reason;
    }

    public void AddCharacteristic(CharValue characteristic)
    {
        _characteristics.Add(characteristic);
    }

    public void RemoveCharacteristic(string key)
    {
        _characteristics.RemoveAll(c => c.Key == key);
    }

    public void AddCreditProfile(CreditProfile profile)
    {
        _creditProfiles.Add(profile);
    }

    public void AddRelatedParty(RelatedParty party)
    {
        _relatedParties.Add(party);
    }

    public void RemoveRelatedParty(Guid referredId)
    {
        _relatedParties.RemoveAll(r => r.ReferredId == referredId);
    }
```

- [ ] **Build domain to verify**

Run: `dotnet build src/Modules/CRM/Obss.CRM.Domain/Obss.CRM.Domain.csproj --configuration Release`
Expected: Build succeeded, 0 errors

---

### Task 7: EF configurations for all new entities

**Files:**
- Create: `src/Modules/CRM/Obss.CRM.Infrastructure/Persistence/Configurations/IndividualConfiguration.cs`
- Create: `src/Modules/CRM/Obss.CRM.Infrastructure/Persistence/Configurations/IdentityDocumentConfiguration.cs`
- Create: `src/Modules/CRM/Obss.CRM.Infrastructure/Persistence/Configurations/OrganizationConfiguration.cs`
- Create: `src/Modules/CRM/Obss.CRM.Infrastructure/Persistence/Configurations/CreditProfileConfiguration.cs`
- Create: `src/Modules/CRM/Obss.CRM.Infrastructure/Persistence/Configurations/RelatedPartyConfiguration.cs`

- [ ] **Create IndividualConfiguration.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class IndividualConfiguration : IEntityTypeConfiguration<Individual>
{
    public void Configure(EntityTypeBuilder<Individual> builder)
    {
        builder.ToTable("individuals");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();

        builder.Property(i => i.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
        builder.Property(i => i.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
        builder.Property(i => i.MiddleName).HasColumnName("middle_name").HasMaxLength(100);
        builder.Property(i => i.Salutation).HasColumnName("salutation").HasMaxLength(20);
        builder.Property(i => i.Title).HasColumnName("title").HasMaxLength(50);
        builder.Property(i => i.BirthDate).HasColumnName("birth_date");
        builder.Property(i => i.Nationality).HasColumnName("nationality").HasMaxLength(3);
        builder.Property(i => i.Gender).HasColumnName("gender").HasMaxLength(20);
        builder.Property(i => i.KycStatus).HasColumnName("kyc_status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(i => i.KycVerifiedAt).HasColumnName("kyc_verified_at");
        builder.Property(i => i.KycVerifiedBy).HasColumnName("kyc_verified_by").HasMaxLength(100);
        builder.Property(i => i.RiskRating).HasColumnName("risk_rating").HasConversion<string>().HasMaxLength(10).IsRequired();

        builder.HasMany(i => i.Documents)
            .WithOne()
            .HasForeignKey(d => d.IndividualId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Documents).AutoInclude();
    }
}
```

- [ ] **Create IdentityDocumentConfiguration.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class IdentityDocumentConfiguration : IEntityTypeConfiguration<IdentityDocument>
{
    public void Configure(EntityTypeBuilder<IdentityDocument> builder)
    {
        builder.ToTable("individual_identity_documents");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.IndividualId).HasColumnName("individual_id").IsRequired();
        builder.Property(d => d.DocumentType).HasColumnName("document_type").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(d => d.DocumentNumber).HasColumnName("document_number").HasMaxLength(100).IsRequired();
        builder.Property(d => d.IssuingAuthority).HasColumnName("issuing_authority").HasMaxLength(200);
        builder.Property(d => d.IssuingCountry).HasColumnName("issuing_country").HasMaxLength(3);
        builder.Property(d => d.IssuedDate).HasColumnName("issued_date");
        builder.Property(d => d.ExpiryDate).HasColumnName("expiry_date");
        builder.Property(d => d.IsVerified).HasColumnName("is_verified").IsRequired();
    }
}
```

- [ ] **Create OrganizationConfiguration.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();

        builder.Property(o => o.TradingName).HasColumnName("trading_name").HasMaxLength(200).IsRequired();
        builder.Property(o => o.CompanyType).HasColumnName("company_type").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(o => o.Industry).HasColumnName("industry").HasMaxLength(100);
        builder.Property(o => o.RegistrationNumber).HasColumnName("registration_number").HasMaxLength(100);
        builder.Property(o => o.TaxNumber).HasColumnName("tax_number").HasMaxLength(50);
        builder.Property(o => o.CountryOfRegistration).HasColumnName("country_of_registration").HasMaxLength(3);
        builder.Property(o => o.KycStatus).HasColumnName("kyc_status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(o => o.KycVerifiedAt).HasColumnName("kyc_verified_at");
        builder.Property(o => o.KycVerifiedBy).HasColumnName("kyc_verified_by").HasMaxLength(100);
    }
}
```

- [ ] **Create CreditProfileConfiguration.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.Entities;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

public sealed class CreditProfileConfiguration : IEntityTypeConfiguration<CreditProfile>
{
    public void Configure(EntityTypeBuilder<CreditProfile> builder)
    {
        builder.ToTable("customer_credit_profiles");

        builder.HasKey(cp => cp.Id);
        builder.Property(cp => cp.Id).ValueGeneratedNever();

        builder.Property(cp => cp.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(cp => cp.Score).HasColumnName("score").IsRequired();
        builder.Property(cp => cp.ScoreType).HasColumnName("score_type").HasMaxLength(50).IsRequired();
        builder.Property(cp => cp.RiskRating).HasColumnName("risk_rating").HasMaxLength(20);

        builder.OwnsOne(cp => cp.ValidFor, vf =>
        {
            vf.Property(p => p.StartDateTime).HasColumnName("valid_from");
            vf.Property(p => p.EndDateTime).HasColumnName("valid_until");
        });
    }
}
```

- [ ] **Create RelatedPartyConfiguration.cs**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.CRM.Domain.ValueObjects;

namespace Obss.CRM.Infrastructure.Persistence.Configurations;

// Configured as owned collection on Customer, this is just for reference
// The actual mapping will be in CustomerConfiguration
```

Actually, RelatedParty is a value object, so it should be configured as an owned collection on Customer in CustomerConfiguration.cs. Skip this file.

---

### Task 8: Update CustomerConfiguration + CrmDbContext

**Files:**
- Modify: `src/Modules/CRM/Obss.CRM.Infrastructure/Persistence/Configurations/CustomerConfiguration.cs`
- Modify: `src/Modules/CRM/Obss.CRM.Infrastructure/Persistence/CrmDbContext.cs`

- [ ] **Read current CustomerConfiguration.cs**
- [ ] **Add new columns and owned collections**

Add before closing brace of `Configure` method:

```csharp
        builder.Property(o => o.IndividualId)
            .HasColumnName("individual_id");

        builder.Property(o => o.OrganizationId)
            .HasColumnName("organization_id");

        builder.Property(o => o.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(o => o.StatusReason)
            .HasColumnName("status_reason")
            .HasMaxLength(500);

        builder.Property(o => o.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(100);

        builder.Property(o => o.Href)
            .HasColumnName("href")
            .HasMaxLength(500);

        builder.OwnsOne(o => o.ValidFor, vf =>
        {
            vf.Property(p => p.StartDateTime).HasColumnName("valid_from");
            vf.Property(p => p.EndDateTime).HasColumnName("valid_until");
        });

        builder.HasOne(o => o.Individual)
            .WithMany()
            .HasForeignKey(o => o.IndividualId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(o => o.Organization)
            .WithMany()
            .HasForeignKey(o => o.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.OwnsMany(o => o.Characteristics, ch =>
        {
            ch.ToTable("customer_characteristics");
            ch.Property<int>("Id").ValueGeneratedOnAdd();
            ch.HasKey("Id");
            ch.Property(c => c.Key).HasColumnName("key").HasMaxLength(100).IsRequired();
            ch.Property(c => c.Value).HasColumnName("value").HasMaxLength(500);
            ch.Property(c => c.ValueType).HasColumnName("value_type").HasMaxLength(30);
            ch.WithOwner().HasForeignKey("customer_id");
        });

        builder.HasMany(o => o.CreditProfiles)
            .WithOne()
            .HasForeignKey(cp => cp.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.OwnsMany(o => o.RelatedParties, rp =>
        {
            rp.ToTable("customer_related_parties");
            rp.Property<int>("Id").ValueGeneratedOnAdd();
            rp.HasKey("Id");
            rp.Property(r => r.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            rp.Property(r => r.Role).HasColumnName("role").HasMaxLength(50).IsRequired();
            rp.Property(r => r.ReferredId).HasColumnName("referred_id").IsRequired();
            rp.Property(r => r.ReferredType).HasColumnName("referred_type").HasMaxLength(50).IsRequired();
            rp.WithOwner().HasForeignKey("customer_id");
        });

        builder.Navigation(o => o.Characteristics)
            .AutoInclude();

        builder.Navigation(o => o.CreditProfiles)
            .AutoInclude();

        builder.Navigation(o => o.RelatedParties)
            .AutoInclude();
```

- [ ] **Add new DbSets to CrmDbContext.cs**

```csharp
    public DbSet<Individual> Individuals => Set<Individual>();
    public DbSet<IdentityDocument> IdentityDocuments => Set<IdentityDocument>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<CreditProfile> CreditProfiles => Set<CreditProfile>();
```

Also add corresponding `ApplyConfiguration` calls:

```csharp
    modelBuilder.ApplyConfiguration(new IndividualConfiguration());
    modelBuilder.ApplyConfiguration(new IdentityDocumentConfiguration());
    modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
    modelBuilder.ApplyConfiguration(new CreditProfileConfiguration());
```

- [ ] **Build infrastructure to verify**

Run: `dotnet build src/Modules/CRM/Obss.CRM.Infrastructure/Obss.CRM.Infrastructure.csproj --configuration Release`
Expected: Build succeeded, 0 errors

---

### Task 9: New DTOs + update CustomerDto

**Files:**
- Create: `src/Modules/CRM/Obss.CRM.Application/DTOs/IndividualDto.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/DTOs/IdentityDocumentDto.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/DTOs/OrganizationDto.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/DTOs/CharValueDto.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/DTOs/CreditProfileDto.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/DTOs/RelatedPartyDto.cs`
- Modify: `src/Modules/CRM/Obss.CRM.Application/DTOs/CustomerDto.cs`

- [ ] **Create IndividualDto**

```csharp
namespace Obss.CRM.Application.DTOs;

public sealed record IndividualDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? Salutation,
    string? Title,
    DateTime? BirthDate,
    string? Nationality,
    string? Gender,
    string KycStatus,
    DateTime? KycVerifiedAt,
    string? KycVerifiedBy,
    string RiskRating,
    List<IdentityDocumentDto> Documents);
```

- [ ] **Create IdentityDocumentDto**

```csharp
namespace Obss.CRM.Application.DTOs;

public sealed record IdentityDocumentDto(
    Guid Id,
    Guid IndividualId,
    string DocumentType,
    string DocumentNumber,
    string? IssuingAuthority,
    string? IssuingCountry,
    DateTime? IssuedDate,
    DateTime? ExpiryDate,
    bool IsVerified);
```

- [ ] **Create OrganizationDto**

```csharp
namespace Obss.CRM.Application.DTOs;

public sealed record OrganizationDto(
    Guid Id,
    string TradingName,
    string CompanyType,
    string? Industry,
    string? RegistrationNumber,
    string? TaxNumber,
    string? CountryOfRegistration,
    string KycStatus,
    DateTime? KycVerifiedAt,
    string? KycVerifiedBy);
```

- [ ] **Create CharValueDto**

```csharp
namespace Obss.CRM.Application.DTOs;

public sealed record CharValueDto(string Key, string Value, string ValueType);
```

- [ ] **Create CreditProfileDto**

```csharp
namespace Obss.CRM.Application.DTOs;

public sealed record CreditProfileDto(
    Guid Id,
    Guid CustomerId,
    int Score,
    string ScoreType,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    string? RiskRating);
```

- [ ] **Create RelatedPartyDto**

```csharp
namespace Obss.CRM.Application.DTOs;

public sealed record RelatedPartyDto(string Name, string Role, Guid ReferredId, string ReferredType);
```

- [ ] **Read current CustomerDto.cs and add new fields**

Add after existing fields:

```csharp
    string? Description,
    string? StatusReason,
    string? ExternalId,
    string? Href,
    DateTime? ValidFrom,
    DateTime? ValidUntil,
    IndividualDto? Individual,
    OrganizationDto? Organization,
    List<CharValueDto> Characteristics,
    List<CreditProfileDto> CreditProfiles,
    List<RelatedPartyDto> RelatedParties,
```

---

### Task 10: Update CrmMappingConfig

**Files:**
- Modify: `src/Modules/CRM/Obss.CRM.Application/Mappings/CrmMappingConfig.cs`

- [ ] **Read current CrmMappingConfig.cs**
- [ ] **Add new mappings**

```csharp
        TypeAdapterConfig<Individual, IndividualDto>.NewConfig()
            .Map(dest => dest.KycStatus, src => src.KycStatus.ToString())
            .Map(dest => dest.RiskRating, src => src.RiskRating.ToString())
            .Map(dest => dest.Documents, src => src.Documents.Adapt<List<IdentityDocumentDto>>());

        TypeAdapterConfig<IdentityDocument, IdentityDocumentDto>.NewConfig()
            .Map(dest => dest.DocumentType, src => src.DocumentType.ToString());

        TypeAdapterConfig<Organization, OrganizationDto>.NewConfig()
            .Map(dest => dest.CompanyType, src => src.CompanyType.ToString())
            .Map(dest => dest.KycStatus, src => src.KycStatus.ToString());

        TypeAdapterConfig<CreditProfile, CreditProfileDto>.NewConfig()
            .Map(dest => dest.ValidFrom, src => src.ValidFor.StartDateTime)
            .Map(dest => dest.ValidUntil, src => src.ValidFor.EndDateTime);

        // Update Customer-to-CustomerDto mapping
        // (the new fields auto-map by convention since names match)
        // But need explicit maps for nested objects:
```

Add to the existing Customer -> CustomerDto config (or add a new one):

```csharp
        TypeAdapterConfig<Customer, CustomerDto>.NewConfig()
            // Keep existing mappings...
            .Map(dest => dest.ValidFrom, src => src.ValidFor != null ? src.ValidFor.StartDateTime : null)
            .Map(dest => dest.ValidUntil, src => src.ValidFor != null ? src.ValidFor.EndDateTime : null)
            .Map(dest => dest.Individual, src => src.Individual != null ? src.Individual.Adapt<IndividualDto>() : null)
            .Map(dest => dest.Organization, src => src.Organization != null ? src.Organization.Adapt<OrganizationDto>() : null)
            .Map(dest => dest.Characteristics, src => src.Characteristics.Adapt<List<CharValueDto>>())
            .Map(dest => dest.CreditProfiles, src => src.CreditProfiles.Adapt<List<CreditProfileDto>>())
            .Map(dest => dest.RelatedParties, src => src.RelatedParties.Adapt<List<RelatedPartyDto>>());
```

---

### Task 11: Application Commands — Part 1 (PATCH + Individual CRUD)

**Files:**
- Create: `src/Modules/CRM/Obss.CRM.Application/Commands/PartialUpdateCustomer/PartialUpdateCustomerCommand.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/Commands/PartialUpdateCustomer/PartialUpdateCustomerCommandHandler.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/Commands/PartialUpdateCustomer/PartialUpdateCustomerValidator.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/Commands/CreateIndividual/CreateIndividualCommand.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/Commands/CreateIndividual/CreateIndividualCommandHandler.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/Commands/CreateIndividual/CreateIndividualValidator.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/Commands/UpdateIndividual/UpdateIndividualCommand.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/Commands/UpdateIndividual/UpdateIndividualCommandHandler.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/Commands/UpdateIndividual/UpdateIndividualValidator.cs`

- [ ] **Create PartialUpdateCustomerCommand**

```csharp
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.PartialUpdateCustomer;

public sealed record PartialUpdateCustomerCommand(
    Guid Id,
    string? Description,
    string? StatusReason,
    string? ExternalId,
    DateTime? ValidFrom,
    DateTime? ValidUntil) : IRequest<Result<CustomerDto>>;
```

- [ ] **Create PartialUpdateCustomerCommandHandler**

Handler follows pattern: get customer by id, call `UpdateTmfDetails()`, save, return adapted DTO.

```csharp
using Mapster;
using MediatR;
using Obss.CRM.Application.Abstractions;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Application.Commands.PartialUpdateCustomer;

public sealed class PartialUpdateCustomerCommandHandler : IRequestHandler<PartialUpdateCustomerCommand, Result<CustomerDto>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PartialUpdateCustomerCommandHandler(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerDto>> Handle(PartialUpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.Id, cancellationToken);
        if (customer is null)
            return Result.Failure<CustomerDto>(Error.NotFound("Customer", request.Id));

        TimePeriod? validFor = null;
        if (request.ValidFrom.HasValue || request.ValidUntil.HasValue)
            validFor = new TimePeriod(request.ValidFrom, request.ValidUntil);

        customer.UpdateTmfDetails(
            request.Description,
            request.StatusReason,
            request.ExternalId,
            validFor: validFor);

        await _customerRepository.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(customer.Adapt<CustomerDto>());
    }
}
```

- [ ] **Create PartialUpdateCustomerValidator**

```csharp
using FluentValidation;

namespace Obss.CRM.Application.Commands.PartialUpdateCustomer;

internal sealed class PartialUpdateCustomerValidator : AbstractValidator<PartialUpdateCustomerCommand>
{
    public PartialUpdateCustomerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.StatusReason).MaximumLength(500);
        RuleFor(x => x.ExternalId).MaximumLength(100);
    }
}
```

- [ ] **Create CreateIndividualCommand**

```csharp
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.CreateIndividual;

public sealed record CreateIndividualCommand(
    string FirstName,
    string LastName,
    string? MiddleName,
    string? Salutation,
    string? Title,
    DateTime? BirthDate,
    string? Nationality,
    string? Gender) : IRequest<Result<IndividualDto>>;
```

- [ ] **Create CreateIndividualCommandHandler**

Handler creates a new Individual entity, saves it, returns DTO. Since Individual is its own aggregate, it uses IRepository<Individual> or a new IIndividualRepository.

- [ ] **Create CreateIndividualValidator**

Validates FirstName and LastName required, max lengths.

- [ ] **Create UpdateIndividualCommand**

Similar shape with Id + all optional fields.

- [ ] **Create UpdateIndividualCommandHandler**

Gets by Id, calls UpdateDetails, saves.

- [ ] **Create UpdateIndividualValidator**

- [ ] **Build application layer**

Run: `dotnet build src/Modules/CRM/Obss.CRM.Application/Obss.CRM.Application.csproj --configuration Release`
Expected: Build succeeded, 0 errors

---

### Task 12: Application Commands — Part 2 (Organization CRUD + Characteristics + CreditProfile + RelatedParty + KYC)

**Files:**
- Create: Organization CRUD commands (6 files)
- Create: AddCharacteristic/RemoveCharacteristic (6 files)
- Create: AddCreditProfile (3 files)
- Create: AddRelatedParty/RemoveRelatedParty (6 files)
- Create: VerifyCustomerKyc (3 files)
- Create: AddIdentityDocument/RemoveIdentityDocument (6 files)
- Create: GetCreditProfiles query (2 files)

- [ ] **Create Organization CRUD** — follows same pattern as Individual

CreateOrganizationCommand(CreateOrganizationCommandHandler, CreateOrganizationValidator)
UpdateOrganizationCommand(UpdateOrganizationCommandHandler, UpdateOrganizationValidator)

- [ ] **Create AddCharacteristicCommand**

```csharp
public sealed record AddCharacteristicCommand(Guid CustomerId, string Key, string Value, string ValueType) : IRequest<Result>;
```

Handler gets customer, calls `customer.AddCharacteristic(new CharValue(...))`, saves.

- [ ] **Create RemoveCharacteristicCommand**

```csharp
public sealed record RemoveCharacteristicCommand(Guid CustomerId, string Key) : IRequest<Result>;
```

- [ ] **Create AddCreditProfileCommand**

```csharp
public sealed record AddCreditProfileCommand(
    Guid CustomerId, int Score, string ScoreType,
    DateTime? ValidFrom, DateTime? ValidUntil, string? RiskRating) : IRequest<Result>;
```

Handler creates `new CreditProfile(...)` and calls `customer.AddCreditProfile(...)`.

- [ ] **Create AddRelatedPartyCommand**

```csharp
public sealed record AddRelatedPartyCommand(
    Guid CustomerId, string Name, string Role,
    Guid ReferredId, string ReferredType) : IRequest<Result>;
```

- [ ] **Create RemoveRelatedPartyCommand**

```csharp
public sealed record RemoveRelatedPartyCommand(Guid CustomerId, Guid ReferredId) : IRequest<Result>;
```

- [ ] **Create VerifyCustomerKycCommand**

```csharp
public sealed record VerifyCustomerKycCommand(Guid CustomerId, string VerifiedBy, bool IsApproved) : IRequest<Result>;
```

Handler gets customer, checks if Individual exists or Organization exists:
```csharp
// In handler:
if (customer.Individual is not null)
{
    if (request.IsApproved)
        customer.Individual.VerifyKyc(request.VerifiedBy);
    else
        customer.Individual.RejectKyc();
}
else if (customer.Organization is not null)
{
    if (request.IsApproved)
        customer.Organization.VerifyKyc(request.VerifiedBy);
    else
        customer.Organization.RejectKyc();
}
```

- [ ] **Create AddIdentityDocumentCommand**

```csharp
public sealed record AddIdentityDocumentCommand(
    Guid IndividualId, string DocumentType, string DocumentNumber,
    string? IssuingAuthority, string? IssuingCountry,
    DateTime? IssuedDate, DateTime? ExpiryDate) : IRequest<Result>;
```

- [ ] **Create RemoveIdentityDocumentCommand**

```csharp
public sealed record RemoveIdentityDocumentCommand(Guid IndividualId, Guid DocumentId) : IRequest<Result>;
```

- [ ] **Create GetCreditProfilesQuery + Handler**

```csharp
public sealed record GetCreditProfilesQuery(Guid CustomerId) : IRequest<Result<IReadOnlyList<CreditProfileDto>>>;
```

Handler loads customer and maps credit profiles.

- [ ] **Build**

Run: `dotnet build src/Modules/CRM/Obss.CRM.Application/Obss.CRM.Application.csproj --configuration Release`
Expected: Build succeeded, 0 errors

---

### Task 13: API Endpoints

**Files:**
- Modify: `src/Modules/CRM/Obss.CRM.Api/Endpoints/CustomerEndpoints.cs`
- Create: `src/Modules/CRM/Obss.CRM.Api/Endpoints/PartyEndpoints.cs`

- [ ] **Read current CustomerEndpoints.cs**

- [ ] **Add PATCH, Characteristic, CreditProfile, RelatedParty, KYC endpoints to CustomerEndpoints.cs**

Add before closing brace of `Map` method:

```csharp
        group.MapPatch("/customers/{id:guid}", async (Guid id, PartialUpdateCustomerCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/characteristics", async (Guid id, AddCharacteristicCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId) return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess ? TypedResults.NoContent() : TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/customers/{id:guid}/characteristics/{key}", async (Guid id, string key, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveCharacteristicCommand(id, key));
            return result.IsSuccess ? TypedResults.NoContent() : TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/credit-profiles", async (Guid id, AddCreditProfileCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId) return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess ? TypedResults.NoContent() : TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/customers/{id:guid}/credit-profiles", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCreditProfilesQuery(id));
            return result.IsSuccess ? TypedResults.Ok(result.Value) : TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/related-parties", async (Guid id, AddRelatedPartyCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId) return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess ? TypedResults.NoContent() : TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/customers/{id:guid}/related-parties/{referredId:guid}", async (Guid id, Guid referredId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveRelatedPartyCommand(id, referredId));
            return result.IsSuccess ? TypedResults.NoContent() : TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/customers/{id:guid}/kyc-verify", async (Guid id, VerifyCustomerKycCommand command, IMediator mediator) =>
        {
            if (id != command.CustomerId) return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess ? TypedResults.NoContent() : TypedResults.BadRequest(result.Error);
        });
```

- [ ] **Create PartyEndpoints.cs** for Individual and Organization CRUD

```csharp
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.CRM.Application.Commands.AddIdentityDocument;
using Obss.CRM.Application.Commands.CreateIndividual;
using Obss.CRM.Application.Commands.RemoveIdentityDocument;
using Obss.CRM.Application.Commands.UpdateIndividual;
using Obss.CRM.Application.Commands.CreateOrganization;
using Obss.CRM.Application.Commands.UpdateOrganization;
using Obss.CRM.Application.Queries.GetIndividualById;
using Obss.CRM.Application.Queries.GetOrganizationById;

namespace Obss.CRM.Api.Endpoints;

public static class PartyEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        // Individuals
        group.MapPost("/individuals", async (CreateIndividualCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? TypedResults.Created($"/api/v1/crm/individuals/{result.Value.Id}", result.Value)
                : TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/individuals/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetIndividualByIdQuery(id));
            return result.IsSuccess
                ? TypedResults.Ok(result.Value)
                : TypedResults.NotFound(result.Error);
        });

        group.MapPut("/individuals/{id:guid}", async (Guid id, UpdateIndividualCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? TypedResults.Ok(result.Value)
                : TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/individuals/{individualId:guid}/documents", async (Guid individualId, AddIdentityDocumentCommand command, IMediator mediator) =>
        {
            if (individualId != command.IndividualId) return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess ? TypedResults.NoContent() : TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/individuals/{individualId:guid}/documents/{docId:guid}", async (Guid individualId, Guid docId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveIdentityDocumentCommand(individualId, docId));
            return result.IsSuccess ? TypedResults.NoContent() : TypedResults.BadRequest(result.Error);
        });

        // Organizations
        group.MapPost("/organizations", async (CreateOrganizationCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? TypedResults.Created($"/api/v1/crm/organizations/{result.Value.Id}", result.Value)
                : TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/organizations/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOrganizationByIdQuery(id));
            return result.IsSuccess
                ? TypedResults.Ok(result.Value)
                : TypedResults.NotFound(result.Error);
        });

        group.MapPut("/organizations/{id:guid}", async (Guid id, UpdateOrganizationCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? TypedResults.Ok(result.Value)
                : TypedResults.BadRequest(result.Error);
        });
    }
}
```

- [ ] **Register PartyEndpoints in CrmModuleRegistration.cs**

```csharp
    CustomerEndpoints.Map(group);
    PartyEndpoints.Map(group);  // Add this line
    LookupEndpoints.Map(group);
```

- [ ] **Build API project**

Run: `dotnet build src/Modules/CRM/Obss.CRM.Api/Obss.CRM.Api.csproj --configuration Release`
Expected: Build succeeded, 0 errors

---

### Task 14: Frontend — Types and DTOs

**Files:**
- Modify: `frontend/src/api/generated/dto.ts`
- Modify: `frontend/src/types/api.ts`

- [ ] **Add new interfaces to dto.ts**

```typescript
export interface IndividualDto {
  id: string;
  firstName: string;
  lastName: string;
  middleName: string | null;
  salutation: string | null;
  title: string | null;
  birthDate: string | null;
  nationality: string | null;
  gender: string | null;
  kycStatus: string;
  kycVerifiedAt: string | null;
  kycVerifiedBy: string | null;
  riskRating: string;
  documents: IdentityDocumentDto[];
}

export interface IdentityDocumentDto {
  id: string;
  individualId: string;
  documentType: string;
  documentNumber: string;
  issuingAuthority: string | null;
  issuingCountry: string | null;
  issuedDate: string | null;
  expiryDate: string | null;
  isVerified: boolean;
}

export interface OrganizationDto {
  id: string;
  tradingName: string;
  companyType: string;
  industry: string | null;
  registrationNumber: string | null;
  taxNumber: string | null;
  countryOfRegistration: string | null;
  kycStatus: string;
  kycVerifiedAt: string | null;
  kycVerifiedBy: string | null;
}

export interface CharValueDto {
  key: string;
  value: string;
  valueType: string;
}

export interface CreditProfileDto {
  id: string;
  customerId: string;
  score: number;
  scoreType: string;
  validFrom: string | null;
  validUntil: string | null;
  riskRating: string | null;
}

export interface RelatedPartyDto {
  name: string;
  role: string;
  referredId: string;
  referredType: string;
}
```

- [ ] **Update CustomerDto** — add new fields after statusReason:

```typescript
export interface CustomerDto {
  // ...existing fields...
  statusReason: string | null;
  description: string | null;
  externalId: string | null;
  href: string | null;
  validFrom: string | null;
  validUntil: string | null;
  individual: IndividualDto | null;
  organization: OrganizationDto | null;
  characteristics: CharValueDto[];
  creditProfiles: CreditProfileDto[];
  relatedParties: RelatedPartyDto[];
  // ...existing fields after...
}
```

- [ ] **Update types/api.ts exports to include new types**

---

### Task 15: Frontend — Detail page sections

**Files:**
- Modify: `frontend/src/app/customers/[id]/page.tsx`

- [ ] **Read current customer detail page**
- [ ] **Add new sections to the Overview tab:**
  - Individual/Organization info card (name, KYC status, risk rating)
  - Characteristics table (key, value, type)
  - Credit Profile history table
  - Related Parties list
  - Identity Documents (if Individual)

- [ ] **Add KYC verify button** for Draft/Active customers

---

### Task 16: Frontend — Create/Edit form updates

**Files:**
- Modify: `frontend/src/app/customers/new/page.tsx`
- Modify: `frontend/src/app/customers/[id]/edit/page.tsx`

- [ ] **Update create form** — add description field, option to create Individual or Organization engaged party
- [ ] **Update edit form** — use PATCH, add description, validFor date pickers

---

### Task 17: Frontend — Build verification

Run: `cd frontend && npx tsc --noEmit`
Expected: No TypeScript errors

---

### Task 18: Final backend build

Run: `dotnet build src/Modules/CRM/Obss.CRM.Domain/Obss.CRM.Domain.csproj --configuration Release && dotnet build src/Modules/CRM/Obss.CRM.Application/Obss.CRM.Application.csproj --configuration Release && dotnet build src/Modules/CRM/Obss.CRM.Infrastructure/Obss.CRM.Infrastructure.csproj --configuration Release && dotnet build src/Modules/CRM/Obss.CRM.Api/Obss.CRM.Api.csproj --configuration Release`
Expected: All 4 projects build with 0 errors
