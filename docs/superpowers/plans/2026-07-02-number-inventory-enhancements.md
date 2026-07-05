# NumberInventory Module Enhancements Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add missing state transition endpoints (Reserve, Suspend, Resume, Disconnect, PortIn, PortOut) and frontend coverage to the NumberInventory module.

**Architecture:** Follow existing patterns: one command record + handler + validator per transition, POST endpoints matching the assign/release pattern, React Query hooks matching existing hook patterns, action buttons in the Actions tab conditioned on current status.

**Tech Stack:** .NET 9 (MediatR, FluentValidation, Mapster), React 19 (@tanstack/react-query), Next.js 16

---

### Task 1: Add `Resume()` method to TelephoneNumber entity

**Files:**
- Modify: `src/Modules/NumberInventory/Obss.NumberInventory.Domain/Entities/TelephoneNumber.cs` (after line 104, before Suspend)

- [ ] **Step 1: Add Resume method**

Insert `Resume()` after `Release()` (after line 104), before `Suspend()`:

```csharp
public void Resume()
{
    if (Status != NumberStatus.Suspended)
        throw new InvalidNumberStateException(
            $"Cannot resume number in '{Status}' status. Only 'Suspended' numbers can be resumed.");

    Status = NumberStatus.Assigned;
    UpdatedAt = DateTime.UtcNow;
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/Modules/NumberInventory/Obss.NumberInventory.Domain/Obss.NumberInventory.Domain.csproj --configuration Release`
Expected: Build succeeded, 0 Warnings, 0 Errors

- [ ] **Step 3: Commit**

```bash
git add src/Modules/NumberInventory/Obss.NumberInventory.Domain/Entities/TelephoneNumber.cs
git commit -m "feat: add Resume method to TelephoneNumber entity"
```

### Task 2: Create ReserveNumber command + handler + validator

**Files:**
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/ReserveNumber/ReserveNumberCommand.cs`
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/ReserveNumber/ReserveNumberCommandHandler.cs`
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/ReserveNumber/ReserveNumberCommandValidator.cs`

- [ ] **Step 1: Create ReserveNumberCommand.cs**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.ReserveNumber;

public sealed record ReserveNumberCommand(Guid NumberId) : IRequest<Result>;
```

- [ ] **Step 2: Create ReserveNumberCommandHandler.cs**

```csharp
using MediatR;
using Obss.NumberInventory.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.ReserveNumber;

public sealed class ReserveNumberCommandHandler : IRequestHandler<ReserveNumberCommand, Result>
{
    private readonly ITelephoneNumberRepository _numberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReserveNumberCommandHandler(
        ITelephoneNumberRepository numberRepository,
        IUnitOfWork unitOfWork)
    {
        _numberRepository = numberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReserveNumberCommand request, CancellationToken cancellationToken)
    {
        var number = await _numberRepository.GetByIdAsync(request.NumberId, cancellationToken);

        if (number is null)
            return Result.Failure(Error.NotFound("TelephoneNumber", request.NumberId));

        number.Reserve();

        await _numberRepository.UpdateAsync(number, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

- [ ] **Step 3: Create ReserveNumberCommandValidator.cs**

```csharp
using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.ReserveNumber;

