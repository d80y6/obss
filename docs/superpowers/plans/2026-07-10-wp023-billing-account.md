# WP-023: BillingAccount TMF666 — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Complete the TMF666 BillingAccount resource by adding AccountBalance, BillPresentationMedia, AccountHolder, PATCH/DELETE endpoints, RelatedParty management, integration events, and frontend pages.

**Architecture:** Enhances existing `Obss.Billing` module (4-layer: Domain/Application/Infrastructure/Api) with new aggregates, owned entities, and value objects. Follows same CQRS/MediatR/EF Core patterns as the existing module.

**Tech Stack:** .NET 9, EF Core + Npgsql, Mapster, FluentValidation, MediatR, Next.js 16 + React 19 + TanStack Query

---

### Task 1: Domain — New Entities, Value Objects, Enums, and Events

**Files:**
- Create: `src/Modules/Billing/Obss.Billing.Domain/Entities/AccountBalance.cs`
- Create: `src/Modules/Billing/Obss.Billing.Domain/Entities/BalanceTransaction.cs`
- Create: `src/Modules/Billing/Obss.Billing.Domain/Entities/BillPresentationMedia.cs`
- Create: `src/Modules/Billing/Obss.Billing.Domain/ValueObjects/AccountHolder.cs`
- Create: `src/Modules/Billing/Obss.Billing.Domain/ValueObjects/TransactionType.cs`
- Create: `src/Modules/Billing/Obss.Billing.Domain/ValueObjects/MediaType.cs`
- Create: `src/Modules/Billing/Obss.Billing.Domain/Events/BillingAccountCreatedEvent.cs`
- Create: `src/Modules/Billing/Obss.Billing.Domain/Events/BillingAccountUpdatedEvent.cs`
- Create: `src/Modules/Billing/Obss.Billing.Domain/Events/BillingAccountDeletedEvent.cs`
- Create: `src/Modules/Billing/Obss.Billing.Domain/Events/BalanceChangedEvent.cs`

- [ ] **Step 1: Create AccountBalance aggregate**

```csharp
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Entities;

public class AccountBalance : AggregateRoot<Guid>
{
    private readonly List<BalanceTransaction> _transactions = [];

    private AccountBalance() { }

    public AccountBalance(Guid billingAccountId, decimal currentBalance, decimal outstandingBalance, decimal availableCredit, string currency)
    {
        Id = Guid.NewGuid();
        BillingAccountId = billingAccountId;
        CurrentBalance = currentBalance;
        OutstandingBalance = outstandingBalance;
        AvailableCredit = availableCredit;
        Currency = currency;
        BalanceDate = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public Guid BillingAccountId { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public decimal OutstandingBalance { get; private set; }
    public decimal AvailableCredit { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime BalanceDate { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    public string? AtType { get; private set; } = "AccountBalance";
    public string? AtBaseType { get; private set; } = "PartyBalance";
    public string? AtSchemaLocation { get; private set; }

    public IReadOnlyCollection<BalanceTransaction> Transactions => _transactions.AsReadOnly();

    public void RecordTransaction(decimal amount, TransactionType type, string description, string? referenceId, string? referenceType)
    {
        CurrentBalance += amount;
        LastUpdatedAt = DateTime.UtcNow;
        _transactions.Add(new BalanceTransaction(Id, amount, type, description, referenceId, referenceType));
    }
}
```

- [ ] **Step 2: Create BalanceTransaction entity**

```csharp
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Entities;

public class BalanceTransaction : Entity<Guid>
{
    private BalanceTransaction() { }

    public BalanceTransaction(Guid balanceId, decimal amount, TransactionType transactionType, string description, string? referenceId, string? referenceType)
    {
        Id = Guid.NewGuid();
        BalanceId = balanceId;
        Amount = amount;
        TransactionType = transactionType;
        Description = description;
        TransactionDate = DateTime.UtcNow;
        ReferenceId = referenceId;
        ReferenceType = referenceType;
    }

    public Guid BalanceId { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType TransactionType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime TransactionDate { get; private set; }
    public string? ReferenceId { get; private set; }
    public string? ReferenceType { get; private set; }
}
```

- [ ] **Step 3: Create BillPresentationMedia entity**

```csharp
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Entities;

public class BillPresentationMedia : Entity<Guid>
{
    private BillPresentationMedia() { }

    public BillPresentationMedia(string mediaType, string? emailAddress, string? paperFormat, string language, bool isPreferred)
    {
        Id = Guid.NewGuid();
        MediaType = mediaType;
        EmailAddress = emailAddress;
        PaperFormat = paperFormat;
        Language = language;
        IsPreferred = isPreferred;
        ValidFrom = DateTime.UtcNow;
    }

    public string MediaType { get; private set; } = string.Empty;
    public string? EmailAddress { get; private set; }
    public string? PaperFormat { get; private set; }
    public string Language { get; private set; } = "en";
    public bool IsPreferred { get; private set; }
    public DateTime? ValidFrom { get; private set; }
    public DateTime? ValidUntil { get; private set; }

    public void Update(string? emailAddress, string? paperFormat, string? language, bool? isPreferred)
    {
        if (emailAddress is not null) EmailAddress = emailAddress;
        if (paperFormat is not null) PaperFormat = paperFormat;
        if (language is not null) Language = language;
        if (isPreferred.HasValue) IsPreferred = isPreferred.Value;
    }

    public void Deactivate() => ValidUntil = DateTime.UtcNow;
}
```

- [ ] **Step 4: Create AccountHolder value object**

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.ValueObjects;

public sealed record AccountHolder
{
    public AccountHolder(string name, string? email, string? phone, Guid? contactId)
    {
        Name = name;
        Email = email;
        Phone = phone;
        ContactId = contactId;
    }

    public string Name { get; }
    public string? Email { get; }
    public string? Phone { get; }
    public Guid? ContactId { get; }
}
```

- [ ] **Step 5: Create TransactionType enum**

```csharp
namespace Obss.Billing.Domain.ValueObjects;

public enum TransactionType
{
    Charge = 1,
    Payment = 2,
    Credit = 3,
    Debit = 4,
    Adjustment = 5,
    Refund = 6
}
```

- [ ] **Step 6: Create MediaType enum**

```csharp
namespace Obss.Billing.Domain.ValueObjects;

public enum MediaType
{
    Email = 1,
    Paper = 2,
    Portal = 3
}
```

- [ ] **Step 7: Create BillingAccountCreatedEvent**

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Events;

public sealed class BillingAccountCreatedEvent : DomainEvent
{
    public BillingAccountCreatedEvent(Guid billingAccountId, Guid customerId, string accountType)
    {
        BillingAccountId = billingAccountId;
        CustomerId = customerId;
        AccountType = accountType;
    }

    public Guid BillingAccountId { get; }
    public Guid CustomerId { get; }
    public string AccountType { get; }
}
```

- [ ] **Step 8: Create BillingAccountUpdatedEvent**

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Events;

public sealed class BillingAccountUpdatedEvent : DomainEvent
{
    public BillingAccountUpdatedEvent(Guid billingAccountId, string name, string status)
    {
        BillingAccountId = billingAccountId;
        Name = name;
        Status = status;
    }

    public Guid BillingAccountId { get; }
    public string Name { get; }
    public string Status { get; }
}
```

- [ ] **Step 9: Create BillingAccountDeletedEvent**

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Events;

public sealed class BillingAccountDeletedEvent : DomainEvent
{
    public BillingAccountDeletedEvent(Guid billingAccountId)
    {
        BillingAccountId = billingAccountId;
    }

    public Guid BillingAccountId { get; }
}
```

- [ ] **Step 10: Create BalanceChangedEvent**

```csharp
using Obss.SharedKernel.Domain.Common;

namespace Obss.Billing.Domain.Events;

public sealed class BalanceChangedEvent : DomainEvent
{
    public BalanceChangedEvent(Guid billingAccountId, decimal previousBalance, decimal newBalance, string currency)
    {
        BillingAccountId = billingAccountId;
        PreviousBalance = previousBalance;
        NewBalance = newBalance;
        Currency = currency;
    }

    public Guid BillingAccountId { get; }
    public decimal PreviousBalance { get; }
    public decimal NewBalance { get; }
    public string Currency { get; }
}
```

- [ ] **Step 11: Verify build**

Run: `dotnet build src/Modules/Billing/Obss.Billing.Domain/Obss.Billing.Domain.csproj`

Expected: Build succeeded with 0 warnings and 0 errors.

- [ ] **Step 12: Commit**

```bash
git add src/Modules/Billing/Obss.Billing.Domain/
git commit -m "feat: add AccountBalance, BillPresentationMedia, AccountHolder, domain events for TMF666"
```

---

### Task 2: Domain — Enhance Existing Entities

**Files:**
- Modify: `src/Modules/Billing/Obss.Billing.Domain/Entities/BillingAccount.cs` — add AccountHolder, BillPresentationMedia, PaymentMethodId, new domain events
- Modify: `src/Modules/Billing/Obss.Billing.Domain/Entities/TaxExemption.cs` — add BillingAccountId

- [ ] **Step 1: Enhance BillingAccount entity**

Read existing file. Add to the existing `BillingAccount` class:

Imports: add `using Obss.Billing.Domain.Events;` and `using Obss.Billing.Domain.ValueObjects;`

Add private fields:
```csharp
private readonly List<BillPresentationMedia> _billPresentationMedia = [];
```

Add new properties after `ExternalId`:
```csharp
public AccountHolder? AccountHolder { get; private set; }
public string? PaymentMethodId { get; private set; }
public IReadOnlyCollection<BillPresentationMedia> BillPresentationMedia => _billPresentationMedia.AsReadOnly();
```

Add new methods after `AddRelatedParty`:
```csharp
public void SetAccountHolder(AccountHolder accountHolder)
{
    AccountHolder = accountHolder;
    UpdatedAt = DateTime.UtcNow;
}

public void SetPaymentMethodId(string? paymentMethodId)
{
    PaymentMethodId = paymentMethodId;
    UpdatedAt = DateTime.UtcNow;
}

public void AddBillPresentationMedia(BillPresentationMedia media) => _billPresentationMedia.Add(media);

public void RemoveBillPresentationMedia(Guid mediaId)
{
    var media = _billPresentationMedia.FirstOrDefault(m => m.Id == mediaId);
    if (media is not null)
        _billPresentationMedia.Remove(media);
}

public void MarkDeleted()
{
    IsActive = false;
    Status = "Deleted";
    UpdatedAt = DateTime.UtcNow;
    AddDomainEvent(new BillingAccountDeletedEvent(Id));
}
```

