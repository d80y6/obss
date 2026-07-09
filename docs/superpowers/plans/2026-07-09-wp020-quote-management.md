# WP-020: Quote Management (TMF648) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a TMF648-compliant `Quote` aggregate root to the CRM module alongside existing `Customer`, `Agreement`. Quote lifecycle: InProgress → Pending → Approved → Accepted/Rejected/Cancelled.

**Architecture:** New `Quote` aggregate root in CRM module with `QuoteItem` child entity. Reuses existing CRM value objects (`RelatedParty`, `AccountRef`, `AgreementRef`, `ContactMedium`). Owned value objects stored as JSONB columns in PostgreSQL. Quote acceptance triggers Order creation via integration event.

**Tech Stack:** .NET 9, EF Core/Npgsql, MediatR, Mapster, FluentValidation, Next.js/React/TanStack Query

**Design Spec:** `docs/superpowers/specs/2026-07-09-wp020-quote-management-design.md`

---

### Task 1: Domain — Enums, value objects, and domain events

**Files (Domain layer):**
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/QuoteState.cs` (enum)
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/QuoteItemAction.cs` (enum)
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/QuoteItemState.cs` (enum)
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/PriceType.cs` (enum, if not already exists)
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/AuthorizationState.cs` (enum)
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/QuotePrice.cs` (record)
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/PriceAlteration.cs` (record)
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/QuoteAuthorization.cs` (record)
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/QuoteItemRelationship.cs` (record)
- Create: `src/Modules/CRM/Obss.CRM.Domain/ValueObjects/Note.cs` (record, if not already exists)
- Create: `src/Modules/CRM/Obss.CRM.Domain/Events/QuoteCreatedDomainEvent.cs`
- Create: `src/Modules/CRM/Obss.CRM.Domain/Events/QuoteStateChangedDomainEvent.cs`
- Create: `src/Modules/CRM/Obss.CRM.Domain/Events/QuoteAcceptedDomainEvent.cs`
- Create: `src/Modules/CRM/Obss.CRM.Domain/Events/QuoteItemStateChangedDomainEvent.cs`

- [ ] **Step 1: Create enums**
- [ ] **Step 2: Create value object records**
- [ ] **Step 3: Create domain events**
- [ ] **Step 4: Commit**

---

### Task 2: Domain — Quote aggregate root and QuoteItem entity

**Files:**
- Create: `src/Modules/CRM/Obss.CRM.Domain/Entities/Quote.cs`
- Create: `src/Modules/CRM/Obss.CRM.Domain/Entities/QuoteItem.cs`

- [ ] **Step 1: Create QuoteItem entity**
- [ ] **Step 2: Create Quote aggregate root**
- [ ] **Step 3: Commit**

---

### Task 3: Application — Repository, DTOs, and mapping

**Files:**
- Create: `src/Modules/CRM/Obss.CRM.Application/Abstractions/IQuoteRepository.cs`
- Create: `src/Modules/CRM/Obss.CRM.Application/DTOs/QuoteDto.cs`
- Modify: `src/Modules/CRM/Obss.CRM.Application/Mappings/CrmMappingConfig.cs`

- [ ] **Step 1: Create repository interface**
- [ ] **Step 2: Create DTOs**
- [ ] **Step 3: Update mapping config**
- [ ] **Step 4: Commit**

---

### Task 4: Application — Commands

**Files: 7 command groups (21 files):**
- Create: `Commands/CreateQuote/CreateQuoteCommand.cs`, `CreateQuoteCommandHandler.cs`, `CreateQuoteCommandValidator.cs`
- Create: `Commands/UpdateQuote/UpdateQuoteCommand.cs`, `UpdateQuoteCommandHandler.cs`
- Create: `Commands/SubmitQuote/SubmitQuoteCommand.cs`, `SubmitQuoteCommandHandler.cs`
- Create: `Commands/ApproveQuote/ApproveQuoteCommand.cs`, `ApproveQuoteCommandHandler.cs`
- Create: `Commands/AcceptQuote/AcceptQuoteCommand.cs`, `AcceptQuoteCommandHandler.cs`
- Create: `Commands/RejectQuote/RejectQuoteCommand.cs`, `RejectQuoteCommandHandler.cs`
- Create: `Commands/CancelQuote/CancelQuoteCommand.cs`, `CancelQuoteCommandHandler.cs`
- Create: `Commands/AddQuoteItem/AddQuoteItemCommand.cs`, `AddQuoteItemCommandHandler.cs`
- Create: `Commands/UpdateQuoteItem/UpdateQuoteItemCommand.cs`, `UpdateQuoteItemCommandHandler.cs`
- Create: `Commands/RemoveQuoteItem/RemoveQuoteItemCommand.cs`, `RemoveQuoteItemCommandHandler.cs`

- [ ] **Step 1: CreateQuote command**
- [ ] **Step 2: UpdateQuote command**
- [ ] **Step 3: SubmitQuote command (state transition)**
- [ ] **Step 4: ApproveQuote command (state transition)**
- [ ] **Step 5: AcceptQuote command (state transition + domain event)**
- [ ] **Step 6: RejectQuote command (state transition)**
- [ ] **Step 7: CancelQuote command (state transition)**
- [ ] **Step 8: AddQuoteItem command**
- [ ] **Step 9: UpdateQuoteItem command**
- [ ] **Step 10: RemoveQuoteItem command**
- [ ] **Step 11: Commit**

---

### Task 5: Application — Queries

**Files:**
- Create: `Queries/GetQuoteById/GetQuoteByIdQuery.cs`, `GetQuoteByIdQueryHandler.cs`
- Create: `Queries/GetQuotes/GetQuotesQuery.cs`, `GetQuotesQueryHandler.cs`
- Create: `Queries/GetQuotesByCustomer/GetQuotesByCustomerQuery.cs`, `GetQuotesByCustomerQueryHandler.cs`

- [ ] **Step 1: GetQuoteById query**
- [ ] **Step 2: GetQuotes query (paginated, filterable)**
- [ ] **Step 3: GetQuotesByCustomer query**
- [ ] **Step 4: Commit**

---

### Task 6: Application — Integration events

**Files:**
- Create: `IntegrationEvents/QuoteCreatedIntegrationEvent.cs`
- Create: `IntegrationEvents/QuoteAcceptedIntegrationEvent.cs`
- Create: `IntegrationEvents/QuoteStateChangedIntegrationEvent.cs`

- [ ] **Step 1: Create outbound integration events**
- [ ] **Step 2: Commit**

---

### Task 7: Infrastructure — EF config and repository

**Files:**
- Create: `Persistence/Configurations/QuoteConfiguration.cs`
- Create: `Persistence/Configurations/QuoteItemConfiguration.cs`
- Create: `Persistence/Repositories/QuoteRepository.cs`
- Modify: `Persistence/CrmDbContext.cs` (add DbSet)

- [ ] **Step 1: QuoteConfiguration (jsonb columns for owned VOs)**
- [ ] **Step 2: QuoteItemConfiguration**
- [ ] **Step 3: Repository implementation**
- [ ] **Step 4: Register DbSet in CrmDbContext**
- [ ] **Step 5: Commit**

---

### Task 8: Infrastructure — EF Migration

- [ ] **Step 1: Generate migration (AddQuoteManagementTables)**
- [ ] **Step 2: Build and verify**
- [ ] **Step 3: Commit**

---

### Task 9: API — Quote endpoints

**Files:**
- Create: `Obss.CRM.Api/Endpoints/QuoteEndpoints.cs`
- Modify: `Obss.CRM.Api/Extensions/CrmModuleRegistration.cs`

- [ ] **Step 1: Create QuoteEndpoints (13 endpoints)**
- [ ] **Step 2: Register DI (IQuoteRepository + handlers)**
- [ ] **Step 3: Commit**

---

### Task 10: Frontend — Quotes pages

**Files:**
- Create: `frontend/src/app/crm/quotes/page.tsx`
- Create: `frontend/src/app/crm/quotes/[id]/page.tsx`
- Create: `frontend/src/api/hooks/useQuotes.ts`

- [ ] **Step 1: Create API hooks (useQuotes, useQuote, useCustomerQuotes)**
- [ ] **Step 2: Create quote list page**
- [ ] **Step 3: Create quote detail page**
- [ ] **Step 4: Commit**

---

### Task 11: Full build verification

- [ ] **Step 1: Full solution build (Release, no warnings)**
- [ ] **Step 2: Fix any analyzer issues**
- [ ] **Step 3: Verify frontend build**

---

### Task 12 (bonus): Update Order creation flow

- [ ] **Step 1: Add inbound handler for QuoteAcceptedIntegrationEvent in Orders module**
- [ ] **Step 2: Handler creates Order from accepted Quote items**
- [ ] **Step 3: Commit**