internal sealed class ReserveNumberCommandValidator : AbstractValidator<ReserveNumberCommand>
{
    public ReserveNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build src/Modules/NumberInventory/Obss.NumberInventory.Application/Obss.NumberInventory.Application.csproj --configuration Release`
Expected: Build succeeded, 0 Warnings, 0 Errors

- [ ] **Step 5: Commit**

```bash
git add src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/ReserveNumber/
git commit -m "feat: add ReserveNumber command"
```

### Task 3: Create SuspendNumber command + handler + validator

**Files:**
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/SuspendNumber/SuspendNumberCommand.cs`
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/SuspendNumber/SuspendNumberCommandHandler.cs`
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/SuspendNumber/SuspendNumberCommandValidator.cs`

- [ ] **Step 1: Create SuspendNumberCommand.cs**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.SuspendNumber;

public sealed record SuspendNumberCommand(Guid NumberId) : IRequest<Result>;
```

- [ ] **Step 2: Create SuspendNumberCommandHandler.cs** (same pattern as ReserveNumber, calls `number.Suspend()`)

```csharp
using MediatR;
using Obss.NumberInventory.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.SuspendNumber;

public sealed class SuspendNumberCommandHandler : IRequestHandler<SuspendNumberCommand, Result>
{
    private readonly ITelephoneNumberRepository _numberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SuspendNumberCommandHandler(
        ITelephoneNumberRepository numberRepository,
        IUnitOfWork unitOfWork)
    {
        _numberRepository = numberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SuspendNumberCommand request, CancellationToken cancellationToken)
    {
        var number = await _numberRepository.GetByIdAsync(request.NumberId, cancellationToken);

        if (number is null)
            return Result.Failure(Error.NotFound("TelephoneNumber", request.NumberId));

        number.Suspend();

        await _numberRepository.UpdateAsync(number, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

- [ ] **Step 3: Create SuspendNumberCommandValidator.cs**

```csharp
using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.SuspendNumber;

internal sealed class SuspendNumberCommandValidator : AbstractValidator<SuspendNumberCommand>
{
    public SuspendNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build src/Modules/NumberInventory/Obss.NumberInventory.Application/Obss.NumberInventory.Application.csproj --configuration Release`

- [ ] **Step 5: Commit**

```bash
git add src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/SuspendNumber/
git commit -m "feat: add SuspendNumber command"
```

### Task 4: Create ResumeNumber command + handler + validator

**Files:**
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/ResumeNumber/ResumeNumberCommand.cs`
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/ResumeNumber/ResumeNumberCommandHandler.cs`
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/ResumeNumber/ResumeNumberCommandValidator.cs`

- [ ] **Step 1: Create files** (same pattern as SuspendNumber, calls `number.Resume()`)

```csharp
// ResumeNumberCommand.cs
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.ResumeNumber;

public sealed record ResumeNumberCommand(Guid NumberId) : IRequest<Result>;
```

```csharp
// ResumeNumberCommandHandler.cs
using MediatR;
using Obss.NumberInventory.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.ResumeNumber;

public sealed class ResumeNumberCommandHandler : IRequestHandler<ResumeNumberCommand, Result>
{
    private readonly ITelephoneNumberRepository _numberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResumeNumberCommandHandler(
        ITelephoneNumberRepository numberRepository,
        IUnitOfWork unitOfWork)
    {
        _numberRepository = numberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ResumeNumberCommand request, CancellationToken cancellationToken)
    {
        var number = await _numberRepository.GetByIdAsync(request.NumberId, cancellationToken);

        if (number is null)
            return Result.Failure(Error.NotFound("TelephoneNumber", request.NumberId));

        number.Resume();

        await _numberRepository.UpdateAsync(number, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

```csharp
// ResumeNumberCommandValidator.cs
using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.ResumeNumber;

internal sealed class ResumeNumberCommandValidator : AbstractValidator<ResumeNumberCommand>
{
    public ResumeNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
```

- [ ] **Step 2: Verify build**

Run: `dotnet build src/Modules/NumberInventory/Obss.NumberInventory.Application/Obss.NumberInventory.Application.csproj --configuration Release`

- [ ] **Step 3: Commit**

```bash
git add src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/ResumeNumber/
git commit -m "feat: add ResumeNumber command"
```

### Task 5: Create DisconnectNumber command + handler + validator

**Files:**
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/DisconnectNumber/DisconnectNumberCommand.cs`
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/DisconnectNumber/DisconnectNumberCommandHandler.cs`
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/DisconnectNumber/DisconnectNumberCommandValidator.cs`

- [ ] **Step 1: Create DisconnectNumberCommand.cs**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.DisconnectNumber;

public sealed record DisconnectNumberCommand(Guid NumberId) : IRequest<Result>;
```

- [ ] **Step 2: Create DisconnectNumberCommandHandler.cs**

```csharp
using MediatR;
using Obss.NumberInventory.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.DisconnectNumber;

public sealed class DisconnectNumberCommandHandler : IRequestHandler<DisconnectNumberCommand, Result>
{
    private readonly ITelephoneNumberRepository _numberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DisconnectNumberCommandHandler(
        ITelephoneNumberRepository numberRepository,
        IUnitOfWork unitOfWork)
    {
        _numberRepository = numberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DisconnectNumberCommand request, CancellationToken cancellationToken)
    {
        var number = await _numberRepository.GetByIdAsync(request.NumberId, cancellationToken);

        if (number is null)
            return Result.Failure(Error.NotFound("TelephoneNumber", request.NumberId));

        number.Disconnect();

        await _numberRepository.UpdateAsync(number, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

- [ ] **Step 3: Create DisconnectNumberCommandValidator.cs**

```csharp
using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.DisconnectNumber;

internal sealed class DisconnectNumberCommandValidator : AbstractValidator<DisconnectNumberCommand>
{
    public DisconnectNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build src/Modules/NumberInventory/Obss.NumberInventory.Application/Obss.NumberInventory.Application.csproj --configuration Release`

- [ ] **Step 5: Commit**

```bash
git add src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/DisconnectNumber/
git commit -m "feat: add DisconnectNumber command"
```

### Task 6: Create PortInNumber command + handler + validator

**Files:**
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/PortInNumber/PortInNumberCommand.cs`
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/PortInNumber/PortInNumberCommandHandler.cs`
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/PortInNumber/PortInNumberCommandValidator.cs`

- [ ] **Step 1: Create PortInNumberCommand.cs**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.PortInNumber;

public sealed record PortInNumberCommand(Guid NumberId, Guid? CustomerId) : IRequest<Result>;
```

- [ ] **Step 2: Create PortInNumberCommandHandler.cs** (calls `number.PortIn(request.CustomerId)`)

```csharp
using MediatR;
using Obss.NumberInventory.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.PortInNumber;

public sealed class PortInNumberCommandHandler : IRequestHandler<PortInNumberCommand, Result>
{
    private readonly ITelephoneNumberRepository _numberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PortInNumberCommandHandler(
        ITelephoneNumberRepository numberRepository,
        IUnitOfWork unitOfWork)
    {
        _numberRepository = numberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(PortInNumberCommand request, CancellationToken cancellationToken)
    {
        var number = await _numberRepository.GetByIdAsync(request.NumberId, cancellationToken);

        if (number is null)
            return Result.Failure(Error.NotFound("TelephoneNumber", request.NumberId));

        number.PortIn(request.CustomerId);

        await _numberRepository.UpdateAsync(number, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

- [ ] **Step 3: Create PortInNumberCommandValidator.cs**

```csharp
using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.PortInNumber;

internal sealed class PortInNumberCommandValidator : AbstractValidator<PortInNumberCommand>
{
    public PortInNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
```

- [ ] **Step 4: Verify build**

- [ ] **Step 5: Commit**

### Task 7: Create PortOutNumber command + handler + validator

**Files:**
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/PortOutNumber/PortOutNumberCommand.cs`
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/PortOutNumber/PortOutNumberCommandHandler.cs`
- Create: `src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/PortOutNumber/PortOutNumberCommandValidator.cs`

- [ ] **Step 1: Create PortOutNumberCommand.cs**

```csharp
using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.PortOutNumber;

public sealed record PortOutNumberCommand(Guid NumberId) : IRequest<Result>;
```

- [ ] **Step 2: Create PortOutNumberCommandHandler.cs**

```csharp
using MediatR;
using Obss.NumberInventory.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.PortOutNumber;

public sealed class PortOutNumberCommandHandler : IRequestHandler<PortOutNumberCommand, Result>
{
    private readonly ITelephoneNumberRepository _numberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PortOutNumberCommandHandler(
        ITelephoneNumberRepository numberRepository,
        IUnitOfWork unitOfWork)
    {
        _numberRepository = numberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(PortOutNumberCommand request, CancellationToken cancellationToken)
    {
        var number = await _numberRepository.GetByIdAsync(request.NumberId, cancellationToken);

        if (number is null)
            return Result.Failure(Error.NotFound("TelephoneNumber", request.NumberId));

        number.PortOut();

        await _numberRepository.UpdateAsync(number, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
```

- [ ] **Step 3: Create PortOutNumberCommandValidator.cs**

```csharp
using FluentValidation;

namespace Obss.NumberInventory.Application.Commands.PortOutNumber;

internal sealed class PortOutNumberCommandValidator : AbstractValidator<PortOutNumberCommand>
{
    public PortOutNumberCommandValidator()
    {
        RuleFor(x => x.NumberId)
            .NotEmpty().WithMessage("Number ID is required.");
    }
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build src/Modules/NumberInventory/Obss.NumberInventory.Application/Obss.NumberInventory.Application.csproj --configuration Release`

- [ ] **Step 5: Commit**

```bash
git add src/Modules/NumberInventory/Obss.NumberInventory.Application/Commands/PortOutNumber/
git commit -m "feat: add PortOutNumber command"
```

### Task 8: Add 6 new endpoints to NumberEndpoints.cs

**Files:**
- Modify: `src/Modules/NumberInventory/Obss.NumberInventory.Api/Endpoints/NumberEndpoints.cs`

- [ ] **Step 1: Add imports and endpoints**

Add imports:
```csharp
using Obss.NumberInventory.Application.Commands.ReserveNumber;
using Obss.NumberInventory.Application.Commands.SuspendNumber;
using Obss.NumberInventory.Application.Commands.ResumeNumber;
using Obss.NumberInventory.Application.Commands.DisconnectNumber;
using Obss.NumberInventory.Application.Commands.PortInNumber;
using Obss.NumberInventory.Application.Commands.PortOutNumber;
```

Add endpoints before the closing `}` of `Map`:
```csharp
group.MapPost("/numbers/{id:guid}/reserve", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new ReserveNumberCommand(id));
    return result.IsSuccess
        ? (IResult)TypedResults.NoContent()
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapPost("/numbers/{id:guid}/suspend", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new SuspendNumberCommand(id));
    return result.IsSuccess
        ? (IResult)TypedResults.NoContent()
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapPost("/numbers/{id:guid}/resume", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new ResumeNumberCommand(id));
    return result.IsSuccess
        ? (IResult)TypedResults.NoContent()
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapPost("/numbers/{id:guid}/disconnect", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new DisconnectNumberCommand(id));
    return result.IsSuccess
        ? (IResult)TypedResults.NoContent()
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapPost("/numbers/{id:guid}/port-in", async (Guid id, PortInNumberCommand command, IMediator mediator) =>
{
    if (id != command.NumberId)
        return (IResult)TypedResults.BadRequest();
    var result = await mediator.Send(command);
    return result.IsSuccess
        ? (IResult)TypedResults.NoContent()
        : (IResult)TypedResults.BadRequest(result.Error);
});

group.MapPost("/numbers/{id:guid}/port-out", async (Guid id, IMediator mediator) =>
{
    var result = await mediator.Send(new PortOutNumberCommand(id));
    return result.IsSuccess
        ? (IResult)TypedResults.NoContent()
        : (IResult)TypedResults.BadRequest(result.Error);
});
```

- [ ] **Step 2: Verify full solution build**

Run: `dotnet build Obss.sln --configuration Release`
Expected: Build succeeded, 0 Warnings, 0 Errors

- [ ] **Step 3: Commit**

```bash
git add src/Modules/NumberInventory/Obss.NumberInventory.Api/Endpoints/NumberEndpoints.cs
git commit -m "feat: add reserve/suspend/resume/disconnect/port-in/port-out endpoints"
```

### Task 9: Create frontend hooks (6 new files)

**Files:**
- Create: `frontend/src/api/hooks/useReserveTelephoneNumber.ts`
- Create: `frontend/src/api/hooks/useSuspendTelephoneNumber.ts`
- Create: `frontend/src/api/hooks/useResumeTelephoneNumber.ts`
- Create: `frontend/src/api/hooks/useDisconnectTelephoneNumber.ts`
- Create: `frontend/src/api/hooks/usePortInTelephoneNumber.ts`
- Create: `frontend/src/api/hooks/usePortOutTelephoneNumber.ts`

- [ ] **Step 1: Create all 6 hook files** (same pattern as `useReleaseTelephoneNumber.ts`)

```typescript
// useReserveTelephoneNumber.ts (and same pattern for suspend, resume, disconnect, port-out)
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useReserveTelephoneNumber() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/number-inventory/numbers/${id}/reserve`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.numberInventory.numbers.lists() })
      queryClient.invalidateQueries({ queryKey: queryKeys.numberInventory.numbers.details() })
    },
  })
}
```

For `useSuspendTelephoneNumber.ts` → `/suspend`
For `useResumeTelephoneNumber.ts` → `/resume`
For `useDisconnectTelephoneNumber.ts` → `/disconnect`
For `usePortOutTelephoneNumber.ts` → `/port-out`

For `usePortInTelephoneNumber.ts` (needs CustomerId body):
```typescript
import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function usePortInTelephoneNumber() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ id, ...data }: { id: string } & Record<string, unknown>) => {
      const res = await api.post(`/api/v1/number-inventory/numbers/${id}/port-in`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.numberInventory.numbers.lists() })
      queryClient.invalidateQueries({ queryKey: queryKeys.numberInventory.numbers.details() })
    },
  })
}
```

- [ ] **Step 2: Verify frontend typecheck**

Run: `cd frontend && bun run build` (or `bun run lint` for faster check)

- [ ] **Step 3: Commit**

```bash
git add frontend/src/api/hooks/useReserveTelephoneNumber.ts frontend/src/api/hooks/useSuspendTelephoneNumber.ts frontend/src/api/hooks/useResumeTelephoneNumber.ts frontend/src/api/hooks/useDisconnectTelephoneNumber.ts frontend/src/api/hooks/usePortInTelephoneNumber.ts frontend/src/api/hooks/usePortOutTelephoneNumber.ts
git commit -m "feat: add telephone number state transition hooks"
```

### Task 10: Add action buttons to number detail page

**Files:**
- Modify: `frontend/src/app/number-inventory/[id]/page.tsx`

- [ ] **Step 1: Add imports** for all new hooks

Add after existing hook imports:
```typescript
import { useReserveTelephoneNumber } from "@/api/hooks/useReserveTelephoneNumber"
import { useSuspendTelephoneNumber } from "@/api/hooks/useSuspendTelephoneNumber"
import { useResumeTelephoneNumber } from "@/api/hooks/useResumeTelephoneNumber"
import { useDisconnectTelephoneNumber } from "@/api/hooks/useDisconnectTelephoneNumber"
import { usePortInTelephoneNumber } from "@/api/hooks/usePortInTelephoneNumber"
import { usePortOutTelephoneNumber } from "@/api/hooks/usePortOutTelephoneNumber"
```

- [ ] **Step 2: Initialize mutations**

Add after existing mutation hooks:
```typescript
const reserveMutation = useReserveTelephoneNumber()
const suspendMutation = useSuspendTelephoneNumber()
const resumeMutation = useResumeTelephoneNumber()
const disconnectMutation = useDisconnectTelephoneNumber()
const portInMutation = usePortInTelephoneNumber()
const portOutMutation = usePortOutTelephoneNumber()
```

- [ ] **Step 3: Add handler functions**

Add before the `tabs` definition:
```typescript
const handleReserve = () => {
  reserveMutation.mutate(id, {
    onSuccess: () => toast({ title: "Number reserved", description: `Number ${tel?.number} has been reserved.` }),
    onError: () => toast({ title: "Error", description: "Failed to reserve number.", variant: "destructive" }),
  })
}