Update constructor to fire `BillingAccountCreatedEvent`:
```csharp
public BillingAccount(Guid customerId, AccountType accountType, string name, decimal creditLimit, string currency)
{
    Id = Guid.NewGuid();
    CustomerId = customerId;
    AccountType = accountType;
    Name = name;
    CreditLimit = creditLimit;
    Currency = currency;
    Status = "Active";
    IsActive = true;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
    AddDomainEvent(new BillingAccountCreatedEvent(Id, customerId, accountType.ToString()));
}
```

Update `UpdateDetails` to fire updated event:
```csharp
public void UpdateDetails(string name, decimal creditLimit, string currency, string? description)
{
    Name = name;
    CreditLimit = creditLimit;
    Currency = currency;
    Description = description;
    UpdatedAt = DateTime.UtcNow;
    AddDomainEvent(new BillingAccountUpdatedEvent(Id, name, Status));
}
```

- [ ] **Step 2: Enhance TaxExemption with BillingAccountId**

Replace the private constructor's parameter list:
```csharp
private TaxExemption(
    Guid id,
    string tenantId,
    Guid customerId,
    Guid taxRuleId,
    string exemptionCertificate,
    decimal exemptionRate,
    DateTime validFrom,
    DateTime validTo,
    string approvedBy)
    : base(id)
```

With:
```csharp
private TaxExemption(
    Guid id,
    string tenantId,
    Guid customerId,
    Guid? billingAccountId,
    Guid taxRuleId,
    string exemptionCertificate,
    decimal exemptionRate,
    DateTime validFrom,
    DateTime validTo,
    string approvedBy)
    : base(id)
{
    TenantId = tenantId;
    CustomerId = customerId;
    BillingAccountId = billingAccountId;
    TaxRuleId = taxRuleId;
    ExemptionCertificate = exemptionCertificate;
    ExemptionRate = exemptionRate;
    ValidFrom = validFrom;
    ValidTo = validTo;
    ApprovedBy = approvedBy;
    CreatedAt = DateTime.UtcNow;
}
```

Add property after `CustomerId`:
```csharp
public Guid? BillingAccountId { get; private set; }
```

Update `Create` static method signature:
```csharp
public static TaxExemption Create(
    string tenantId,
    Guid customerId,
    Guid? billingAccountId,
    Guid taxRuleId,
    string exemptionCertificate,
    decimal exemptionRate,
    DateTime validFrom,
    DateTime validTo,
    string approvedBy)
{
    return new TaxExemption(
        Guid.NewGuid(),
        tenantId,
        customerId,
        billingAccountId,
        taxRuleId,
        exemptionCertificate,
        exemptionRate,
        validFrom,
        validTo,
        approvedBy);
}
```

- [ ] **Step 3: Update existing ApplyTaxExemptionCommandHandler for new signature**

Read `src/Modules/Billing/Obss.Billing.Application/Commands/ApplyTaxExemption/ApplyTaxExemptionCommandHandler.cs`. Update the `TaxExemption.Create` call to pass `BillingAccountId`:

```csharp
        var exemption = TaxExemption.Create(
            tenantId,
            request.CustomerId,
            billingAccountId: null, // optional, can be set via separate endpoint later
            request.TaxRuleId,
            request.ExemptionCertificate,
            request.ExemptionRate,
            request.ValidFrom,
            request.ValidTo,
            request.ApprovedBy);
```

Also add `BillingAccountId` to `ApplyTaxExemptionCommand.cs` (nullable) and pass it through:

```csharp
public sealed record ApplyTaxExemptionCommand(
    Guid CustomerId,
    Guid? BillingAccountId,  // add this
    Guid TaxRuleId,
    string ExemptionCertificate,
    decimal ExemptionRate,
    DateTime ValidFrom,
    DateTime ValidTo,
    string ApprovedBy) : IRequest<Result<TaxExemptionDto>>;
```

Update handler to use `request.BillingAccountId` instead of `null`.

- [ ] **Step 4: Verify build**

Run: `dotnet build src/Modules/Billing/Obss.Billing.Domain/Obss.Billing.Domain.csproj`

Expected: Build succeeded with 0 warnings and 0 errors.

- [ ] **Step 4: Commit**

```bash
git add src/Modules/Billing/Obss.Billing.Domain/
git commit -m "feat: enhance BillingAccount with AccountHolder, BillPresentationMedia, PaymentMethodId, domain events"
```

---

### Task 3: Application — DTOs, Abstractions, and Mapster Config

**Files:**
- Create: `src/Modules/Billing/Obss.Billing.Application/DTOs/AccountBalanceDto.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/DTOs/BalanceTransactionDto.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/DTOs/BillPresentationMediaDto.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/DTOs/AccountHolderDto.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/DTOs/PatchBillingAccountRequest.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Abstractions/IAccountBalanceRepository.cs`
- Modify: `src/Modules/Billing/Obss.Billing.Application/DTOs/BillingAccountDto.cs` — add new fields
- Modify: `src/Modules/Billing/Obss.Billing.Application/Mappings/BillingMappingConfig.cs` — add new mappings

- [ ] **Step 1: Create AccountBalanceDto**

```csharp
namespace Obss.Billing.Application.DTOs;

public sealed record AccountBalanceDto(
    Guid Id,
    Guid BillingAccountId,
    decimal CurrentBalance,
    decimal OutstandingBalance,
    decimal AvailableCredit,
    string Currency,
    DateTime BalanceDate,
    DateTime LastUpdatedAt,
    string? AtType,
    string? AtBaseType,
    string? AtSchemaLocation,
    List<BalanceTransactionDto>? Transactions);
```

- [ ] **Step 2: Create BalanceTransactionDto**

```csharp
namespace Obss.Billing.Application.DTOs;

public sealed record BalanceTransactionDto(
    Guid Id,
    decimal Amount,
    string TransactionType,
    string Description,
    DateTime TransactionDate,
    string? ReferenceId,
    string? ReferenceType);
```

- [ ] **Step 3: Create BillPresentationMediaDto**

```csharp
namespace Obss.Billing.Application.DTOs;

public sealed record BillPresentationMediaDto(
    Guid Id,
    string MediaType,
    string? EmailAddress,
    string? PaperFormat,
    string Language,
    bool IsPreferred,
    DateTime? ValidFrom,
    DateTime? ValidUntil);
```

- [ ] **Step 4: Create AccountHolderDto**

```csharp
namespace Obss.Billing.Application.DTOs;

public sealed record AccountHolderDto(
    string Name,
    string? Email,
    string? Phone,
    Guid? ContactId);
```

- [ ] **Step 5: Create PatchBillingAccountRequest**

```csharp
namespace Obss.Billing.Application.DTOs;

public sealed record PatchBillingAccountRequest(
    string? Name,
    decimal? CreditLimit,
    string? Currency,
    string? Description,
    string? Status,
    string? PaymentMethodId,
    AccountHolderDto? AccountHolder);
```

- [ ] **Step 6: Create IAccountBalanceRepository**

```csharp
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Application.Abstractions;

public interface IAccountBalanceRepository
{
    Task<AccountBalance?> GetByBillingAccountIdAsync(Guid billingAccountId, CancellationToken cancellationToken = default);
    Task<AccountBalance?> GetByBillingAccountIdWithTransactionsAsync(Guid billingAccountId, CancellationToken cancellationToken = default);
    Task AddAsync(AccountBalance balance, CancellationToken cancellationToken = default);
    Task UpdateAsync(AccountBalance balance, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 7: Enhance BillingAccountDto**

Read existing file. Add to the record:
```csharp
    AccountHolderDto? AccountHolder,
    List<BillPresentationMediaDto>? BillPresentationMedia,
    string? PaymentMethodId);