const handleSuspend = () => {
  suspendMutation.mutate(id, {
    onSuccess: () => toast({ title: "Number suspended", description: `Number ${tel?.number} has been suspended.` }),
    onError: () => toast({ title: "Error", description: "Failed to suspend number.", variant: "destructive" }),
  })
}

const handleResume = () => {
  resumeMutation.mutate(id, {
    onSuccess: () => toast({ title: "Number resumed", description: `Number ${tel?.number} has been resumed.` }),
    onError: () => toast({ title: "Error", description: "Failed to resume number.", variant: "destructive" }),
  })
}

const handleDisconnect = () => {
  disconnectMutation.mutate(id, {
    onSuccess: () => toast({ title: "Number disconnected", description: `Number ${tel?.number} has been disconnected.` }),
    onError: () => toast({ title: "Error", description: "Failed to disconnect number.", variant: "destructive" }),
  })
}

const handlePortIn = () => {
  portInMutation.mutate({ id }, {
    onSuccess: () => toast({ title: "Number ported in", description: `Number ${tel?.number} has been ported in.` }),
    onError: () => toast({ title: "Error", description: "Failed to port in number.", variant: "destructive" }),
  })
}

const handlePortOut = () => {
  portOutMutation.mutate(id, {
    onSuccess: () => toast({ title: "Number ported out", description: `Number ${tel?.number} has been ported out.` }),
    onError: () => toast({ title: "Error", description: "Failed to port out number.", variant: "destructive" }),
  })
}
```

- [ ] **Step 4: Replace the Actions tab content**

Replace the Actions tab content (lines 83-113) to add all state transition buttons conditioned on current status:

```typescript
      content: (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">Actions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            {tel?.status === "Available" && (
              <div className="flex flex-wrap gap-3">
                <div className="flex items-end gap-3 w-full">
                  <div className="flex-1 space-y-2">
                    <label className="text-sm font-medium">Customer ID</label>
                    <Input
                      placeholder="Enter customer ID"
                      value={customerId}
                      onChange={(e) => setCustomerId(e.target.value)}
                    />
                  </div>
                  <Button onClick={handleAssign} disabled={assignMutation.isPending}>
                    {assignMutation.isPending ? "Assigning..." : "Assign"}
                  </Button>
                </div>
                <Button variant="outline" onClick={handleReserve} disabled={reserveMutation.isPending}>
                  {reserveMutation.isPending ? "Reserving..." : "Reserve"}
                </Button>
                <Button variant="outline" onClick={handlePortIn} disabled={portInMutation.isPending}>
                  {portInMutation.isPending ? "Porting In..." : "Port In"}
                </Button>
              </div>
            )}
            {tel?.status === "Reserved" && (
              <div className="flex flex-wrap gap-3">
                <div className="flex items-end gap-3 w-full">
                  <div className="flex-1 space-y-2">
                    <label className="text-sm font-medium">Customer ID</label>
                    <Input
                      placeholder="Enter customer ID"
                      value={customerId}
                      onChange={(e) => setCustomerId(e.target.value)}
                    />
                  </div>
                  <Button onClick={handleAssign} disabled={assignMutation.isPending}>
                    {assignMutation.isPending ? "Assigning..." : "Assign"}
                  </Button>
                </div>
                <Button variant="destructive" onClick={handleRelease} disabled={releaseMutation.isPending}>
                  {releaseMutation.isPending ? "Releasing..." : "Release"}
                </Button>
              </div>
            )}
            {tel?.status === "Assigned" && (
              <div className="flex flex-wrap gap-3">
                <Button variant="outline" onClick={handleSuspend} disabled={suspendMutation.isPending}>
                  {suspendMutation.isPending ? "Suspending..." : "Suspend"}
                </Button>
                <Button variant="outline" onClick={handlePortOut} disabled={portOutMutation.isPending}>
                  {portOutMutation.isPending ? "Porting Out..." : "Port Out"}
                </Button>
                <Button variant="destructive" onClick={handleDisconnect} disabled={disconnectMutation.isPending}>
                  {disconnectMutation.isPending ? "Disconnecting..." : "Disconnect"}
                </Button>
              </div>
            )}
            {tel?.status === "Ported" && (
              <div className="flex flex-wrap gap-3">
                <Button variant="outline" onClick={handleSuspend} disabled={suspendMutation.isPending}>
                  {suspendMutation.isPending ? "Suspending..." : "Suspend"}
                </Button>
                <Button variant="outline" onClick={handlePortOut} disabled={portOutMutation.isPending}>
                  {portOutMutation.isPending ? "Porting Out..." : "Port Out"}
                </Button>
                <Button variant="destructive" onClick={handleDisconnect} disabled={disconnectMutation.isPending}>
                  {disconnectMutation.isPending ? "Disconnecting..." : "Disconnect"}
                </Button>
              </div>
            )}
            {tel?.status === "Suspended" && (
              <div className="flex flex-wrap gap-3">
                <Button variant="outline" onClick={handleResume} disabled={resumeMutation.isPending}>
                  {resumeMutation.isPending ? "Resuming..." : "Resume"}
                </Button>
                <Button variant="destructive" onClick={handleDisconnect} disabled={disconnectMutation.isPending}>
                  {disconnectMutation.isPending ? "Disconnecting..." : "Disconnect"}
                </Button>
              </div>
            )}
            {tel?.status === "Disconnected" && (
              <p className="text-sm text-muted-foreground">No actions available for disconnected numbers.</p>
            )}
          </CardContent>
        </Card>
      ),
```

- [ ] **Step 5: Verify frontend build**

Run: `cd frontend && bun run build`

- [ ] **Step 6: Commit**

```bash
git add frontend/src/app/number-inventory/
git commit -m "feat: add action buttons for all telephone number state transitions"
```

### Task 11: Add Number Inventory link to sidebar

**Files:**
- Modify: `frontend/src/components/shared/ModuleSidebar.tsx`

- [ ] **Step 1: Add Phone icon import**

Add `Phone` to the lucide-react imports:
```typescript
import {
  LayoutDashboard,
  Users,
  Package,
  ShoppingCart,
  ClipboardList,
  FileText,
  CreditCard,
  DollarSign,
  Scale,
  Ticket,
  Shield,
  Network,
  Cable,
  Settings,
  Bell,
  FileBarChart,
  ScrollText,
  Waypoints,
  LogOut,
  ChevronLeft,
  Phone,
} from "lucide-react"
```

- [ ] **Step 2: Add Number Inventory nav item**

Insert after the `{ href: "/network", label: "Network", icon: Cable }` line:
```typescript
  { href: "/number-inventory", label: "Numbers", icon: Phone },
```

- [ ] **Step 3: Verify frontend build**

Run: `cd frontend && bun run build`

- [ ] **Step 4: Commit**

```bash
git add frontend/src/components/shared/ModuleSidebar.tsx
git commit -m "feat: add Number Inventory link to sidebar"
```

### Task 12: Final verification

- [ ] **Step 1: Run full backend build**

Run: `dotnet build Obss.sln --configuration Release`
Expected: 0 Warnings, 0 Errors

- [ ] **Step 2: Run full frontend build**

Run: `cd frontend && bun run build`
Expected: Build succeeds

- [ ] **Step 3: Run tests**

Run: `dotnet test --no-build --configuration Release`
Expected: All tests pass

- [ ] **Step 4: Final commit**

```bash
git add -A
git commit -m "chore: complete NumberInventory module enhancements"
```