```

Keep all existing fields; add these three at the end before the closing parenthesis.

- [ ] **Step 8: Update BillingMappingConfig**

Read existing file. Add after the existing `BillingAccount` mapping (before the closing `}` of `Configure()`):

```csharp
        TypeAdapterConfig<AccountBalance, AccountBalanceDto>.NewConfig()
            .Map(dest => dest.Transactions, src => src.Transactions.Adapt<List<BalanceTransactionDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<BalanceTransaction, BalanceTransactionDto>.NewConfig()
            .Map(dest => dest.TransactionType, src => src.TransactionType.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<BillPresentationMedia, BillPresentationMediaDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<AccountHolder, AccountHolderDto>.NewConfig();
```

Update the existing `BillingAccount` config to map new fields:
```csharp
        TypeAdapterConfig<BillingAccount, BillingAccountDto>.NewConfig()
            .Map(dest => dest.AccountType, src => src.AccountType.ToString())
            .Map(dest => dest.RelatedParties, src => src.RelatedParties.Adapt<List<RelatedPartyDto>>())
            .Map(dest => dest.AccountHolder, src => src.AccountHolder is not null ? src.AccountHolder.Adapt<AccountHolderDto>() : null)
            .Map(dest => dest.BillPresentationMedia, src => src.BillPresentationMedia.Adapt<List<BillPresentationMediaDto>>())
            .Map(dest => dest.Id, src => src.Id);
```

- [ ] **Step 9: Verify build**

Run: `dotnet build src/Modules/Billing/Obss.Billing.Application/Obss.Billing.Application.csproj`

Expected: Build succeeded with 0 warnings and 0 errors.

- [ ] **Step 10: Commit**

```bash
git add src/Modules/Billing/Obss.Billing.Application/
git commit -m "feat: add DTOs, IAccountBalanceRepository, and Mapster mappings for TMF666 resources"
```

---

### Task 4: Application — Patch and Delete BillingAccount Commands

**Files:**
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/PatchBillingAccount/PatchBillingAccountCommand.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/PatchBillingAccount/PatchBillingAccountCommandHandler.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/PatchBillingAccount/PatchBillingAccountCommandValidator.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/DeleteBillingAccount/DeleteBillingAccountCommand.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/DeleteBillingAccount/DeleteBillingAccountCommandHandler.cs`

- [ ] **Step 1: Create PatchBillingAccountCommand**

```csharp
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.PatchBillingAccount;

public sealed record PatchBillingAccountCommand(
    Guid Id,
    string? Name,
    decimal? CreditLimit,
    string? Currency,
    string? Description,
    string? Status,
    string? PaymentMethodId,
    AccountHolderDto? AccountHolder) : IRequest<Result<BillingAccountDto>>;
```

- [ ] **Step 2: Create PatchBillingAccountCommandHandler**

```csharp
using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.PatchBillingAccount;

public sealed class PatchBillingAccountCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<PatchBillingAccountCommand, Result<BillingAccountDto>>
{
    public async Task<Result<BillingAccountDto>> Handle(PatchBillingAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
            return Result.Failure<BillingAccountDto>(Error.NotFound(nameof(BillingAccount), request.Id));

        if (request.Name is not null || request.CreditLimit.HasValue || request.Currency is not null || request.Description is not null)
        {
            account.UpdateDetails(
                request.Name ?? account.Name,
                request.CreditLimit ?? account.CreditLimit,
                request.Currency ?? account.Currency,
                request.Description ?? account.Description);
        }

        if (request.Status is not null)
            account.SetStatus(request.Status);

        if (request.PaymentMethodId is not null)
            account.SetPaymentMethodId(request.PaymentMethodId);

        if (request.AccountHolder is not null)
        {
            var holder = new AccountHolder(
                request.AccountHolder.Name,
                request.AccountHolder.Email,
                request.AccountHolder.Phone,
                request.AccountHolder.ContactId);
            account.SetAccountHolder(holder);
        }

        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(account.Adapt<BillingAccountDto>());
    }
}
```

- [ ] **Step 3: Create PatchBillingAccountCommandValidator**

```csharp
using FluentValidation;

namespace Obss.Billing.Application.Commands.PatchBillingAccount;

public sealed class PatchBillingAccountCommandValidator : AbstractValidator<PatchBillingAccountCommand>
{
    public PatchBillingAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Billing account ID is required.");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.")
            .When(x => x.Name is not null);

        RuleFor(x => x.Currency)
            .MaximumLength(3).WithMessage("Currency must not exceed 3 characters.")
            .When(x => x.Currency is not null);

        RuleFor(x => x.CreditLimit)
            .GreaterThanOrEqualTo(0).WithMessage("Credit limit must be greater than or equal to 0.")
            .When(x => x.CreditLimit.HasValue);
    }
}
```

- [ ] **Step 4: Create DeleteBillingAccountCommand**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.DeleteBillingAccount;

public sealed record DeleteBillingAccountCommand(Guid Id) : IRequest<Result>;
```

- [ ] **Step 5: Create DeleteBillingAccountCommandHandler**

```csharp
using MediatR;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.DeleteBillingAccount;

public sealed class DeleteBillingAccountCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBillingAccountCommand, Result>
{
    public async Task<Result> Handle(DeleteBillingAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (account is null)
            return Result.Failure(Error.NotFound(nameof(BillingAccount), request.Id));

        account.MarkDeleted();
        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
```

- [ ] **Step 6: Verify build**

Run: `dotnet build src/Modules/Billing/Obss.Billing.Application/Obss.Billing.Application.csproj`

Expected: Build succeeded with 0 warnings and 0 errors.

- [ ] **Step 7: Commit**

```bash
git add src/Modules/Billing/Obss.Billing.Application/
git commit -m "feat: add PatchBillingAccount and DeleteBillingAccount commands"
```

---

### Task 5: Application — RelatedParty and BillPresentationMedia Commands

**Files:**
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/AddBillingAccountRelatedParty/AddBillingAccountRelatedPartyCommand.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/AddBillingAccountRelatedParty/AddBillingAccountRelatedPartyCommandHandler.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/RemoveBillingAccountRelatedParty/RemoveBillingAccountRelatedPartyCommand.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/RemoveBillingAccountRelatedParty/RemoveBillingAccountRelatedPartyCommandHandler.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/CreateBillPresentationMedia/CreateBillPresentationMediaCommand.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/CreateBillPresentationMedia/CreateBillPresentationMediaCommandHandler.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/CreateBillPresentationMedia/CreateBillPresentationMediaCommandValidator.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/UpdateBillPresentationMedia/UpdateBillPresentationMediaCommand.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/UpdateBillPresentationMedia/UpdateBillPresentationMediaCommandHandler.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/RemoveBillPresentationMedia/RemoveBillPresentationMediaCommand.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/RemoveBillPresentationMedia/RemoveBillPresentationMediaCommandHandler.cs`

- [ ] **Step 1: Create AddBillingAccountRelatedPartyCommand**

```csharp
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.AddBillingAccountRelatedParty;

public sealed record AddBillingAccountRelatedPartyCommand(
    Guid BillingAccountId,
    string PartyId,
    string PartyName,
    string Role) : IRequest<Result<BillingAccountDto>>;
```

- [ ] **Step 2: Create AddBillingAccountRelatedPartyCommandHandler**

```csharp
using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.AddBillingAccountRelatedParty;

public sealed class AddBillingAccountRelatedPartyCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddBillingAccountRelatedPartyCommand, Result<BillingAccountDto>>
{
    public async Task<Result<BillingAccountDto>> Handle(AddBillingAccountRelatedPartyCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure<BillingAccountDto>(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        account.AddRelatedParty(request.PartyId, request.PartyName, request.Role);
        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(account.Adapt<BillingAccountDto>());
    }
}
```

- [ ] **Step 3: Create RemoveBillingAccountRelatedPartyCommand**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.RemoveBillingAccountRelatedParty;

public sealed record RemoveBillingAccountRelatedPartyCommand(
    Guid BillingAccountId,
    string PartyId) : IRequest<Result>;
```

- [ ] **Step 4: Create RemoveBillingAccountRelatedPartyCommandHandler**

```csharp
using MediatR;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.RemoveBillingAccountRelatedParty;

public sealed class RemoveBillingAccountRelatedPartyCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveBillingAccountRelatedPartyCommand, Result>
{
    public async Task<Result> Handle(RemoveBillingAccountRelatedPartyCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        var party = account.RelatedParties.FirstOrDefault(rp => rp.PartyId == request.PartyId);
        if (party is null)
            return Result.Failure(Error.NotFound("RelatedParty", request.PartyId));

        account.RemoveRelatedParty(party);
        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
```

BillingAccount needs a `RemoveRelatedParty` method. Add it to the Domain entity:
```csharp
public void RemoveRelatedParty(RelatedParty party) => _relatedParties.Remove(party);
```

- [ ] **Step 5: Create CreateBillPresentationMediaCommand**

```csharp
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.CreateBillPresentationMedia;

public sealed record CreateBillPresentationMediaCommand(
    Guid BillingAccountId,
    string MediaType,
    string? EmailAddress,
    string? PaperFormat,
    string Language,
    bool IsPreferred) : IRequest<Result<BillPresentationMediaDto>>;
```

- [ ] **Step 6: Create CreateBillPresentationMediaCommandHandler**

```csharp
using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.CreateBillPresentationMedia;

public sealed class CreateBillPresentationMediaCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBillPresentationMediaCommand, Result<BillPresentationMediaDto>>
{
    public async Task<Result<BillPresentationMediaDto>> Handle(CreateBillPresentationMediaCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure<BillPresentationMediaDto>(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        var media = new BillPresentationMedia(request.MediaType, request.EmailAddress, request.PaperFormat, request.Language, request.IsPreferred);
        account.AddBillPresentationMedia(media);
        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(media.Adapt<BillPresentationMediaDto>());
    }
}
```

- [ ] **Step 7: Create CreateBillPresentationMediaCommandValidator**

```csharp
using FluentValidation;

namespace Obss.Billing.Application.Commands.CreateBillPresentationMedia;

public sealed class CreateBillPresentationMediaCommandValidator : AbstractValidator<CreateBillPresentationMediaCommand>
{
    public CreateBillPresentationMediaCommandValidator()
    {
        RuleFor(x => x.BillingAccountId)
            .NotEmpty().WithMessage("Billing account ID is required.");

        RuleFor(x => x.MediaType)
            .NotEmpty().WithMessage("Media type is required.")
            .Must(m => m is "Email" or "Paper" or "Portal").WithMessage("Media type must be Email, Paper, or Portal.");

        RuleFor(x => x.EmailAddress)
            .EmailAddress().When(x => x.EmailAddress is not null)
            .WithMessage("Invalid email address.");

        RuleFor(x => x.PaperFormat)
            .Must(f => f is "A4" or "Letter").When(x => x.PaperFormat is not null)
            .WithMessage("Paper format must be A4 or Letter.");

        RuleFor(x => x.Language)
            .NotEmpty().WithMessage("Language is required.")
            .MaximumLength(10).WithMessage("Language must not exceed 10 characters.");
    }
}
```

- [ ] **Step 8: Create UpdateBillPresentationMediaCommand**

```csharp
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.UpdateBillPresentationMedia;

public sealed record UpdateBillPresentationMediaCommand(
    Guid BillingAccountId,
    Guid MediaId,
    string? EmailAddress,
    string? PaperFormat,
    string? Language,
    bool? IsPreferred) : IRequest<Result<BillPresentationMediaDto>>;
```

- [ ] **Step 9: Create UpdateBillPresentationMediaCommandHandler**

```csharp
using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.UpdateBillPresentationMedia;

public sealed class UpdateBillPresentationMediaCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBillPresentationMediaCommand, Result<BillPresentationMediaDto>>
{
    public async Task<Result<BillPresentationMediaDto>> Handle(UpdateBillPresentationMediaCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure<BillPresentationMediaDto>(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        var media = account.BillPresentationMedia.FirstOrDefault(m => m.Id == request.MediaId);
        if (media is null)
            return Result.Failure<BillPresentationMediaDto>(Error.NotFound("BillPresentationMedia", request.MediaId));

        media.Update(request.EmailAddress, request.PaperFormat, request.Language, request.IsPreferred);
        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(media.Adapt<BillPresentationMediaDto>());
    }
}
```

- [ ] **Step 10: Create RemoveBillPresentationMediaCommand**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.RemoveBillPresentationMedia;

public sealed record RemoveBillPresentationMediaCommand(
    Guid BillingAccountId,
    Guid MediaId) : IRequest<Result>;
```

- [ ] **Step 11: Create RemoveBillPresentationMediaCommandHandler**

```csharp
using MediatR;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.RemoveBillPresentationMedia;

public sealed class RemoveBillPresentationMediaCommandHandler(
    IRepository<BillingAccount> repository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveBillPresentationMediaCommand, Result>
{
    public async Task<Result> Handle(RemoveBillPresentationMediaCommand request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        account.RemoveBillPresentationMedia(request.MediaId);
        await repository.UpdateAsync(account, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
```

- [ ] **Step 12: Verify build**

Run: `dotnet build src/Modules/Billing/Obss.Billing.Application/Obss.Billing.Application.csproj`

Expected: Build succeeded with 0 warnings and 0 errors.

- [ ] **Step 13: Commit**

```bash
git add src/Modules/Billing/Obss.Billing.Application/
git add src/Modules/Billing/Obss.Billing.Domain/Entities/BillingAccount.cs
git commit -m "feat: add RelatedParty and BillPresentationMedia CRUD commands"
```

---

### Task 6: Application — RecordBalanceTransaction Command and Queries

**Files:**
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/RecordBalanceTransaction/RecordBalanceTransactionCommand.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/RecordBalanceTransaction/RecordBalanceTransactionCommandHandler.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Commands/RecordBalanceTransaction/RecordBalanceTransactionCommandValidator.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Queries/GetBillingAccountBalance/GetBillingAccountBalanceQuery.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Queries/GetBillingAccountBalance/GetBillingAccountBalanceQueryHandler.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Queries/GetBillingAccountRelatedParties/GetBillingAccountRelatedPartiesQuery.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Queries/GetBillingAccountRelatedParties/GetBillingAccountRelatedPartiesQueryHandler.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Queries/GetBillingAccountBillPresentationMedia/GetBillingAccountBillPresentationMediaQuery.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/Queries/GetBillingAccountBillPresentationMedia/GetBillingAccountBillPresentationMediaQueryHandler.cs`

- [ ] **Step 1: Create RecordBalanceTransactionCommand**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.RecordBalanceTransaction;

public sealed record RecordBalanceTransactionCommand(
    Guid BillingAccountId,
    decimal Amount,
    string TransactionType,
    string Description,
    string? ReferenceId,
    string? ReferenceType) : IRequest<Result>;
```

- [ ] **Step 2: Create RecordBalanceTransactionCommandHandler**

```csharp
using Mapster;
using MediatR;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Domain.Events;
using Obss.Billing.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.RecordBalanceTransaction;

public sealed class RecordBalanceTransactionCommandHandler(
    IAccountBalanceRepository balanceRepository,
    IRepository<BillingAccount> accountRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RecordBalanceTransactionCommand, Result>
{
    public async Task<Result> Handle(RecordBalanceTransactionCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TransactionType>(request.TransactionType, true, out var transactionType))
            return Result.Failure(Error.Validation($"Invalid transaction type: '{request.TransactionType}'."));

        var account = await accountRepository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        var balance = await balanceRepository.GetByBillingAccountIdAsync(request.BillingAccountId, cancellationToken);
        if (balance is null)
        {
            balance = new AccountBalance(request.BillingAccountId, 0, 0, account.CreditLimit, account.Currency);
            await balanceRepository.AddAsync(balance, cancellationToken);
        }

        var previousBalance = balance.CurrentBalance;
        balance.RecordTransaction(request.Amount, transactionType, request.Description, request.ReferenceId, request.ReferenceType);
        balance.AddDomainEvent(new BalanceChangedEvent(request.BillingAccountId, previousBalance, balance.CurrentBalance, balance.Currency));

        await balanceRepository.UpdateAsync(balance, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
```

- [ ] **Step 3: Create RecordBalanceTransactionCommandValidator**

```csharp
using FluentValidation;

namespace Obss.Billing.Application.Commands.RecordBalanceTransaction;

public sealed class RecordBalanceTransactionCommandValidator : AbstractValidator<RecordBalanceTransactionCommand>
{
    public RecordBalanceTransactionCommandValidator()
    {
        RuleFor(x => x.BillingAccountId)
            .NotEmpty().WithMessage("Billing account ID is required.");

        RuleFor(x => x.TransactionType)
            .NotEmpty().WithMessage("Transaction type is required.")
            .Must(t => t is "Charge" or "Payment" or "Credit" or "Debit" or "Adjustment" or "Refund")
            .WithMessage("Invalid transaction type.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    }
}
```

- [ ] **Step 4: Create GetBillingAccountBalanceQuery**

```csharp
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountBalance;

public sealed record GetBillingAccountBalanceQuery(
    Guid BillingAccountId,
    DateTime? AsOfDate) : IRequest<Result<AccountBalanceDto>>;
```

- [ ] **Step 5: Create GetBillingAccountBalanceQueryHandler**

```csharp
using Mapster;
using MediatR;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountBalance;

public sealed class GetBillingAccountBalanceQueryHandler(
    IAccountBalanceRepository balanceRepository)
    : IRequestHandler<GetBillingAccountBalanceQuery, Result<AccountBalanceDto>>
{
    public async Task<Result<AccountBalanceDto>> Handle(GetBillingAccountBalanceQuery request, CancellationToken cancellationToken)
    {
        var balance = await balanceRepository.GetByBillingAccountIdWithTransactionsAsync(request.BillingAccountId, cancellationToken);
        if (balance is null)
            return Result.Failure<AccountBalanceDto>(Error.NotFound("AccountBalance", request.BillingAccountId));

        var dto = balance.Adapt<AccountBalanceDto>();
        return Result.Success(dto);
    }
}
```

- [ ] **Step 6: Create GetBillingAccountRelatedPartiesQuery**

```csharp
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountRelatedParties;

public sealed record GetBillingAccountRelatedPartiesQuery(
    Guid BillingAccountId) : IRequest<Result<List<RelatedPartyDto>>>;
```

- [ ] **Step 7: Create GetBillingAccountRelatedPartiesQueryHandler**

```csharp
using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountRelatedParties;

public sealed class GetBillingAccountRelatedPartiesQueryHandler(
    IRepository<BillingAccount> repository)
    : IRequestHandler<GetBillingAccountRelatedPartiesQuery, Result<List<RelatedPartyDto>>>
{
    public async Task<Result<List<RelatedPartyDto>>> Handle(GetBillingAccountRelatedPartiesQuery request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure<List<RelatedPartyDto>>(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        var dtos = account.RelatedParties.Adapt<List<RelatedPartyDto>>();
        return Result.Success(dtos);
    }
}
```

- [ ] **Step 8: Create GetBillingAccountBillPresentationMediaQuery**

```csharp
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountBillPresentationMedia;

public sealed record GetBillingAccountBillPresentationMediaQuery(
    Guid BillingAccountId) : IRequest<Result<List<BillPresentationMediaDto>>>;
```

- [ ] **Step 9: Create GetBillingAccountBillPresentationMediaQueryHandler**

```csharp
using Mapster;
using MediatR;
using Obss.Billing.Application.DTOs;
using Obss.Billing.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Queries.GetBillingAccountBillPresentationMedia;

public sealed class GetBillingAccountBillPresentationMediaQueryHandler(
    IRepository<BillingAccount> repository)
    : IRequestHandler<GetBillingAccountBillPresentationMediaQuery, Result<List<BillPresentationMediaDto>>>
{
    public async Task<Result<List<BillPresentationMediaDto>>> Handle(GetBillingAccountBillPresentationMediaQuery request, CancellationToken cancellationToken)
    {
        var account = await repository.GetByIdAsync(request.BillingAccountId, cancellationToken);
        if (account is null)
            return Result.Failure<List<BillPresentationMediaDto>>(Error.NotFound(nameof(BillingAccount), request.BillingAccountId));

        var dtos = account.BillPresentationMedia.Adapt<List<BillPresentationMediaDto>>();
        return Result.Success(dtos);
    }
}
```

- [ ] **Step 10: Verify build**

Run: `dotnet build src/Modules/Billing/Obss.Billing.Application/Obss.Billing.Application.csproj`

Expected: Build succeeded with 0 warnings and 0 errors.

- [ ] **Step 11: Commit**

```bash
git add src/Modules/Billing/Obss.Billing.Application/
git commit -m "feat: add RecordBalanceTransaction command and TMF666 resource queries"
```

---

### Task 7: Application — Integration Events and Domain Event Handlers

**Files:**
- Create: `src/Modules/Billing/Obss.Billing.Application/IntegrationEvents/BillingAccountCreatedIntegrationEvent.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/IntegrationEvents/BillingAccountUpdatedIntegrationEvent.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/IntegrationEvents/BillingAccountDeletedIntegrationEvent.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/IntegrationEvents/BillingAccountBalanceChangedIntegrationEvent.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/DomainEventHandlers/BillingAccountCreatedEventHandler.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/DomainEventHandlers/BillingAccountUpdatedEventHandler.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/DomainEventHandlers/BillingAccountDeletedEventHandler.cs`
- Create: `src/Modules/Billing/Obss.Billing.Application/DomainEventHandlers/BalanceChangedEventHandler.cs`

- [ ] **Step 1: Create BillingAccountCreatedIntegrationEvent**

```csharp
using Obss.SharedKernel.Domain.Events;

namespace Obss.Billing.Application.IntegrationEvents;

public sealed class BillingAccountCreatedIntegrationEvent : IntegrationEvent
{
    public BillingAccountCreatedIntegrationEvent(Guid billingAccountId, Guid customerId, string accountType)
    {
        BillingAccountId = billingAccountId;
        CustomerId = customerId;
        AccountType = accountType;
    }

    public Guid BillingAccountId { get; }
    public Guid CustomerId { get; }
    public string AccountType { get; }
}
```

- [ ] **Step 2: Create BillingAccountUpdatedIntegrationEvent**

```csharp
using Obss.SharedKernel.Domain.Events;

namespace Obss.Billing.Application.IntegrationEvents;

public sealed class BillingAccountUpdatedIntegrationEvent : IntegrationEvent
{
    public BillingAccountUpdatedIntegrationEvent(Guid billingAccountId, string name, string status)
    {
        BillingAccountId = billingAccountId;
        Name = name;
        Status = status;
    }

    public Guid BillingAccountId { get; }
    public string Name { get; }
    public string Status { get; }
}
```

- [ ] **Step 3: Create BillingAccountDeletedIntegrationEvent**

```csharp
using Obss.SharedKernel.Domain.Events;

namespace Obss.Billing.Application.IntegrationEvents;

public sealed class BillingAccountDeletedIntegrationEvent : IntegrationEvent
{
    public BillingAccountDeletedIntegrationEvent(Guid billingAccountId)
    {
        BillingAccountId = billingAccountId;
    }

    public Guid BillingAccountId { get; }
}
```

- [ ] **Step 4: Create BillingAccountBalanceChangedIntegrationEvent**

```csharp
using Obss.SharedKernel.Domain.Events;

namespace Obss.Billing.Application.IntegrationEvents;

public sealed class BillingAccountBalanceChangedIntegrationEvent : IntegrationEvent
{
    public BillingAccountBalanceChangedIntegrationEvent(Guid billingAccountId, decimal previousBalance, decimal newBalance, string currency)
    {
        BillingAccountId = billingAccountId;
        PreviousBalance = previousBalance;
        NewBalance = newBalance;
        Currency = currency;
    }

    public Guid BillingAccountId { get; }
    public decimal PreviousBalance { get; }
    public decimal NewBalance { get; }
    public string Currency { get; }
}
```

- [ ] **Step 5: Create BillingAccountCreatedEventHandler**

Follow the pattern from `BillFinalizedEventHandler`:

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.IntegrationEvents;
using Obss.Billing.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Application.DomainEventHandlers;

public sealed class BillingAccountCreatedEventHandler : INotificationHandler<BillingAccountCreatedEvent>
{
    private readonly IMediator _mediator;
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<BillingAccountCreatedEventHandler> _logger;

    public BillingAccountCreatedEventHandler(
        IMediator mediator,
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        ILogger<BillingAccountCreatedEventHandler> logger)
    {
        _mediator = mediator;
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(BillingAccountCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Billing account {BillingAccountId} created for customer {CustomerId}. Type: {AccountType}.",
            notification.BillingAccountId, notification.CustomerId, notification.AccountType);

        var integrationEvent = new BillingAccountCreatedIntegrationEvent(
            notification.BillingAccountId,
            notification.CustomerId,
            notification.AccountType)
        {
            TenantId = _currentTenant.TenantId ?? string.Empty,
            CorrelationId = notification.EventId.ToString()
        };

        await _outboxService.AddAsync(integrationEvent, cancellationToken);
        await _mediator.Publish(integrationEvent, cancellationToken);
    }
}
```

- [ ] **Step 6: Create BillingAccountUpdatedEventHandler**

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.IntegrationEvents;
using Obss.Billing.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Application.DomainEventHandlers;

public sealed class BillingAccountUpdatedEventHandler : INotificationHandler<BillingAccountUpdatedEvent>
{
    private readonly IMediator _mediator;
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<BillingAccountUpdatedEventHandler> _logger;

    public BillingAccountUpdatedEventHandler(
        IMediator mediator,
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        ILogger<BillingAccountUpdatedEventHandler> logger)
    {
        _mediator = mediator;
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(BillingAccountUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Billing account {BillingAccountId} updated. Name: {Name}, Status: {Status}.",
            notification.BillingAccountId, notification.Name, notification.Status);

        var integrationEvent = new BillingAccountUpdatedIntegrationEvent(
            notification.BillingAccountId,
            notification.Name,
            notification.Status)
        {
            TenantId = _currentTenant.TenantId ?? string.Empty,
            CorrelationId = notification.EventId.ToString()
        };

        await _outboxService.AddAsync(integrationEvent, cancellationToken);
        await _mediator.Publish(integrationEvent, cancellationToken);
    }
}
```

- [ ] **Step 7: Create BillingAccountDeletedEventHandler**

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.IntegrationEvents;
using Obss.Billing.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Application.DomainEventHandlers;

public sealed class BillingAccountDeletedEventHandler : INotificationHandler<BillingAccountDeletedEvent>
{
    private readonly IMediator _mediator;
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<BillingAccountDeletedEventHandler> _logger;

    public BillingAccountDeletedEventHandler(
        IMediator mediator,
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        ILogger<BillingAccountDeletedEventHandler> logger)
    {
        _mediator = mediator;
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(BillingAccountDeletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Billing account {BillingAccountId} deleted.",
            notification.BillingAccountId);

        var integrationEvent = new BillingAccountDeletedIntegrationEvent(notification.BillingAccountId)
        {
            TenantId = _currentTenant.TenantId ?? string.Empty,
            CorrelationId = notification.EventId.ToString()
        };

        await _outboxService.AddAsync(integrationEvent, cancellationToken);
        await _mediator.Publish(integrationEvent, cancellationToken);
    }
}
```

- [ ] **Step 8: Create BalanceChangedEventHandler**

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Billing.Application.IntegrationEvents;
using Obss.Billing.Domain.Events;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Application.DomainEventHandlers;

public sealed class BalanceChangedEventHandler : INotificationHandler<BalanceChangedEvent>
{
    private readonly IMediator _mediator;
    private readonly IOutboxService _outboxService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<BalanceChangedEventHandler> _logger;

    public BalanceChangedEventHandler(
        IMediator mediator,
        IOutboxService outboxService,
        ICurrentTenant currentTenant,
        ILogger<BalanceChangedEventHandler> logger)
    {
        _mediator = mediator;
        _outboxService = outboxService;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task Handle(BalanceChangedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Balance changed for billing account {BillingAccountId}: {PreviousBalance} -> {NewBalance} {Currency}.",
            notification.BillingAccountId, notification.PreviousBalance, notification.NewBalance, notification.Currency);

        var integrationEvent = new BillingAccountBalanceChangedIntegrationEvent(
            notification.BillingAccountId,
            notification.PreviousBalance,
            notification.NewBalance,
            notification.Currency)
        {
            TenantId = _currentTenant.TenantId ?? string.Empty,
            CorrelationId = notification.EventId.ToString()
        };

        await _outboxService.AddAsync(integrationEvent, cancellationToken);
        await _mediator.Publish(integrationEvent, cancellationToken);
    }
}
```

- [ ] **Step 9: Verify build**

Run: `dotnet build src/Modules/Billing/Obss.Billing.Application/Obss.Billing.Application.csproj`

Expected: Build succeeded with 0 warnings and 0 errors.

- [ ] **Step 10: Commit**

```bash
git add src/Modules/Billing/Obss.Billing.Application/
git commit -m "feat: add integration events and domain event handlers for TMF666 billing account lifecycle"
```

---

### Task 8: Infrastructure — New EF Configurations

**Files:**
- Create: `src/Modules/Billing/Obss.Billing.Infrastructure/Persistence/Configurations/AccountBalanceConfiguration.cs`
- Create: `src/Modules/Billing/Obss.Billing.Infrastructure/Persistence/Configurations/BalanceTransactionConfiguration.cs`
- Create: `src/Modules/Billing/Obss.Billing.Infrastructure/Persistence/Configurations/BillPresentationMediaConfiguration.cs`
- Modify: `src/Modules/Billing/Obss.Billing.Infrastructure/Persistence/BillingDbContext.cs` — add DbSet for AccountBalance

- [ ] **Step 1: Create AccountBalanceConfiguration**

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Obss.Billing.Domain.Entities;

namespace Obss.Billing.Infrastructure.Persistence.Configurations;

public sealed class AccountBalanceConfiguration : IEntityTypeConfiguration<AccountBalance>
{
    public void Configure(EntityTypeBuilder<AccountBalance> builder)
    {
        builder.ToTable("account_balances");

        builder.HasKey(ab => ab.Id);

        builder.Property(ab => ab.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ab => ab.BillingAccountId)
            .HasColumnName("billing_account_id")
            .IsRequired();

        builder.Property(ab => ab.CurrentBalance)
            .HasColumnName("current_balance")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(ab => ab.OutstandingBalance)
            .HasColumnName("outstanding_balance")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(ab => ab.AvailableCredit)
            .HasColumnName("available_credit")
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(ab => ab.Currency)
            .HasColumnName("currency")
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(ab => ab.BalanceDate)
            .HasColumnName("balance_date")
            .IsRequired();

        builder.Property(ab => ab.LastUpdatedAt)
            .HasColumnName("last_updated_at")
            .IsRequired();

        builder.Property(ab => ab.AtType)
            .HasColumnName("at_type")
            .HasMaxLength(100);

        builder.Property(ab => ab.AtBaseType)
            .HasColumnName("at_base_type")
            .HasMaxLength(100);

        builder.Property(ab => ab.AtSchemaLocation)
            .HasColumnName("at_schema_location")
            .HasMaxLength(500);

        builder.OwnsMany(ab => ab.Transactions, t =>
        {
            t.ToTable("balance_transactions");
            t.WithOwner().HasForeignKey("balance_id");
            t.HasKey("Id");

            t.Property(t => t.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            t.Property(t => t.Amount)
                .HasColumnName("amount")
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            t.Property(t => t.TransactionType)
                .HasColumnName("transaction_type")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            t.Property(t => t.Description)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired();

            t.Property(t => t.TransactionDate)
                .HasColumnName("transaction_date")
                .IsRequired();

            t.Property(t => t.ReferenceId)
                .HasColumnName("reference_id")
                .HasMaxLength(100);

            t.Property(t => t.ReferenceType)
                .HasColumnName("reference_type")
                .HasMaxLength(50);
        });

        builder.Navigation(ab => ab.Transactions)
            .AutoInclude();

        builder.HasIndex(ab => ab.BillingAccountId)
            .HasDatabaseName("ix_account_balances_billing_account_id");

        builder.HasIndex(ab => ab.BalanceDate)
            .HasDatabaseName("ix_account_balances_balance_date");
    }
}
```

- [ ] **Step 2: Create BalanceTransactionConfiguration**

This is optional since the owned type config is already in AccountBalanceConfiguration. Skip this file.

- [ ] **Step 3: Create BillPresentationMediaConfiguration**

This config is embedded within the BillingAccountConfiguration (Task 9). Skip standalone file.

- [ ] **Step 4: Update BillingDbContext**

Add to existing class:
```csharp
    public DbSet<AccountBalance> AccountBalances => Set<AccountBalance>();
```

- [ ] **Step 5: Verify build**

Run: `dotnet build src/Modules/Billing/Obss.Billing.Infrastructure/Obss.Billing.Infrastructure.csproj`

Expected: Build succeeded with 0 warnings and 0 errors.

- [ ] **Step 6: Commit**

```bash
git add src/Modules/Billing/Obss.Billing.Infrastructure/
git commit -m "feat: add AccountBalance EF configuration and DbContext update"
```

---

### Task 9: Infrastructure — Modify Existing EF Configs and Add Repository

**Files:**
- Modify: `src/Modules/Billing/Obss.Billing.Infrastructure/Persistence/Configurations/BillingAccountConfiguration.cs` — add AccountHolder, BillPresentationMedia, PaymentMethodId
- Modify: `src/Modules/Billing/Obss.Billing.Infrastructure/Persistence/Configurations/TaxExemptionConfiguration.cs` — add BillingAccountId
- Create: `src/Modules/Billing/Obss.Billing.Infrastructure/Persistence/Repositories/AccountBalanceRepository.cs`
- Modify: `src/Modules/Billing/Obss.Billing.Api/Extensions/BillingModuleRegistration.cs` — register AccountBalanceRepository

- [ ] **Step 1: Enhance BillingAccountConfiguration**

Read existing file. Add before `builder.OwnsMany(ba => ba.RelatedParties...`:

```csharp
        builder.Property(ba => ba.PaymentMethodId)
            .HasColumnName("payment_method_id")
            .HasMaxLength(100);

        builder.OwnsOne(ba => ba.AccountHolder, ah =>
        {
            ah.Property(a => a.Name).HasColumnName("account_holder_name").HasMaxLength(200);
            ah.Property(a => a.Email).HasColumnName("account_holder_email").HasMaxLength(200);
            ah.Property(a => a.Phone).HasColumnName("account_holder_phone").HasMaxLength(50);
            ah.Property(a => a.ContactId).HasColumnName("account_holder_contact_id");
        });

        builder.OwnsMany(ba => ba.BillPresentationMedia, pm =>
        {
            pm.ToTable("billing_account_presentation_media");
            pm.WithOwner().HasForeignKey("billing_account_id");
            pm.HasKey("Id");

            pm.Property(p => p.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            pm.Property(p => p.MediaType)
                .HasColumnName("media_type")
                .HasMaxLength(20)
                .IsRequired();

            pm.Property(p => p.EmailAddress)
                .HasColumnName("email_address")
                .HasMaxLength(200);

            pm.Property(p => p.PaperFormat)
                .HasColumnName("paper_format")
                .HasMaxLength(10);

            pm.Property(p => p.Language)
                .HasColumnName("language")
                .HasMaxLength(10)
                .IsRequired();

            pm.Property(p => p.IsPreferred)
                .HasColumnName("is_preferred")
                .IsRequired();

            pm.Property(p => p.ValidFrom)
                .HasColumnName("valid_from");

            pm.Property(p => p.ValidUntil)
                .HasColumnName("valid_until");
        });

        builder.Navigation(ba => ba.BillPresentationMedia)
            .AutoInclude();
```

- [ ] **Step 2: Enhance TaxExemptionConfiguration**

Read existing file. Add after the `CustomerId` property mapping:

```csharp
        builder.Property(te => te.BillingAccountId)
            .HasColumnName("billing_account_id");
```

- [ ] **Step 3: Create AccountBalanceRepository**

```csharp
using Microsoft.EntityFrameworkCore;
using Obss.Billing.Application.Abstractions;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Infrastructure.Persistence;
using Obss.SharedKernel.Infrastructure.Persistence;

namespace Obss.Billing.Infrastructure.Persistence.Repositories;

public sealed class AccountBalanceRepository : IAccountBalanceRepository
{
    private readonly BillingDbContext _dbContext;

    public AccountBalanceRepository(BillingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AccountBalance?> GetByBillingAccountIdAsync(Guid billingAccountId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AccountBalances
            .FirstOrDefaultAsync(ab => ab.BillingAccountId == billingAccountId, cancellationToken);
    }

    public async Task<AccountBalance?> GetByBillingAccountIdWithTransactionsAsync(Guid billingAccountId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.AccountBalances
            .Include(ab => ab.Transactions)
            .FirstOrDefaultAsync(ab => ab.BillingAccountId == billingAccountId, cancellationToken);
    }

    public async Task AddAsync(AccountBalance balance, CancellationToken cancellationToken = default)
    {
        await _dbContext.AccountBalances.AddAsync(balance, cancellationToken);
    }

    public Task UpdateAsync(AccountBalance balance, CancellationToken cancellationToken = default)
    {
        _dbContext.AccountBalances.Update(balance);
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 4: Register AccountBalanceRepository in ModuleRegistration**

Read `BillingModuleRegistration.cs`. Add after the existing repository registrations:

```csharp
        services.AddScoped<IAccountBalanceRepository, AccountBalanceRepository>();
```

Add `using` at top:
```csharp
using Obss.Billing.Application.Abstractions;
```

- [ ] **Step 5: Verify build**

Run: `dotnet build src/Modules/Billing/Obss.Billing.Infrastructure/Obss.Billing.Infrastructure.csproj`

Then: `dotnet build src/Modules/Billing/Obss.Billing.Api/Obss.Billing.Api.csproj`

Expected: Both succeed with 0 warnings and 0 errors.

- [ ] **Step 6: Commit**

```bash
git add src/Modules/Billing/
git commit -m "feat: enhance EF configs for TMF666 fields, add AccountBalanceRepository"
```

---

### Task 10: Infrastructure — EF Migration

**Files:**
- Create: `src/Modules/Billing/Obss.Billing.Infrastructure/Migrations/20260710xxxxx_AddBillingAccountTmf666Resources.cs`
- Create: `src/Modules/Billing/Obss.Billing.Infrastructure/Migrations/20260710xxxxx_AddBillingAccountTmf666Resources.Designer.cs`

- [ ] **Step 1: Generate migration**

Run from repo root:
```bash
dotnet ef migrations add AddBillingAccountTmf666Resources \
  -p src/Modules/Billing/Obss.Billing.Infrastructure \
  -s src/Host/Obss.Host \
  -c BillingDbContext \
  --connection "Host=localhost;Database=obss_billing;Username=obss;Password=obss"
```

Expected: Migration files created in `src/Modules/Billing/Obss.Billing.Infrastructure/Persistence/Migrations/`.

- [ ] **Step 2: Verify migration builds**

Run: `dotnet build src/Modules/Billing/Obss.Billing.Infrastructure/Obss.Billing.Infrastructure.csproj`

Expected: Build succeeded with 0 warnings and 0 errors.

- [ ] **Step 3: Review migration code**

Open the generated migration and verify it:
- Creates `account_balances` table with all columns
- Creates `balance_transactions` table with FK to account_balances
- Creates `billing_account_presentation_media` table with FK to billing_accounts
- Adds `payment_method_id`, `account_holder_name`, `account_holder_email`, `account_holder_phone`, `account_holder_contact_id` columns to `billing_accounts`
- Adds `billing_account_id` column to `tax_exemptions` table

- [ ] **Step 4: Commit**

```bash
git add src/Modules/Billing/Obss.Billing.Infrastructure/Persistence/Migrations/
git commit -m "feat: add EF migration for TMF666 billing account resources"
```

---

### Task 11: API — Endpoints and Module Registration

**Files:**
- Modify: `src/Modules/Billing/Obss.Billing.Api/Endpoints/BillingEndpoints.cs` — add 11 new endpoint mappings
- Modify: `src/Modules/Billing/Obss.Billing.Api/Extensions/BillingModuleRegistration.cs` — already done in Task 9

- [ ] **Step 1: Add new endpoint mappings**

Read existing `BillingEndpoints.cs`. Add to the `Map` method, after the existing billing account endpoints (before the closing `}` of `Map`):

```csharp
        // TMF666 BillingAccount enhancements
        group.MapPatch("/billing-accounts/{id:guid}", async (Guid id, PatchBillingAccountCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/billing-accounts/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteBillingAccountCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/billing-accounts/{id:guid}/balance", async (Guid id, DateTime? asOfDate, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBillingAccountBalanceQuery(id, asOfDate));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/billing-accounts/{id:guid}/adjustments", async (Guid id, RecordBalanceTransactionCommand command, IMediator mediator) =>
        {
            if (id != command.BillingAccountId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/billing-accounts/{id:guid}/related-parties", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBillingAccountRelatedPartiesQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/billing-accounts/{id:guid}/related-parties", async (Guid id, AddBillingAccountRelatedPartyCommand command, IMediator mediator) =>
        {
            if (id != command.BillingAccountId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/billing/billing-accounts/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/billing-accounts/{id:guid}/related-parties/{partyId}", async (Guid id, string partyId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveBillingAccountRelatedPartyCommand(id, partyId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/billing-accounts/{id:guid}/presentation-media", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBillingAccountBillPresentationMediaQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/billing-accounts/{id:guid}/presentation-media", async (Guid id, CreateBillPresentationMediaCommand command, IMediator mediator) =>
        {
            if (id != command.BillingAccountId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/billing/billing-accounts/{id}/presentation-media/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPut("/billing-accounts/{id:guid}/presentation-media/{mediaId:guid}", async (Guid id, Guid mediaId, UpdateBillPresentationMediaCommand command, IMediator mediator) =>
        {
            if (id != command.BillingAccountId || mediaId != command.MediaId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapDelete("/billing-accounts/{id:guid}/presentation-media/{mediaId:guid}", async (Guid id, Guid mediaId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveBillPresentationMediaCommand(id, mediaId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound(result.Error);
        });
```

Add new imports at top:
```csharp
using Obss.Billing.Application.Commands.PatchBillingAccount;
using Obss.Billing.Application.Commands.DeleteBillingAccount;
using Obss.Billing.Application.Commands.AddBillingAccountRelatedParty;
using Obss.Billing.Application.Commands.RemoveBillingAccountRelatedParty;
using Obss.Billing.Application.Commands.CreateBillPresentationMedia;
using Obss.Billing.Application.Commands.UpdateBillPresentationMedia;
using Obss.Billing.Application.Commands.RemoveBillPresentationMedia;
using Obss.Billing.Application.Commands.RecordBalanceTransaction;
using Obss.Billing.Application.Queries.GetBillingAccountBalance;
using Obss.Billing.Application.Queries.GetBillingAccountRelatedParties;
using Obss.Billing.Application.Queries.GetBillingAccountBillPresentationMedia;
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/Host/Obss.Host/Obss.Host.csproj`

Expected: Build succeeded with 0 warnings and 0 errors.

- [ ] **Step 3: Commit**

```bash
git add src/Modules/Billing/Obss.Billing.Api/
git commit -m "feat: add TMF666 billing account API endpoints (PATCH, DELETE, balance, related parties, presentation media)"
```

---

### Task 12: Frontend — Query Keys and Hooks

**Files:**
- Modify: `frontend/src/lib/query-keys.ts` — add billingAccounts factory to existing billing section
- Create: `frontend/src/api/hooks/useBillingAccounts.ts` — all billing account hooks

- [ ] **Step 1: Add billingAccounts query keys**

Read `query-keys.ts`. Add to the existing `billing` section, after the `taxRules` block and before the closing `}` of `billing`:

```typescript
    billingAccounts: {
      all: ["billing", "billing-accounts"] as const,
      lists: () => [...queryKeys.billing.billingAccounts.all, "list"] as const,
      list: (filters: Record<string, string> = {}) =>
        [...queryKeys.billing.billingAccounts.lists(), filters] as const,
      details: () => [...queryKeys.billing.billingAccounts.all, "detail"] as const,
      detail: (id: string) => [...queryKeys.billing.billingAccounts.details(), id] as const,
      balance: (id: string) => [...queryKeys.billing.billingAccounts.detail(id), "balance"] as const,
      relatedParties: (id: string) => [...queryKeys.billing.billingAccounts.detail(id), "related-parties"] as const,
      presentationMedia: (id: string) => [...queryKeys.billing.billingAccounts.detail(id), "presentation-media"] as const,
    },
```

- [ ] **Step 2: Create useBillingAccounts.ts**

Following the pattern from `useServiceQualification.ts`:

```typescript
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export interface AccountHolderDto {
  name: string
  email: string | null
  phone: string | null
  contactId: string | null
}

export interface RelatedPartyDto {
  partyId: string
  partyName: string
  role: string
}

export interface BillPresentationMediaDto {
  id: string
  mediaType: string
  emailAddress: string | null
  paperFormat: string | null
  language: string
  isPreferred: boolean
  validFrom: string | null
  validUntil: string | null
}

export interface BillingAccountDto {
  id: string
  customerId: string
  accountType: string
  name: string
  status: string
  creditLimit: number
  currency: string
  validFrom: string | null
  validUntil: string | null
  description: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string
  href: string | null
  atType: string | null
  atBaseType: string | null
  atSchemaLocation: string | null
  externalId: string | null
  relatedParties: RelatedPartyDto[] | null
  accountHolder: AccountHolderDto | null
  billPresentationMedia: BillPresentationMediaDto[] | null
  paymentMethodId: string | null
}

export interface BalanceTransactionDto {
  id: string
  amount: number
  transactionType: string
  description: string
  transactionDate: string
  referenceId: string | null
  referenceType: string | null
}

export interface AccountBalanceDto {
  id: string
  billingAccountId: string
  currentBalance: number
  outstandingBalance: number
  availableCredit: number
  currency: string
  balanceDate: string
  lastUpdatedAt: string
  atType: string | null
  atBaseType: string | null
  atSchemaLocation: string | null
  transactions: BalanceTransactionDto[] | null
}

export function useBillingAccounts(filters: Record<string, string> = {}) {
  return useQuery<BillingAccountDto[]>({
    queryKey: queryKeys.billing.billingAccounts.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams(filters)
      const res = await api.get(`/api/v1/billing/billing-accounts?${params.toString()}`)
      return res.json()
    },
  })
}

export function useBillingAccount(id: string) {
  return useQuery<BillingAccountDto>({
    queryKey: queryKeys.billing.billingAccounts.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/billing/billing-accounts/${id}`)
      return res.json()
    },
    enabled: !!id,
  })
}

export function useBillingAccountBalance(id: string) {
  return useQuery<AccountBalanceDto>({
    queryKey: queryKeys.billing.billingAccounts.balance(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/billing/billing-accounts/${id}/balance`)
      return res.json()
    },
    enabled: !!id,
  })
}

export function useCreateBillingAccount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      customerId: string
      accountType: string
      name: string
      creditLimit: number
      currency: string
    }) => {
      const res = await api.post("/api/v1/billing/billing-accounts", data)
      return res.json()
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.billingAccounts.all })
    },
  })
}

export function useUpdateBillingAccount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string
      name: string
      creditLimit: number
      currency: string
      description: string | null
    }) => {
      const res = await api.put(`/api/v1/billing/billing-accounts/${id}`, data)
      return res.json()
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.billingAccounts.detail(variables.id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.billingAccounts.lists() })
    },
  })
}

export function useDeleteBillingAccount() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/api/v1/billing/billing-accounts/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.billingAccounts.all })
    },
  })
}

export function useAddBillingAccountRelatedParty() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ billingAccountId, ...data }: {
      billingAccountId: string
      partyId: string
      partyName: string
      role: string
    }) => {
      const res = await api.post(
        `/api/v1/billing/billing-accounts/${billingAccountId}/related-parties`,
        data,
      )
      return res.json()
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.billing.billingAccounts.detail(variables.billingAccountId),
      })
    },
  })
}

export function useRemoveBillingAccountRelatedParty() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ billingAccountId, partyId }: { billingAccountId: string; partyId: string }) => {
      await api.delete(`/api/v1/billing/billing-accounts/${billingAccountId}/related-parties/${partyId}`)
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.billing.billingAccounts.detail(variables.billingAccountId),
      })
    },
  })
}

export function useCreateBillPresentationMedia() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ billingAccountId, ...data }: {
      billingAccountId: string
      mediaType: string
      emailAddress: string | null
      paperFormat: string | null
      language: string
      isPreferred: boolean
    }) => {
      const res = await api.post(
        `/api/v1/billing/billing-accounts/${billingAccountId}/presentation-media`,
        data,
      )
      return res.json()
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.billing.billingAccounts.detail(variables.billingAccountId),
      })
    },
  })
}

export function useRemoveBillPresentationMedia() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ billingAccountId, mediaId }: { billingAccountId: string; mediaId: string }) => {
      await api.delete(
        `/api/v1/billing/billing-accounts/${billingAccountId}/presentation-media/${mediaId}`,
      )
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({
        queryKey: queryKeys.billing.billingAccounts.detail(variables.billingAccountId),
      })
    },
  })
}
```

- [ ] **Step 3: Verify frontend build**

Run: `cd frontend && bun run lint`

Expected: No lint errors (pre-existing errors should not be introduced).

- [ ] **Step 4: Commit**

```bash
git add frontend/
git commit -m "feat: add billing account query keys and frontend hooks"
```

---

### Task 13: Frontend — Pages

**Files:**
- Create: `frontend/src/app/billing/accounts/page.tsx` — billing account list page
- Create: `frontend/src/app/billing/accounts/new/page.tsx` — create billing account form
- Create: `frontend/src/app/billing/accounts/[id]/page.tsx` — billing account detail page
- Create: `frontend/src/app/billing/accounts/[id]/edit/page.tsx` — edit billing account form

- [ ] **Step 1: Create billing account list page**

```tsx
"use client"

import Link from "next/link"
import { useBillingAccounts, useDeleteBillingAccount } from "@/api/hooks/useBillingAccounts"

export default function BillingAccountsPage() {
  const { data: accounts, isLoading } = useBillingAccounts()
  const deleteAccount = useDeleteBillingAccount()

  if (isLoading) return <div className="p-6 text-gray-500">Loading billing accounts...</div>

  return (
    <div className="p-6">
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold">Billing Accounts</h1>
        <Link
          href="/billing/accounts/new"
          className="rounded bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
        >
          New Account
        </Link>
      </div>

      <div className="overflow-x-auto rounded border">
        <table className="min-w-full text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Name</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Type</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Status</th>
              <th className="px-4 py-3 text-right font-medium text-gray-600">Credit Limit</th>
              <th className="px-4 py-3 text-left font-medium text-gray-600">Currency</th>
              <th className="px-4 py-3 text-center font-medium text-gray-600">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y">
            {accounts?.map((a) => (
              <tr key={a.id} className="hover:bg-gray-50">
                <td className="px-4 py-3">
                  <Link href={`/billing/accounts/${a.id}`} className="text-blue-600 hover:underline font-medium">
                    {a.name}
                  </Link>
                </td>
                <td className="px-4 py-3 capitalize">{a.accountType.toLowerCase()}</td>
                <td className="px-4 py-3">
                  <span className={`inline-flex rounded-full px-2 py-0.5 text-xs font-medium ${
                    a.status === "Active" ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-600"
                  }`}>
                    {a.status}
                  </span>
                </td>
                <td className="px-4 py-3 text-right">{a.creditLimit.toLocaleString()}</td>
                <td className="px-4 py-3">{a.currency}</td>
                <td className="px-4 py-3 text-center">
                  <Link
                    href={`/billing/accounts/${a.id}/edit`}
                    className="text-blue-600 hover:underline text-xs mr-3"
                  >
                    Edit
                  </Link>
                  <button
                    onClick={() => { if (confirm("Delete this account?")) deleteAccount.mutate(a.id) }}
                    className="text-red-600 hover:underline text-xs"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
            {(!accounts || accounts.length === 0) && (
              <tr>
                <td colSpan={6} className="px-4 py-8 text-center text-gray-400">
                  No billing accounts found. Create one to get started.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
```

- [ ] **Step 2: Create billing account create page**

```tsx
"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import { useCreateBillingAccount } from "@/api/hooks/useBillingAccounts"

export default function NewBillingAccountPage() {
  const router = useRouter()
  const createAccount = useCreateBillingAccount()
  const [form, setForm] = useState({
    customerId: "",
    accountType: "Standard",
    name: "",
    creditLimit: 0,
    currency: "YER",
  })

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    const result = await createAccount.mutateAsync(form)
    router.push(`/billing/accounts/${result.id}`)
  }

  return (
    <div className="max-w-lg mx-auto p-6">
      <h1 className="text-2xl font-semibold mb-6">New Billing Account</h1>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Customer ID</label>
          <input
            type="text"
            value={form.customerId}
            onChange={(e) => setForm({ ...form, customerId: e.target.value })}
            className="w-full rounded border px-3 py-2 text-sm"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Account Type</label>
          <select
            value={form.accountType}
            onChange={(e) => setForm({ ...form, accountType: e.target.value })}
            className="w-full rounded border px-3 py-2 text-sm"
          >
            <option value="Standard">Standard</option>
            <option value="Prepaid">Prepaid</option>
            <option value="Postpaid">Postpaid</option>
            <option value="Corporate">Corporate</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
          <input
            type="text"
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            className="w-full rounded border px-3 py-2 text-sm"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Credit Limit</label>
          <input
            type="number"
            value={form.creditLimit}
            onChange={(e) => setForm({ ...form, creditLimit: Number(e.target.value) })}
            className="w-full rounded border px-3 py-2 text-sm"
            min="0"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Currency</label>
          <select
            value={form.currency}
            onChange={(e) => setForm({ ...form, currency: e.target.value })}
            className="w-full rounded border px-3 py-2 text-sm"
          >
            <option value="YER">YER</option>
            <option value="USD">USD</option>
            <option value="SAR">SAR</option>
          </select>
        </div>
        <div className="flex gap-3 pt-2">
          <button
            type="submit"
            disabled={createAccount.isPending}
            className="rounded bg-blue-600 px-6 py-2 text-sm text-white hover:bg-blue-700 disabled:opacity-50"
          >
            {createAccount.isPending ? "Creating..." : "Create Account"}
          </button>
          <button
            type="button"
            onClick={() => router.back()}
            className="rounded border px-6 py-2 text-sm text-gray-600 hover:bg-gray-50"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  )
}
```

- [ ] **Step 3: Create billing account detail page**

```tsx
"use client"

import { useParams } from "next/navigation"
import Link from "next/link"
import {
  useBillingAccount,
  useBillingAccountBalance,
  useAddBillingAccountRelatedParty,
  useRemoveBillingAccountRelatedParty,
} from "@/api/hooks/useBillingAccounts"
import { useState } from "react"

export default function BillingAccountDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: account, isLoading } = useBillingAccount(id)
  const { data: balance } = useBillingAccountBalance(id)
  const addParty = useAddBillingAccountRelatedParty()
  const removeParty = useRemoveBillingAccountRelatedParty()
  const [newParty, setNewParty] = useState({ partyId: "", partyName: "", role: "" })

  if (isLoading) return <div className="p-6 text-gray-500">Loading...</div>
  if (!account) return <div className="p-6 text-red-500">Account not found</div>

  const handleAddParty = async (e: React.FormEvent) => {
    e.preventDefault()
    await addParty.mutateAsync({ billingAccountId: id, ...newParty })
    setNewParty({ partyId: "", partyName: "", role: "" })
  }

  return (
    <div className="max-w-3xl mx-auto p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold">{account.name}</h1>
          <p className="text-sm text-gray-500">{account.accountType} &middot; {account.status}</p>
        </div>
        <Link
          href={`/billing/accounts/${id}/edit`}
          className="rounded border px-4 py-2 text-sm text-gray-600 hover:bg-gray-50"
        >
          Edit
        </Link>
      </div>

      {/* Info card */}
      <div className="rounded border p-4 grid grid-cols-2 gap-4 text-sm">
        <div><span className="text-gray-500">Customer ID:</span> <span className="font-mono">{account.customerId}</span></div>
        <div><span className="text-gray-500">Currency:</span> {account.currency}</div>
        <div><span className="text-gray-500">Credit Limit:</span> {account.creditLimit.toLocaleString()}</div>
        <div><span className="text-gray-500">Description:</span> {account.description ?? "—"}</div>
        {account.accountHolder && (
          <>
            <div><span className="text-gray-500">Holder Name:</span> {account.accountHolder.name}</div>
            <div><span className="text-gray-500">Holder Email:</span> {account.accountHolder.email ?? "—"}</div>
          </>
        )}
      </div>

      {/* Balance widget */}
      {balance && (
        <div className="rounded border p-4">
          <h2 className="text-lg font-medium mb-3">Balance</h2>
          <div className="grid grid-cols-3 gap-4 text-sm">
            <div className="bg-blue-50 rounded p-3">
              <div className="text-blue-600 font-medium">Current</div>
              <div className="text-lg font-semibold">{balance.currentBalance.toLocaleString()} {balance.currency}</div>
            </div>
            <div className="bg-yellow-50 rounded p-3">
              <div className="text-yellow-600 font-medium">Outstanding</div>
              <div className="text-lg font-semibold">{balance.outstandingBalance.toLocaleString()} {balance.currency}</div>
            </div>
            <div className="bg-green-50 rounded p-3">
              <div className="text-green-600 font-medium">Available Credit</div>
              <div className="text-lg font-semibold">{balance.availableCredit.toLocaleString()} {balance.currency}</div>
            </div>
          </div>
        </div>
      )}

      {/* Related Parties */}
      <div className="rounded border p-4">
        <h2 className="text-lg font-medium mb-3">Related Parties</h2>
        {account.relatedParties && account.relatedParties.length > 0 ? (
          <table className="w-full text-sm mb-4">
            <thead>
              <tr className="border-b text-left text-gray-500">
                <th className="pb-2">Party ID</th>
                <th className="pb-2">Name</th>
                <th className="pb-2">Role</th>
                <th className="pb-2"></th>
              </tr>
            </thead>
            <tbody>
              {account.relatedParties.map((rp) => (
                <tr key={rp.partyId} className="border-b">
                  <td className="py-2 font-mono text-xs">{rp.partyId}</td>
                  <td className="py-2">{rp.partyName}</td>
                  <td className="py-2">{rp.role}</td>
                  <td className="py-2 text-right">
                    <button
                      onClick={() => removeParty.mutate({ billingAccountId: id, partyId: rp.partyId })}
                      className="text-xs text-red-600 hover:underline"
                    >
                      Remove
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <p className="text-sm text-gray-400 mb-4">No related parties</p>
        )}
        <form onSubmit={handleAddParty} className="flex gap-2 items-end">
          <input
            placeholder="Party ID"
            value={newParty.partyId}
            onChange={(e) => setNewParty({ ...newParty, partyId: e.target.value })}
            className="rounded border px-2 py-1 text-xs flex-1"
            required
          />
          <input
            placeholder="Name"
            value={newParty.partyName}
            onChange={(e) => setNewParty({ ...newParty, partyName: e.target.value })}
            className="rounded border px-2 py-1 text-xs flex-1"
            required
          />
          <input
            placeholder="Role"
            value={newParty.role}
            onChange={(e) => setNewParty({ ...newParty, role: e.target.value })}
            className="rounded border px-2 py-1 text-xs w-24"
            required
          />
          <button type="submit" className="rounded bg-blue-600 px-3 py-1 text-xs text-white hover:bg-blue-700">
            Add
          </button>
        </form>
      </div>
    </div>
  )
}
```

- [ ] **Step 4: Create billing account edit page**

```tsx
"use client"

import { useState, useEffect } from "react"
import { useParams, useRouter } from "next/navigation"
import { useBillingAccount, useUpdateBillingAccount } from "@/api/hooks/useBillingAccounts"

export default function EditBillingAccountPage() {
  const { id } = useParams<{ id: string }>()
  const router = useRouter()
  const { data: account, isLoading } = useBillingAccount(id)
  const updateAccount = useUpdateBillingAccount()
  const [form, setForm] = useState({
    name: "",
    creditLimit: 0,
    currency: "YER",
    description: "",
  })

  useEffect(() => {
    if (account) {
      setForm({
        name: account.name,
        creditLimit: account.creditLimit,
        currency: account.currency,
        description: account.description ?? "",
      })
    }
  }, [account])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    await updateAccount.mutateAsync({ id, ...form, description: form.description || null })
    router.push(`/billing/accounts/${id}`)
  }

  if (isLoading) return <div className="p-6 text-gray-500">Loading...</div>
  if (!account) return <div className="p-6 text-red-500">Account not found</div>

  return (
    <div className="max-w-lg mx-auto p-6">
      <h1 className="text-2xl font-semibold mb-6">Edit Billing Account</h1>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
          <input
            type="text"
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            className="w-full rounded border px-3 py-2 text-sm"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Credit Limit</label>
          <input
            type="number"
            value={form.creditLimit}
            onChange={(e) => setForm({ ...form, creditLimit: Number(e.target.value) })}
            className="w-full rounded border px-3 py-2 text-sm"
            min="0"
            required
          />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Currency</label>
          <select
            value={form.currency}
            onChange={(e) => setForm({ ...form, currency: e.target.value })}
            className="w-full rounded border px-3 py-2 text-sm"
          >
            <option value="YER">YER</option>
            <option value="USD">USD</option>
            <option value="SAR">SAR</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Description</label>
          <textarea
            value={form.description}
            onChange={(e) => setForm({ ...form, description: e.target.value })}
            className="w-full rounded border px-3 py-2 text-sm"
            rows={3}
          />
        </div>
        <div className="flex gap-3 pt-2">
          <button
            type="submit"
            disabled={updateAccount.isPending}
            className="rounded bg-blue-600 px-6 py-2 text-sm text-white hover:bg-blue-700 disabled:opacity-50"
          >
            {updateAccount.isPending ? "Saving..." : "Save"}
          </button>
          <button
            type="button"
            onClick={() => router.back()}
            className="rounded border px-6 py-2 text-sm text-gray-600 hover:bg-gray-50"
          >
            Cancel
          </button>
        </div>
      </form>
    </div>
  )
}
```

- [ ] **Step 5: Verify frontend build**

Run: `cd frontend && bun run lint`

Expected: No lint errors.

- [ ] **Step 6: Commit**

```bash
git add frontend/src/app/billing/
git commit -m "feat: add billing account frontend pages (list, new, detail, edit)"
```

---

### Task 14: Full Build Verification

- [ ] **Step 1: Build entire solution**

Run: `dotnet build Obss.sln --configuration Release`

Expected: Build succeeded with 0 warnings and 0 errors.

- [ ] **Step 2: Run format check**

Run: `dotnet format --verify-no-changes --verbosity diagnostic`

Expected: No formatting issues in modified files. Pre-existing CHARSET warnings in migration files are acceptable.

- [ ] **Step 3: Verify frontend build**

Run: `cd frontend && bun run lint`

Expected: No lint errors (pre-existing errors unchanged).

- [ ] **Step 4: Final commit with build fixes (if any)**

```bash
git commit -am "fix: address build and formatting issues"
```

---

### Task 15: Merge

- [ ] **Step 1: Check status and create PR**

```bash
git status
git log --oneline -10
git push origin HEAD
gh pr create --title "WP-023: BillingAccount TMF666 alignment" --body "Completes TMF666 BillingAccount resource with AccountBalance, BillPresentationMedia, AccountHolder, PATCH/DELETE endpoints, related party management, integration events, and frontend pages."

gh pr merge --squash
```
