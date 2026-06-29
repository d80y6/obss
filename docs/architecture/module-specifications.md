# Telecom OSS/BSS Platform — Module Specifications

> **Tech Stack:** .NET 9, ASP.NET Core, EF Core, PostgreSQL, Redis, RabbitMQ, Keycloak  
> **Architecture:** Modular Monolith with event-driven communication between bounded contexts  
> **Last Updated:** 2026-06-20

---

## Table of Contents

1. [IAM — Identity & Access Management](#1-iam--identity--access-management)
2. [CRM — Customer Relationship Management](#2-crm--customer-relationship-management)
3. [ProductCatalog — Product and Service Catalog](#3-productcatalog--product-and-service-catalog)
4. [Orders — Order Management](#4-orders--order-management)
5. [Subscriptions — Subscription Management](#5-subscriptions--subscription-management)
6. [Rating — Usage Rating Engine](#6-rating--usage-rating-engine)
7. [Billing — Billing Engine](#7-billing--billing-engine)
8. [Invoices — Invoice Management](#8-invoices--invoice-management)
9. [Payments — Payment Processing](#9-payments--payment-processing)
10. [Collections — Debt Collection](#10-collections--debt-collection)
11. [ServiceInventory — Service Inventory](#11-serviceinventory--service-inventory)
12. [NetworkInventory — Network Inventory](#12-networkinventory--network-inventory)
13. [Provisioning — Service Provisioning](#13-provisioning--service-provisioning)
14. [Workflow — Workflow Engine](#14-workflow--workflow-engine)
15. [Ticketing — Ticketing/Helpdesk](#15-ticketing--ticketinghelpdesk)
16. [Notifications — Notification Engine](#16-notifications--notification-engine)
17. [Reporting — Reporting & BI](#17-reporting--reporting--bi)
18. [Audit — Audit Trail](#18-audit--audit-trail)
19. [APIGateway — API Gateway](#19-apigateway--api-gateway)

---

## 1. IAM — Identity & Access Management

### 1.1 Domain Layer

#### Aggregate: Tenant

```
Id: Guid
Name: string
Slug: string
LogoUrl: string?
IsActive: bool
Settings: TenantSettings (VO)
CreatedAt: DateTime
UpdatedAt: DateTime

Methods:
  Activate()
  Deactivate()
  UpdateSettings(TenantSettings settings)
```

#### Aggregate: User

```
Id: Guid
TenantId: Guid
Username: string
Email: string
FirstName: string
LastName: string
PhoneNumber: string?
IsActive: bool
EmailVerified: bool
Attributes: Dictionary<string, string>
LastLoginAt: DateTime?
CreatedAt: DateTime
UpdatedAt: DateTime

Methods:
  VerifyEmail()
  UpdateProfile(string firstName, string lastName, string phoneNumber)
  RecordLogin()
  Deactivate()
  AssignRole(Role role)
  RemoveRole(Role role)
  HasPermission(string permission): bool
```

#### Aggregate: Role

```
Id: Guid
TenantId: Guid
Name: string
Description: string
IsSystem: bool
Permissions: List<string>

Methods:
  AddPermission(string permission)
  RemovePermission(string permission)
  HasPermission(string permission): bool
```

#### Value Objects

```
TenantSettings:
  AllowedAuthProviders: List<AuthProvider>
  PasswordPolicy: PasswordPolicy
  SessionTimeoutMinutes: int
  MaxLoginAttempts: int
  RequireMfa: bool
  MfaProviders: List<MfaProvider>

PasswordPolicy:
  MinLength: int
  RequireUppercase: bool
  RequireLowercase: bool
  RequireDigits: bool
  RequireSpecialChars: bool
  ExpiryDays: int
  ReusePreventionCount: int

AuthProvider: enum { Password, Oidc, Saml, Ldap }
MfaProvider: enum { Totp, Sms, Email }
```

#### Domain Events

```
UserCreatedEvent         { UserId, TenantId, Username, Email, OccurredAt }
UserDeactivatedEvent     { UserId, TenantId, OccurredAt }
UserLoggedInEvent        { UserId, TenantId, IpAddress, OccurredAt }
RoleCreatedEvent         { RoleId, TenantId, Name, OccurredAt }
PasswordChangedEvent     { UserId, TenantId, OccurredAt }
MfaEnabledEvent          { UserId, TenantId, Provider, OccurredAt }
```

#### Domain Services

```
IAuthenticationService:
  AuthenticateAsync(string username, string password, Guid tenantId): AuthResult
  ValidateTokenAsync(string token): TokenValidationResult
  RefreshTokenAsync(string refreshToken): AuthResult
  InitiatePasswordResetAsync(string email, Guid tenantId)
  ResetPasswordAsync(string token, string newPassword)

IPasswordService:
  HashPassword(string password): string
  VerifyPassword(string password, string hash): bool
  ValidateAgainstPolicy(string password, PasswordPolicy policy): ValidationResult

IRoleService:
  GetEffectivePermissions(User user): List<string>
  CloneRole(Role source, Guid targetTenantId): Role
```

#### Repository Interfaces

```
ITenantRepository:
  GetByIdAsync(Guid id): Tenant?
  GetBySlugAsync(string slug): Tenant?
  AddAsync(Tenant tenant)
  UpdateAsync(Tenant tenant)

IUserRepository:
  GetByIdAsync(Guid id): User?
  GetByUsernameAsync(string username, Guid tenantId): User?
  GetByEmailAsync(string email, Guid tenantId): User?
  GetPagedAsync(Guid tenantId, int page, int size): PagedResult<User>
  AddAsync(User user)
  UpdateAsync(User user)
  CountByTenantAsync(Guid tenantId): int

IRoleRepository:
  GetByIdAsync(Guid id): Role?
  GetByNameAsync(string name, Guid tenantId): Role?
  ListByTenantAsync(Guid tenantId): List<Role>
  AddAsync(Role role)
  UpdateAsync(Role role)
  DeleteAsync(Role role)
```

#### Domain Rules / Invariants

- Username must be unique within a tenant
- Email must be unique within a tenant
- System roles (Admin, SuperAdmin) cannot be deleted
- A tenant must have at least one admin user
- Password must satisfy the tenant's PasswordPolicy
- Cannot deactivate the last admin user in a tenant

### 1.2 Application Layer

#### Commands

```
RegisterTenantCommand          { Name, Slug, AdminEmail, AdminUsername, AdminPassword }
ActivateTenantCommand          { TenantId }
DeactivateTenantCommand        { TenantId }
UpdateTenantSettingsCommand    { TenantId, Settings }
CreateUserCommand              { TenantId, Username, Email, FirstName, LastName, Password, Roles }
UpdateUserCommand              { UserId, FirstName, LastName, PhoneNumber }
DeactivateUserCommand          { UserId }
AssignRoleCommand              { UserId, RoleId }
RemoveRoleCommand              { UserId, RoleId }
CreateRoleCommand              { TenantId, Name, Description, Permissions }
UpdateRoleCommand              { RoleId, Name, Description }
DeleteRoleCommand              { RoleId }
LoginCommand                   { TenantId, Username, Password, IpAddress }
RefreshTokenCommand            { RefreshToken }
ChangePasswordCommand          { UserId, CurrentPassword, NewPassword }
InitiatePasswordResetCommand   { Email, TenantId }
ResetPasswordCommand           { Token, NewPassword }
```

#### Queries

```
GetTenantQuery                { TenantId }
GetTenantBySlugQuery          { Slug }
GetUserQuery                  { UserId }
ListTenantUsersQuery          { TenantId, Page, Size, Search? }
ListTenantRolesQuery          { TenantId }
GetUserPermissionsQuery       { UserId }
SearchUsersQuery              { TenantId, SearchTerm, Page, Size }
GetSessionInfoQuery           { Token }
```

#### Command Handlers

```
RegisterTenantHandler         -> Creates tenant, admin role, admin user
ActivateTenantHandler         -> Activates tenant
DeactivateTenantHandler       -> Deactivates tenant (publishes TenantDeactivatedEvent)
CreateUserHandler             -> Validates uniqueness, creates user, publishes UserCreatedEvent
UpdateUserHandler             -> Updates profile
DeactivateUserHandler         -> Validates last-admin rule, deactivates
AssignRoleHandler             -> Assigns role
RemoveRoleHandler             -> Removes role
LoginHandler                  -> Validates credentials, publishes UserLoggedInEvent
ChangePasswordHandler         -> Validates old password, policy, publishes PasswordChangedEvent
CreateRoleHandler             -> Creates role
DeleteRoleHandler             -> Validates non-system
```

#### Query Handlers

```
GetTenantHandler              -> Returns TenantDto
GetUserHandler                -> Returns UserDto
ListTenantUsersHandler        -> Returns PagedResult<UserDto>
ListTenantRolesHandler        -> Returns List<RoleDto>
GetUserPermissionsHandler     -> Returns List<string>
```

#### Application Services

```
ITokenService:
  GenerateAccessToken(User user, Tenant tenant): string
  GenerateRefreshToken(): string
  ValidateToken(string token): ClaimsPrincipal?
  InvalidateRefreshToken(string refreshToken)

ISessionService:
  CreateSessionAsync(User user, Tenant tenant, string ipAddress): SessionResult
  GetSessionAsync(string token): SessionInfo?
  RevokeSessionAsync(string token)

IKeycloakSyncService:
  SyncUserToKeycloakAsync(User user, Tenant tenant)
  SyncRoleToKeycloakAsync(Role role, Tenant tenant)
  RemoveUserFromKeycloakAsync(User user)
```

#### DTOs

```
TenantDto           { Id, Name, Slug, LogoUrl, IsActive, Settings, CreatedAt }
UserDto             { Id, TenantId, Username, Email, FirstName, LastName, PhoneNumber, IsActive, EmailVerified, Roles, CreatedAt }
RoleDto             { Id, TenantId, Name, Description, IsSystem, Permissions, UserCount }
LoginResponseDto    { AccessToken, RefreshToken, ExpiresIn, TokenType, User }
AuthTokenDto        { Token, RefreshToken, ExpiresAt }
PermissionDto       { Name, Description, Category, IsSystem }
SessionInfoDto      { SessionId, UserId, TenantId, IpAddress, CreatedAt, ExpiresAt, IsActive }
```

#### Mapping Profiles

```
AutoMapper profiles:
  User -> UserDto
  Role -> RoleDto
  Tenant -> TenantDto
  UserCreateCommand -> User
  RoleCreateCommand -> Role
```

### 1.3 Infrastructure Layer

#### EF Core Entity Configurations

```
UserConfiguration:
  Table: "iam_users"
  HasIndex(u => new { u.TenantId, u.Username }).IsUnique()
  HasIndex(u => new { u.TenantId, u.Email }).IsUnique()
  HasIndex(u => u.TenantId)
  Property(u => u.Username).HasMaxLength(100)
  Property(u => u.Email).HasMaxLength(256)
  Property(u => u.Attributes).HasColumnType("jsonb")

RoleConfiguration:
  Table: "iam_roles"
  HasIndex(r => new { r.TenantId, r.Name }).IsUnique()
  Property(r => r.Permissions).HasColumnType("jsonb")

TenantConfiguration:
  Table: "iam_tenants"
  HasIndex(t => t.Slug).IsUnique()
  OwnsOne(t => t.Settings)

UserRoleConfiguration:
  Table: "iam_user_roles"
  HasKey(ur => new { ur.UserId, ur.RoleId })
```

#### Repository Implementations

```
TenantRepository     -> EF Core implementation of ITenantRepository
UserRepository       -> EF Core implementation of IUserRepository, includes role eager loading
RoleRepository       -> EF Core implementation of IRoleRepository
```

#### Integration Events

**Publishes:**
```
UserCreatedEvent          -> Topic: "iam.user.created"
UserDeactivatedEvent      -> Topic: "iam.user.deactivated"
UserLoggedInEvent         -> Topic: "iam.user.logged_in"
TenantCreatedEvent        -> Topic: "iam.tenant.created"
TenantDeactivatedEvent    -> Topic: "iam.tenant.deactivated"
RoleCreatedEvent          -> Topic: "iam.role.created"
RoleDeletedEvent          -> Topic: "iam.role.deleted"
```

**Subscribes to:** None

#### Background Jobs

```
TokenCleanupJob:
  Schedule: Every 15 minutes
  Deletes expired refresh tokens
  Logs out stale sessions
```

#### Caching Strategy

```
Cache: Tenants by slug (Redis, 1 hour, evict on update)
Cache: User permissions (Redis, 15 minutes, evict on role change)
Cache: Roles by tenant (Redis, 30 minutes, evict on role change)
```

#### External Service Integrations

```
Keycloak Admin API: User/role CRUD, authentication, token validation
  - Create realm per tenant
  - Create/update/delete users per realm
  - Create/update/delete roles per realm
  - Validate tokens via JWKS endpoint
```

### 1.4 API Layer

#### Endpoints

```
POST   /api/v1/tenants                          -> RegisterTenant          [Public]
GET    /api/v1/tenants/{id}                     -> GetTenant               [Admin]
GET    /api/v1/tenants/by-slug/{slug}           -> GetTenantBySlug         [Public]
PUT    /api/v1/tenants/{id}/settings            -> UpdateTenantSettings    [Admin]
POST   /api/v1/tenants/{id}/activate            -> ActivateTenant          [Admin]
POST   /api/v1/tenants/{id}/deactivate          -> DeactivateTenant        [Admin]

POST   /api/v1/auth/login                       -> Login                   [Public]
POST   /api/v1/auth/refresh                     -> RefreshToken            [Public]
POST   /api/v1/auth/change-password             -> ChangePassword          [Authenticated]
POST   /api/v1/auth/forgot-password             -> InitiatePasswordReset   [Public]
POST   /api/v1/auth/reset-password              -> ResetPassword           [Public]

GET    /api/v1/users                            -> ListTenantUsers         [Admin, User.Read]
GET    /api/v1/users/{id}                       -> GetUser                 [Admin, User.Read]
POST   /api/v1/users                            -> CreateUser              [Admin, User.Create]
PUT    /api/v1/users/{id}                       -> UpdateUser              [Admin, User.Update]
DELETE /api/v1/users/{id}                       -> DeactivateUser          [Admin, User.Delete]
POST   /api/v1/users/{id}/roles                 -> AssignRole              [Admin, User.ManageRoles]
DELETE /api/v1/users/{id}/roles/{roleId}        -> RemoveRole              [Admin, User.ManageRoles]

GET    /api/v1/roles                            -> ListTenantRoles         [Admin, Role.Read]
POST   /api/v1/roles                            -> CreateRole              [Admin, Role.Create]
PUT    /api/v1/roles/{id}                       -> UpdateRole              [Admin, Role.Update]
DELETE /api/v1/roles/{id}                       -> DeleteRole              [Admin, Role.Delete]
GET    /api/v1/users/{id}/permissions            -> GetUserPermissions      [Authenticated]
```

#### Request/Response Models

```
LoginRequest         { TenantSlug, Username, Password }
LoginResponse        { AccessToken, RefreshToken, ExpiresIn, User }
CreateUserRequest    { Username, Email, FirstName, LastName, Password, RoleIds }
CreateRoleRequest    { Name, Description, Permissions }
UpdateRoleRequest    { Name, Description }
ChangePasswordRequest { CurrentPassword, NewPassword }
ForgotPasswordRequest { Email, TenantSlug }
ResetPasswordRequest  { Token, NewPassword }
```

#### Authorization Requirements

```
Public endpoints: RegisterTenant, Login, RefreshToken, ForgotPassword, ResetPassword
Authenticated: All user-specific operations
Admin: Tenant management, user administration, role management
Fine-grained: Additional permission checks on specific operations
```

#### Rate Limiting

```
Login: 10 requests/minute per IP, 5 per minute per user after 3 failures
Public tenant endpoints: 30 requests/minute per IP
Admin endpoints: 100 requests/minute per user
Password reset: 3 requests/hour per email
```

#### Versioning Strategy

```
URL-based versioning: /api/v1/
Breaking changes -> new major version (v2)
Non-breaking changes -> additive within existing version
```

### 1.5 Events

#### Published Events

| Event | Topic | Schema |
|-------|-------|--------|
| UserCreated | `iam.user.created` | `{ UserId, TenantId, Username, Email, FirstName, LastName, Roles, OccurredAt }` |
| UserDeactivated | `iam.user.deactivated` | `{ UserId, TenantId, OccurredAt }` |
| UserLoggedIn | `iam.user.logged_in` | `{ UserId, TenantId, IpAddress, Timestamp }` |
| TenantCreated | `iam.tenant.created` | `{ TenantId, Name, Slug, OccurredAt }` |
| TenantDeactivated | `iam.tenant.deactivated` | `{ TenantId, OccurredAt }` |
| RoleCreated | `iam.role.created` | `{ RoleId, TenantId, Name, Permissions, OccurredAt }` |

#### Subscribed Events

None

### 1.6 Data Ownership

#### Owned Tables

| Table | Type | Purpose |
|-------|------|---------|
| `iam_tenants` | Primary | Tenant definitions |
| `iam_users` | Primary | User accounts |
| `iam_roles` | Primary | Role definitions |
| `iam_user_roles` | Join | Many-to-many user-role mapping |
| `iam_refresh_tokens` | Secondary | Refresh token storage |
| `iam_password_resets` | Secondary | Password reset tokens |
| `iam_login_attempts` | Secondary | Failed login tracking |

#### Foreign Key Relationships

```
iam_users.tenant_id -> iam_tenants.id (CASCADE)
iam_roles.tenant_id -> iam_tenants.id (CASCADE)
iam_user_roles.user_id -> iam_users.id (CASCADE)
iam_user_roles.role_id -> iam_roles.id (CASCADE)
```

#### Indexing Strategy

```
iam_users: (tenant_id, username) UNIQUE, (tenant_id, email) UNIQUE, tenant_id
iam_roles: (tenant_id, name) UNIQUE
iam_tenants: slug UNIQUE
iam_refresh_tokens: user_id, expires_at, token (UNIQUE)
iam_login_attempts: (tenant_id, username, attempted_at)
iam_password_resets: email, token (UNIQUE), expires_at
```

### 1.7 Dependencies

#### Depends On

None (foundational module)

#### Depended On By

All modules (tenant context, user identity)

### 1.8 Security Requirements

#### RBAC Roles & Permissions

```
System-level:
  SuperAdmin: Full access across all tenants
  TenantAdmin: Full access within tenant

Module-level permissions:
  User.Read, User.Create, User.Update, User.Delete
  User.ManageRoles
  Role.Read, Role.Create, Role.Update, Role.Delete
  Tenant.Read, Tenant.Update, Tenant.Manage
  Audit.Read
```

#### Data Isolation Rules

- Tenants are fully isolated -- users in Tenant A cannot access Tenant B data
- All queries filter by TenantId (multi-tenant by discriminator)
- System roles are seeded per tenant: Admin, BillingAdmin, SupportAgent, ReadOnly

#### Audit Requirements

- All authentication events (login, logout, failure)
- All user CRUD operations
- All role/permission changes
- All tenant configuration changes
- Immutable audit log (append-only)

---

## 2. CRM — Customer Relationship Management

### 2.1 Domain Layer

#### Aggregate: Customer

```
Id: Guid
TenantId: Guid
ExternalId: string?
Type: CustomerType         (Individual, Business)
Status: CustomerStatus     (Active, Inactive, Suspended, Closed)
AccountManager: Guid?
SalesChannel: string?
AcquisitionDate: DateTime
ChurnRisk: ChurnRiskLevel  (Low, Medium, High)
Tags: List<string>
Attributes: Dictionary<string, string>
Source: string?
CreatedAt: DateTime
UpdatedAt: DateTime

Methods:
  UpdateStatus(CustomerStatus status)
  AssignAccountManager(Guid userId)
  AddTag(string tag)
  RemoveTag(string tag)
  UpdateAttribute(string key, string value)
  CalculateChurnRisk(): ChurnRiskLevel
  CanPlaceOrder(): bool
```

#### Aggregate: Individual

```
Id: Guid
CustomerId: Guid
FirstName: string
LastName: string
MiddleName: string?
DateOfBirth: DateOnly?
Gender: string?
NationalId: string?
TaxId: string?
Email: string
AlternativeEmail: string?
PhoneNumber: string
AlternativePhone: string?
PreferredLanguage: string
PreferredContactMethod: ContactMethod (Email, SMS, Phone, Mail)
MarketingConsent: bool
MarketingConsentDate: DateTime?
IdentityVerified: bool
IdentityVerifiedAt: DateTime?
UserId: Guid?

Methods:
  UpdatePersonalInfo(...)
  VerifyIdentity()
  UpdateMarketingConsent(bool consent)
```

#### Aggregate: Business

```
Id: Guid
CustomerId: Guid
CompanyName: string
RegistrationNumber: string
TaxId: string
VatId: string?
Industry: string?
CompanyType: string?
Website: string?
EmployeeCount: int?
AnnualRevenue: decimal?
IncorporationDate: DateOnly?
Contacts: List<BusinessContact>

Methods:
  UpdateBusinessInfo(...)
  AddContact(BusinessContact contact)
  RemoveContact(Guid contactId)
```

#### Entity: BusinessContact

```
Id: Guid
BusinessId: Guid
FirstName: string
LastName: string
JobTitle: string?
Email: string
PhoneNumber: string
IsPrimary: bool
IsDecisionMaker: bool
Department: string?
```

#### Entity: Address

```
Id: Guid
CustomerId: Guid
Type: AddressType          (Billing, Shipping, Service, Registered)
Line1: string
Line2: string?
City: string
State: string?
PostalCode: string
Country: string
IsDefault: bool
IsVerified: bool
Latitude: double?
Longitude: double?
```

#### Entity: CustomerInteraction

```
Id: Guid
CustomerId: Guid
Type: InteractionType      (Call, Email, Chat, Visit, Note, System)
Channel: string
Subject: string
Description: string
AgentId: Guid
DurationMinutes: int?
RelatedEntityType: string?
RelatedEntityId: Guid?
Outcome: string?
CreatedAt: DateTime
```

#### Entity: CustomerSegment

```
Id: Guid
TenantId: Guid
Name: string
Description: string
Criteria: SegmentCriteria  (json)
IsDynamic: bool
CustomerCount: int
CreatedAt: DateTime
UpdatedAt: DateTime
```

#### Value Objects

```
CustomerType: enum { Individual, Business }
CustomerStatus: enum { Prospective, Active, Inactive, Suspended, Closed }
ContactMethod: enum { Email, SMS, Phone, Mail, Portal }
AddressType: enum { Billing, Shipping, Service, Registered, Postal }
InteractionType: enum { Call, Email, Chat, Visit, Portal, Note, System, Social }
ChurnRiskLevel: enum { Low, Medium, High }
SegmentCriteria: Dictionary<string, object>
```

#### Domain Events

```
CustomerCreatedEvent             { CustomerId, TenantId, Type, OccurredAt }
CustomerStatusChangedEvent       { CustomerId, OldStatus, NewStatus, Reason, OccurredAt }
CustomerChurnRiskUpdatedEvent    { CustomerId, PreviousRisk, NewRisk, OccurredAt }
CustomerAssignedToManagerEvent   { CustomerId, ManagerId, OccurredAt }
CustomerMergedEvent              { PrimaryCustomerId, SecondaryCustomerId, OccurredAt }
InteractionRecordedEvent         { CustomerId, InteractionType, AgentId, OccurredAt }
CustomerSegmentChangedEvent      { CustomerId, OldSegment, NewSegment, OccurredAt }
IndividualVerifiedEvent          { CustomerId, IndividualId, VerifiedAt, OccurredAt }
```

#### Domain Services

```
ICustomerMatchingService:
  FindPotentialDuplicates(Customer customer): List<CustomerMatch>
  MergeCustomers(Customer primary, Customer secondary): Customer
  SuggestMerge(Guid customerId): List<Guid>

ISegmentationService:
  EvaluateCriteria(Customer customer, SegmentCriteria criteria): bool
  RebuildSegment(Guid segmentId): int
  GetAllMatchingSegments(Customer customer): List<CustomerSegment>
```

#### Repository Interfaces

```
ICustomerRepository:
  GetByIdAsync(Guid id): Customer?
  GetByUserIdAsync(Guid userId, Guid tenantId): Customer?
  GetByEmailAsync(string email, Guid tenantId): Customer?
  GetPagedAsync(Guid tenantId, CustomerFilter filter): PagedResult<Customer>
  AddAsync(Customer customer)
  UpdateAsync(Customer customer)
  FindDuplicatesAsync(Customer customer): List<Customer>
  CountByStatusAsync(Guid tenantId, CustomerStatus status): int

IIndividualRepository:
  GetByIdAsync(Guid id): Individual?
  GetByCustomerIdAsync(Guid customerId): Individual
  AddAsync(Individual individual)
  UpdateAsync(Individual individual)

IBusinessRepository:
  GetByIdAsync(Guid id): Business?
  GetByCustomerIdAsync(Guid customerId): Business
  AddAsync(Business business)
  UpdateAsync(Business business)

IAddressRepository:
  GetByCustomerIdAsync(Guid customerId): List<Address>
  AddAsync(Address address)
  UpdateAsync(Address address)
  DeleteAsync(Address address)

ICustomerInteractionRepository:
  GetByCustomerIdAsync(Guid customerId, int page, int size): PagedResult<CustomerInteraction>
  AddAsync(CustomerInteraction interaction)
  GetRecentByCustomerAsync(Guid customerId, int count): List<CustomerInteraction>

ICustomerSegmentRepository:
  GetByIdAsync(Guid id): CustomerSegment?
  ListByTenantAsync(Guid tenantId): List<CustomerSegment>
  AddAsync(CustomerSegment segment)
  UpdateAsync(CustomerSegment segment)
  DeleteAsync(CustomerSegment segment)
  GetForCustomerAsync(Guid customerId): List<CustomerSegment>
```

#### Domain Rules / Invariants

- Customer must have at least one address (billing or service)
- Individual customers must have email or phone
- Business customers must have at least one contact
- Email must be unique per tenant
- Cannot close a customer with active subscriptions
- A customer in Suspended status cannot be reactivated directly -- requires approval
- Segment names must be unique per tenant
- Cannot delete a customer referenced by orders/invoices (soft-delete only)

### 2.2 Application Layer

#### Commands

```
CreateIndividualCustomerCommand  { TenantId, FirstName, LastName, Email, Phone, DateOfBirth, NationalId, Address, MarketingConsent, Tags }
CreateBusinessCustomerCommand    { TenantId, CompanyName, RegistrationNumber, TaxId, Industry, Contacts, Address, Tags }
UpdateCustomerCommand            { CustomerId, Tags, Attributes, AccountManagerId }
UpdateIndividualCommand          { CustomerId, FirstName, LastName, Email, Phone, DateOfBirth }
UpdateBusinessCommand            { CustomerId, CompanyName, TaxId, Industry, EmployeeCount }
ChangeCustomerStatusCommand      { CustomerId, NewStatus, Reason }
AssignAccountManagerCommand      { CustomerId, ManagerId }
AddAddressCommand                { CustomerId, Address }
UpdateAddressCommand             { AddressId, Address }
DeleteAddressCommand             { AddressId }
RecordInteractionCommand         { CustomerId, Type, Channel, Subject, Description, AgentId, Duration }
AddTagCommand                    { CustomerId, Tag }
RemoveTagCommand                 { CustomerId, Tag }
MergeCustomersCommand            { PrimaryCustomerId, SecondaryCustomerId }
CreateSegmentCommand             { TenantId, Name, Description, Criteria, IsDynamic }
UpdateSegmentCommand             { SegmentId, Name, Description, Criteria }
DeleteSegmentCommand             { SegmentId }
VerifyIdentityCommand            { CustomerId, IndividualId }
```

#### Queries

```
GetCustomerQuery                 { CustomerId }
SearchCustomersQuery             { TenantId, SearchTerm, Status, Type, SegmentId, Page, Size }
GetCustomerDetailsQuery          { CustomerId }
GetCustomerInteractionsQuery     { CustomerId, Page, Size }
GetCustomerAddressesQuery        { CustomerId }
GetCustomerSegmentsQuery         { CustomerId }
ListSegmentsQuery                { TenantId }
GetSegmentQuery                  { SegmentId }
GetPotentialDuplicatesQuery      { CustomerId }
GetCustomerAccountsQuery         { CustomerId }
GetCustomerSubscriptionsQuery    { CustomerId }
```

#### Command Handlers

```
CreateIndividualCustomerHandler  -> Validates uniqueness, creates Customer + Individual + Address, publishes CustomerCreatedEvent
CreateBusinessCustomerHandler    -> Validates uniqueness, creates Customer + Business + Contacts + Address, publishes CustomerCreatedEvent
UpdateCustomerHandler            -> Updates aggregate, raises events
UpdateIndividualHandler          -> Updates Individual
ChangeCustomerStatusHandler      -> Validates rules, sets status, publishes CustomerStatusChangedEvent
AssignAccountManagerHandler      -> Assigns manager, publishes event
MergeCustomersHandler            -> Validates, calls MergeCustomers service, publishes CustomerMergedEvent
RecordInteractionHandler         -> Creates interaction, publishes InteractionRecordedEvent
CreateSegmentHandler             -> Creates segment with criteria
DeleteSegmentHandler             -> Validates segment not referenced
VerifyIdentityHandler            -> Validates identity, updates Individual
```

#### Query Handlers

```
GetCustomerHandler               -> Returns CustomerDetailDto
SearchCustomersHandler           -> Returns PagedResult<CustomerSummaryDto> with search/filter
GetCustomerInteractionsHandler   -> Returns PagedResult<InteractionDto>
GetCustomerAddressesHandler      -> Returns List<AddressDto>
```

#### Application Services

```
ICustomerSearchService:
  SearchAsync(Guid tenantId, CustomerSearchCriteria criteria): PagedResult<CustomerSummaryDto>

ICustomerImportService:
  ImportFromCsvAsync(Stream csv, Guid tenantId): ImportResult
  ValidateImportRow(Dictionary<string, string> row): ValidationResult

IDuplicateDetectionService:
  FindDuplicatesAsync(Guid tenantId, Customer customer): List<DuplicateGroup>
  ResolveDuplicatesAsync(Guid primaryId, List<Guid> duplicateIds): Customer
```

#### DTOs

```
CustomerSummaryDto     { Id, Type, FullName/CompanyName, Email, Phone, Status, Segment, AccountManager, CreatedAt, HasActiveSubscriptions }
CustomerDetailDto      { Id, Type, Status, Tags, Attributes, Individual/Business, Addresses, RecentInteractions, AccountManager, ChurnRisk, CreatedAt }
IndividualDto          { FirstName, LastName, DateOfBirth, NationalId, Email, Phone, MarketingConsent, IdentityVerified }
BusinessDto            { CompanyName, RegistrationNumber, TaxId, Industry, Contacts, EmployeeCount }
AddressDto             { Id, Type, Line1, Line2, City, State, PostalCode, Country, IsDefault }
InteractionDto         { Id, Type, Channel, Subject, Description, AgentName, Duration, CreatedAt }
CustomerSegmentDto     { Id, Name, Description, Criteria, IsDynamic, CustomerCount }
BusinessContactDto     { Id, FirstName, LastName, JobTitle, Email, Phone, IsPrimary }
DuplicateGroupDto      { GroupId, CustomerIds, MatchScore, MatchReasons }
```

#### Mapping Profiles

```
CustomerSummary -> CustomerSummaryDto
Customer -> CustomerDetailDto
Individual -> IndividualDto
Business -> BusinessDto
Address -> AddressDto
CustomerInteraction -> InteractionDto
CustomerSegment -> CustomerSegmentDto
BusinessContact -> BusinessContactDto
```

### 2.3 Infrastructure Layer

#### EF Core Entity Configurations

```
CustomerConfiguration:
  Table: "crm_customers"
  HasIndex(c => c.TenantId)
  HasIndex(c => new { c.TenantId, c.Type })
  Property(c => c.Tags).HasColumnType("jsonb")
  Property(c => c.Attributes).HasColumnType("jsonb")
  HasOne(c => c.Individual).WithOne().HasForeignKey<Individual>(i => i.CustomerId)
  HasOne(c => c.Business).WithOne().HasForeignKey<Business>(b => b.CustomerId)

IndividualConfiguration:
  Table: "crm_individuals"
  HasIndex(i => i.Email).HasFilter("email IS NOT NULL")
  HasIndex(i => i.NationalId).HasFilter("national_id IS NOT NULL")

BusinessConfiguration:
  Table: "crm_businesses"
  HasIndex(b => b.RegistrationNumber)
  HasIndex(b => b.TaxId)

AddressConfiguration:
  Table: "crm_addresses"
  HasIndex(a => new { a.CustomerId, a.Type })

CustomerInteractionConfiguration:
  Table: "crm_interactions"
  HasIndex(i => i.CustomerId)
  HasIndex(i => i.CreatedAt)
  Property(i => i.Description).HasColumnType("text")

CustomerSegmentConfiguration:
  Table: "crm_segments"
  HasIndex(s => new { s.TenantId, s.Name }).IsUnique()
  Property(s => s.Criteria).HasColumnType("jsonb")
```

#### Repository Implementations

```
CustomerRepository       -> EF Core implementation with filtering, pagination, includes
IndividualRepository     -> EF Core implementation
BusinessRepository       -> EF Core implementation
AddressRepository        -> EF Core implementation
InteractionRepository    -> EF Core implementation with pagination
SegmentRepository        -> EF Core implementation
```

#### Integration Events

**Publishes:**
```
CustomerCreatedEvent            -> Topic: "crm.customer.created"
CustomerStatusChangedEvent      -> Topic: "crm.customer.status_changed"
CustomerMergedEvent             -> Topic: "crm.customer.merged"
CustomerChurnRiskUpdatedEvent   -> Topic: "crm.customer.churn_risk_updated"
InteractionRecordedEvent        -> Topic: "crm.interaction.recorded"
IndividualVerifiedEvent         -> Topic: "crm.individual.verified"
```

**Subscribes to:**
```
iam.user.created -> Creates individual customer if user has customer role
orders.order.completed -> Updates customer status, records interaction
billing.invoice.generated -> Records interaction
billing.payment.received -> Updates customer payment history
collections.debt.status_changed -> Updates customer status
```

#### Background Jobs

```
SegmentRebuildJob:
  Schedule: Daily
  Re-evaluates all dynamic segments
  Updates customer-segment membership

ChurnRiskCalculationJob:
  Schedule: Weekly
  Re-evaluates churn risk for all active customers

DataQualityJob:
  Schedule: Monthly
  Identifies incomplete profiles, flags potential duplicates

InteractionRetentionJob:
  Schedule: Weekly
  Archives interactions older than 3 years
```

#### Caching Strategy

```
Cache: Customer by ID (Redis, 30 minutes, evict on update)
Cache: Customer segments (Redis, 1 hour, evict on segment change)
Cache: Active customer count by tenant (Redis, 1 hour)
Cache: Customer search results (Redis, 5 minutes, evict on relevant changes)
```

#### External Service Integrations

```
Identity Verification Service (Onfido, Jumio):
  - Verify identity document
  - Check against watchlists

Credit Check Service (Experian, Equifax):
  - Credit check for business customers
  - Credit score lookup

CRM Import/Export:
  - CSV/Excel import
  - API-based sync with external CRM (Salesforce, HubSpot)
```

### 2.4 API Layer

#### Endpoints

```
GET    /api/v1/customers                           -> SearchCustomers          [Authenticated, Customer.Read]
POST   /api/v1/customers/individual                -> CreateIndividualCustomer [Authenticated, Customer.Create]
POST   /api/v1/customers/business                  -> CreateBusinessCustomer   [Authenticated, Customer.Create]
GET    /api/v1/customers/{id}                      -> GetCustomer              [Authenticated, Customer.Read]
PUT    /api/v1/customers/{id}                      -> UpdateCustomer           [Authenticated, Customer.Update]
PATCH  /api/v1/customers/{id}/status               -> ChangeCustomerStatus     [Authenticated, Customer.Update]
POST   /api/v1/customers/{id}/manager              -> AssignAccountManager     [Authenticated, Customer.Update]
POST   /api/v1/customers/{id}/merge                -> MergeCustomers           [Admin, Customer.Admin]
POST   /api/v1/customers/{id}/verify               -> VerifyIdentity           [Authenticated, Customer.Update]
POST   /api/v1/customers/{id}/tags/{tag}           -> AddTag                   [Authenticated, Customer.Update]
DELETE /api/v1/customers/{id}/tags/{tag}            -> RemoveTag                [Authenticated, Customer.Update]
GET    /api/v1/customers/{id}/details              -> GetCustomerDetails       [Authenticated, Customer.Read]
GET    /api/v1/customers/{id}/interactions         -> GetCustomerInteractions  [Authenticated, Customer.Read]
POST   /api/v1/customers/{id}/interactions         -> RecordInteraction        [Authenticated, Customer.Create]
GET    /api/v1/customers/{id}/addresses            -> GetCustomerAddresses     [Authenticated, Customer.Read]
POST   /api/v1/customers/{id}/addresses            -> AddAddress               [Authenticated, Customer.Update]
PUT    /api/v1/customers/{id}/addresses/{addrId}   -> UpdateAddress            [Authenticated, Customer.Update]
DELETE /api/v1/customers/{id}/addresses/{addrId}   -> DeleteAddress            [Authenticated, Customer.Update]
GET    /api/v1/customers/{id}/duplicates           -> GetPotentialDuplicates   [Admin, Customer.Admin]
GET    /api/v1/customers/{id}/segments             -> GetCustomerSegments      [Authenticated, Customer.Read]
GET    /api/v1/customers/{id}/accounts             -> GetCustomerAccounts      [Authenticated, Customer.Read]
GET    /api/v1/customers/{id}/subscriptions        -> GetCustomerSubscriptions [Authenticated, Customer.Read]

GET    /api/v1/segments                            -> ListSegments             [Admin, Segment.Read]
POST   /api/v1/segments                            -> CreateSegment            [Admin, Segment.Create]
GET    /api/v1/segments/{id}                       -> GetSegment               [Admin, Segment.Read]
PUT    /api/v1/segments/{id}                       -> UpdateSegment            [Admin, Segment.Update]
DELETE /api/v1/segments/{id}                       -> DeleteSegment            [Admin, Segment.Delete]
```

#### Request/Response Models

```
CreateIndividualRequest  { FirstName, LastName, Email, Phone, DateOfBirth, NationalId, Address, MarketingConsent, Tags }
CreateBusinessRequest    { CompanyName, RegistrationNumber, TaxId, Industry, Contacts[], Address, Tags }
UpdateCustomerRequest    { Tags, Attributes }
ChangeStatusRequest      { NewStatus, Reason }
RecordInteractionRequest { Type, Channel, Subject, Description, Duration }
AddressRequest           { Type, Line1, Line2, City, State, PostalCode, Country, IsDefault }
CreateSegmentRequest     { Name, Description, Criteria, IsDynamic }
UpdateSegmentRequest     { Name, Description, Criteria }
MergeCustomersRequest    { PrimaryCustomerId, SecondaryCustomerIds[] }
```

#### Authorization Requirements

```
Customer.Read: View customer data
Customer.Create: Create new customers
Customer.Update: Modify customer data
Customer.Delete: Soft-delete customers
Customer.Admin: Merge customers, view duplicates, administrative operations
Segment.Read: View segments
Segment.Create: Create segments
Segment.Update: Modify segments
Segment.Delete: Delete segments
```

#### Rate Limiting

```
Customer search: 30 requests/minute
Customer creation: 10 requests/minute per user
Customer import: 1 request/minute
Interaction recording: 60 requests/minute
Segment operations: 20 requests/minute
```

#### Versioning Strategy

```
URL-based: /api/v1/customers, /api/v1/segments
```

### 2.5 Events

#### Published Events

| Event | Topic | Schema |
|-------|-------|--------|
| CustomerCreated | `crm.customer.created` | `{ CustomerId, TenantId, Type, Email, Phone, Status, OccurredAt }` |
| CustomerStatusChanged | `crm.customer.status_changed` | `{ CustomerId, TenantId, OldStatus, NewStatus, Reason, OccurredAt }` |
| CustomerMerged | `crm.customer.merged` | `{ PrimaryCustomerId, SecondaryCustomerId, TenantId, OccurredAt }` |
| CustomerChurnRiskUpdated | `crm.customer.churn_risk_updated` | `{ CustomerId, TenantId, PreviousRisk, NewRisk, Score, OccurredAt }` |
| InteractionRecorded | `crm.interaction.recorded` | `{ CustomerId, TenantId, InteractionId, Type, Channel, AgentId, OccurredAt }` |
| IndividualVerified | `crm.individual.verified` | `{ CustomerId, IndividualId, TenantId, VerifiedAt, OccurredAt }` |

#### Subscribed Events

| Event | Source | Action |
|-------|--------|--------|
| `iam.user.created` | IAM | Create Individual record linked to user |
| `orders.order.completed` | Orders | Update customer last order date, record interaction |
| `billing.invoice.generated` | Billing | Record automated interaction for invoice |
| `billing.payment.received` | Payments | Update payment history, reduce churn risk |
| `collections.debt.status_changed` | Collections | Flag customer for churn, update status if needed |

### 2.6 Data Ownership

#### Owned Tables

| Table | Type | Purpose |
|-------|------|---------|
| `crm_customers` | Primary | Customer records (both Individual and Business) |
| `crm_individuals` | Primary | Individual-specific data |
| `crm_businesses` | Primary | Business-specific data |
| `crm_business_contacts` | Secondary | Business contact people |
| `crm_addresses` | Primary | Customer addresses |
| `crm_interactions` | Secondary | Customer interaction history |
| `crm_segments` | Primary | Segment definitions |
| `crm_customer_segments` | Join | Customer-segment membership |
| `crm_duplicate_groups` | Secondary | Duplicate customer groups |

#### Foreign Key Relationships

```
crm_customers.tenant_id -> iam_tenants.id (RESTRICT)
crm_individuals.customer_id -> crm_customers.id (CASCADE)
crm_businesses.customer_id -> crm_customers.id (CASCADE)
crm_business_contacts.business_id -> crm_businesses.id (CASCADE)
crm_addresses.customer_id -> crm_customers.id (CASCADE)
crm_interactions.customer_id -> crm_customers.id (CASCADE)
crm_segments.tenant_id -> iam_tenants.id (RESTRICT)
crm_customer_segments.customer_id -> crm_customers.id (CASCADE)
crm_customer_segments.segment_id -> crm_segments.id (CASCADE)
```

#### Indexing Strategy

```
crm_customers: (tenant_id, type), (tenant_id, status), (tenant_id, created_at), (tenant_id, account_manager_id)
crm_individuals: email (UNIQUE), (first_name, last_name), national_id, user_id
crm_businesses: registration_number, tax_id, company_name (trigram index)
crm_addresses: (customer_id, type), (country, city)
crm_interactions: (customer_id, created_at DESC), (agent_id, created_at), type
crm_segments: (tenant_id, name) UNIQUE
crm_customer_segments: (customer_id, segment_id) UNIQUE
```

### 2.7 Dependencies

#### Depends On

| Module | Reason |
|--------|--------|
| IAM | Tenant context, user references for account managers |

#### Depended On By

| Module | Reason |
|--------|--------|
| Orders | Customer reference on orders |
| Subscriptions | Customer reference on subscriptions |
| Billing | Customer for invoice generation |
| Collections | Customer for debt collection |
| Ticketing | Customer reference on tickets |
| Reporting | Customer analytics |
| Notifications | Customer contact info for sending notifications |

### 2.8 Security Requirements

#### RBAC Roles & Permissions

```
Module-level:
  Customer.Read        -- View customer profiles, addresses, interactions
  Customer.Create      -- Create new customers (individual and business)
  Customer.Update      -- Update customer info, add addresses, record interactions
  Customer.Delete      -- Soft-delete customers
  Customer.Admin       -- Merge customers, manage duplicates, override status
  Segment.Read         -- View segments and membership
  Segment.Create       -- Create segments
  Segment.Update       -- Modify segment criteria
  Segment.Delete       -- Remove segments

Role defaults:
  Admin: All permissions
  SupportAgent: Customer.Read, Customer.Create, Customer.Update, Segment.Read
  BillingAdmin: Customer.Read, Segment.Read
  ReadOnly: Customer.Read, Segment.Read
```

#### Data Isolation Rules

- All customer data is tenant-scoped (TenantId on every aggregate)
- Account managers can only see their assigned customers (configurable)
- Individual SSN/NationalId is encrypted at rest (AES-256 column encryption)
- Marketing consent must be explicitly tracked with timestamp
- Data retention: closed customers retained for 7 years (regulatory), then anonymized
- Address verification status tracked for compliance

#### Audit Requirements

- All customer creation and deletion events
- All status changes (with reason)
- All data merges (with both customer IDs)
- Identity verification events
- Address changes (with old/new values)
- Marketing consent changes
- Segment definition changes
- Access to PII logged separately

---

## 3. ProductCatalog — Product and Service Catalog

### 3.1 Domain Layer

#### Aggregate: ProductCategory

```
Id: Guid
TenantId: Guid
ParentId: Guid?
Name: string
Slug: string
Description: string?
DisplayOrder: int
IsActive: bool
ImageUrl: string?
Attributes: Dictionary<string, string>
CreatedAt: DateTime
UpdatedAt: DateTime

Methods:
  Activate()
  Deactivate()
  MoveTo(Guid? newParentId, int displayOrder)
  AddAttribute(string key, string value)
  HasChildren(): bool
```

#### Aggregate: Product

```
Id: Guid
TenantId: Guid
CategoryId: Guid
Name: string
Slug: string
Description: string?
ShortDescription: string?
Type: ProductType              (Physical, Digital, Service, Bundle)
Status: ProductStatus          (Draft, Active, Discontinued, Retired)
Version: int
IsTaxable: bool
TaxCategory: string?
Tags: List<string>
ImageUrls: List<string>
Attributes: Dictionary<string, object>
CustomFields: Dictionary<string, object>
ValidFrom: DateTime?
ValidTo: DateTime?
MaxQuantity: int?
MinQuantity: int
CreatedAt: DateTime
UpdatedAt: DateTime

Methods:
  Publish()
  Discontinue()
  Retire()
  UpdatePricing(List<ProductPrice> prices)
  AddVariant(ProductVariant variant)
  RemoveVariant(Guid variantId)
  AddCharacteristic(ProductCharacteristic characteristic)
  IsAvailable(): bool
  CanBeOrdered(int quantity): (bool, string?)
```

#### Entity: ProductPrice

```
Id: Guid
ProductId: Guid
PriceListId: Guid?
Name: string
Type: PriceType               (OneTime, Recurring, UsageBased, Tiered)
Currency: string
Amount: decimal
RecurringPeriod: RecurrencePeriod?
RecurringCount: int?
BillingCycle: int?
TaxIncluded: bool
Tiers: List<PriceTier>?
ValidFrom: DateTime?
ValidTo: DateTime?
MinQuantity: int?
MaxQuantity: int?

Methods:
  CalculatePrice(int quantity, Dictionary<string, object> usage): PriceCalculation
  IsApplicable(DateTime date, int quantity): bool
```

#### Value Object: PriceTier

```
From: int
To: int?
UnitPrice: decimal
FlatFee: decimal?
```

#### Entity: ProductVariant

```
Id: Guid
ProductId: Guid
Name: string
Sku: string
Attributes: Dictionary<string, string>
Price: ProductPrice
Status: ProductStatus
StockQuantity: int?
```

#### Entity: ProductCharacteristic

```
Id: Guid
ProductId: Guid
Name: string
Description: string?
DataType: string
IsRequired: bool
IsConfigurable: bool
MinValue: string?
MaxValue: string?
AllowedValues: List<string>?
DefaultValue: string?
Unit: string?
DisplayOrder: int
```

#### Entity: PriceList

```
Id: Guid
TenantId: Guid
Name: string
Description: string?
Currency: string
IsDefault: bool
ValidFrom: DateTime?
ValidTo: DateTime?
CustomerSegments: List<Guid>?
Priority: int
```

#### Value Objects

```
ProductType: enum { Physical, Digital, Service, Bundle, Resource }
ProductStatus: enum { Draft, Active, Discontinued, Retired }
PriceType: enum { OneTime, Recurring, UsageBased, Tiered, Dynamic }
RecurrencePeriod: enum { Day, Week, Month, Quarter, Year, OneTime }
```

#### Domain Events

```
ProductCreatedEvent          { ProductId, TenantId, CategoryId, Name, Type, OccurredAt }
ProductPublishedEvent       { ProductId, Version, OccurredAt }
ProductDiscontinuedEvent    { ProductId, OccurredAt }
ProductCategoryCreatedEvent { CategoryId, TenantId, Name, ParentId, OccurredAt }
ProductPriceChangedEvent    { ProductId, PriceId, OldAmount, NewAmount, OccurredAt }
ProductVariantAddedEvent    { ProductId, VariantId, Sku, OccurredAt }
ProductCharacteristicDefined { ProductId, CharacteristicId, Name, OccurredAt }
PriceListCreatedEvent       { PriceListId, TenantId, Name, Currency, OccurredAt }
```

#### Domain Services

```
IProductPricingService:
  CalculatePrice(Product product, int quantity, Guid? priceListId,
                 Dictionary<string, object> characteristics): PriceCalculation
  GetBestPrice(Product product, int quantity, Customer customer): PriceCalculation
  ValidatePriceConfiguration(ProductPrice price): ValidationResult

IProductValidationService:
  ValidateProductConfiguration(Product product, Dictionary<string, object> config): ValidationResult
  CheckAvailability(Product product, int quantity): AvailabilityResult
  ValidateBundleComposition(Product bundle, List<Guid> componentIds): ValidationResult
```

#### Repository Interfaces

```
IProductRepository:
  GetByIdAsync(Guid id): Product?
  GetBySlugAsync(string slug, Guid tenantId): Product?
  GetPagedAsync(Guid tenantId, ProductFilter filter): PagedResult<Product>
  GetByCategoryAsync(Guid categoryId): List<Product>
  AddAsync(Product product)
  UpdateAsync(Product product)
  GetActiveAsync(Guid tenantId): List<Product>
  SearchAsync(Guid tenantId, string searchTerm): List<Product>

ICategoryRepository:
  GetByIdAsync(Guid id): ProductCategory?
  GetBySlugAsync(string slug, Guid tenantId): ProductCategory?
  GetTreeAsync(Guid tenantId): List<ProductCategory> (hierarchical)
  GetChildrenAsync(Guid parentId): List<ProductCategory>
  AddAsync(ProductCategory category)
  UpdateAsync(ProductCategory category)
  DeleteAsync(ProductCategory category)

IPriceListRepository:
  GetByIdAsync(Guid id): PriceList?
  GetDefaultAsync(Guid tenantId): PriceList?
  ListByTenantAsync(Guid tenantId): List<PriceList>
  AddAsync(PriceList priceList)
  UpdateAsync(PriceList priceList)
  DeleteAsync(PriceList priceList)

IProductVariantRepository:
  GetBySkuAsync(string sku, Guid tenantId): ProductVariant?
  GetByProductIdAsync(Guid productId): List<ProductVariant>
  AddAsync(ProductVariant variant)
  UpdateAsync(ProductVariant variant)
```

#### Domain Rules / Invariants

- Product slug must be unique per tenant
- Category slug must be unique per tenant
- A product cannot reference itself or create circular hierarchy
- A category cannot be deleted if it has child categories or products
- Product variant SKU must be unique per tenant
- At least one price must be active for an active product
- Bundle must have at least 2 components
- Recurring prices must have a period defined
- Retired products cannot be ordered
- Draft products are not visible in catalog
- Price tiers must not overlap in quantity ranges

### 3.2 Application Layer

#### Commands

```
CreateCategoryCommand       { TenantId, Name, Slug, Description, ParentId, DisplayOrder, Attributes }
UpdateCategoryCommand       { CategoryId, Name, Description, DisplayOrder, Attributes }
DeleteCategoryCommand       { CategoryId }
MoveCategoryCommand         { CategoryId, NewParentId, DisplayOrder }

CreateProductCommand        { TenantId, CategoryId, Name, Slug, Description, Type, Tags, Attributes, Prices, Characteristics }
UpdateProductCommand        { ProductId, Name, Description, Tags, Attributes }
PublishProductCommand       { ProductId }
DiscontinueProductCommand   { ProductId }
RetireProductCommand        { ProductId }
AddProductPriceCommand      { ProductId, Price }
UpdateProductPriceCommand   { PriceId, Amount, ValidFrom, ValidTo }
RemoveProductPriceCommand   { PriceId }
AddProductVariantCommand    { ProductId, Name, Sku, Attributes, Price }
UpdateProductVariantCommand { VariantId, Name, Sku, Attributes }
RemoveProductVariantCommand { VariantId }
AddCharacteristicCommand    { ProductId, Name, DataType, IsRequired, IsConfigurable, AllowedValues }
UpdateCharacteristicCommand { CharacteristicId, Name, DataType, IsRequired }
RemoveCharacteristicCommand { CharacteristicId }

CreatePriceListCommand      { TenantId, Name, Description, Currency, IsDefault, Priority, CustomerSegments }
UpdatePriceListCommand      { PriceListId, Name, Description, Priority }
DeletePriceListCommand      { PriceListId }
SetDefaultPriceListCommand  { PriceListId }

CloneProductCommand         { ProductId, NewName, NewSlug }
BulkPublishCommand           { ProductIds[] }
```

#### Queries

```
GetCategoryTreeQuery        { TenantId }
GetCategoryQuery            { CategoryId }
GetProductQuery             { ProductId }
SearchProductsQuery         { TenantId, SearchTerm, CategoryId, Type, Status, Tags, Page, Size }
GetProductBySlugQuery       { Slug, TenantId }
GetProductVariantsQuery     { ProductId }
GetProductPricingQuery      { ProductId, PriceListId?, Quantity, Date }
GetPriceListQuery           { PriceListId }
ListPriceListsQuery         { TenantId }
GetProductsByCategoryQuery  { CategoryId, Page, Size }
CheckProductAvailabilityQuery { ProductId, Quantity, Date }
GetProductCharacteristicsQuery { ProductId }
```

#### Command Handlers

```
CreateCategoryHandler       -> Validates parent exists, creates category, publishes ProductCategoryCreatedEvent
DeleteCategoryHandler       -> Validates no children or products
CreateProductHandler        -> Validates category, creates product with prices and characteristics, publishes ProductCreatedEvent
PublishProductHandler       -> Validates completeness (prices, characteristics), publishes ProductPublishedEvent
AddProductPriceHandler      -> Adds price, publishes ProductPriceChangedEvent
AddProductVariantHandler    -> Validates SKU uniqueness, adds variant, publishes ProductVariantAddedEvent
CloneProductHandler         -> Deep-copies product with new slug, variant SKUs
```

#### Query Handlers

```
SearchProductsHandler       -> Returns PagedResult<ProductSummaryDto> with filters
GetProductHandler           -> Returns ProductDetailDto with prices, variants, characteristics
GetCategoryTreeHandler      -> Returns hierarchical tree of ProductCategoryTreeDto
GetProductPricingHandler    -> Returns calculated price with applicable discounts
CheckProductAvailabilityHandler -> Returns availability with estimated delivery date
```

#### Application Services

```
IProductSearchService:
  SearchAsync(ProductSearchCriteria criteria): PagedResult<ProductSummaryDto>
  FacetedSearchAsync(ProductSearchCriteria criteria): FacetedSearchResult

ICatalogImportService:
  ImportProductsAsync(Stream file, Guid tenantId): ImportResult
  ValidateImportTemplate(Stream template): ValidationResult
```

#### DTOs

```
ProductCategoryDto       { Id, TenantId, Name, Slug, Description, ParentId, DisplayOrder, ProductCount, ChildCount, IsActive, CreatedAt }
ProductCategoryTreeDto   { Id, Name, Slug, Children: List<ProductCategoryTreeDto>, ProductCount }
ProductSummaryDto        { Id, Name, Slug, Type, Status, CategoryName, MinPrice, MaxPrice, ImageUrl, Tags, CreatedAt }
ProductDetailDto         { Id, Name, Slug, Type, Status, Description, Category, Prices, Variants, Characteristics, Tags, Attributes, Version, CreatedAt }
ProductPriceDto          { Id, Name, Type, Currency, Amount, Period, PeriodCount, TaxIncluded, Tiers, ValidFrom, ValidTo }
ProductVariantDto        { Id, Name, Sku, Attributes, Price, Status }
ProductCharacteristicDto { Id, Name, DataType, IsRequired, IsConfigurable, AllowedValues, DefaultValue, DisplayOrder }
PriceListDto             { Id, Name, Description, Currency, IsDefault, Priority, CustomerSegmentIds, ProductCount }
PriceCalculationDto      { UnitPrice, Quantity, TotalAmount, Discounts, Taxes, GrandTotal, Currency, PriceBreakdown }
```

#### Mapping Profiles

```
ProductCategory -> ProductCategoryDto
Product -> ProductSummaryDto, ProductDetailDto
ProductPrice -> ProductPriceDto
ProductVariant -> ProductVariantDto
ProductCharacteristic -> ProductCharacteristicDto
PriceList -> PriceListDto
```

### 3.3 Infrastructure Layer

#### EF Core Entity Configurations

```
ProductConfiguration:
  Table: "catalog_products"
  HasIndex(p => new { p.TenantId, p.Slug }).IsUnique()
  HasIndex(p => new { p.TenantId, p.Status })
  HasIndex(p => p.CategoryId)
  HasIndex(p => p.Type)
  Property(p => p.Tags).HasColumnType("jsonb")
  Property(p => p.Attributes).HasColumnType("jsonb")
  Property(p => p.CustomFields).HasColumnType("jsonb")
  Property(p => p.ImageUrls).HasColumnType("jsonb")
  Property(p => p.Description).HasColumnType("text")
  HasMany(p => p.Prices).WithOne().HasForeignKey(pp => pp.ProductId)
  HasMany(p => p.Variants).WithOne().HasForeignKey(pv => pv.ProductId)
  HasMany(p => p.Characteristics).WithOne().HasForeignKey(pc => pc.ProductId)

ProductCategoryConfiguration:
  Table: "catalog_categories"
  HasIndex(c => new { c.TenantId, c.Slug }).IsUnique()
  HasOne(c => c.Parent).WithMany(c => c.Children).HasForeignKey(c => c.ParentId)
  Property(c => c.Attributes).HasColumnType("jsonb")

ProductPriceConfiguration:
  Table: "catalog_product_prices"
  HasIndex(pp => pp.ProductId)
  Property(pp => pp.Tiers).HasColumnType("jsonb")

ProductVariantConfiguration:
  Table: "catalog_product_variants"
  HasIndex(pv => pv.Sku).IsUnique()
  Property(pv => pv.Attributes).HasColumnType("jsonb")

ProductCharacteristicConfiguration:
  Table: "catalog_product_characteristics"
  HasIndex(pc => pc.ProductId)
  Property(pc => pc.AllowedValues).HasColumnType("jsonb")

PriceListConfiguration:
  Table: "catalog_price_lists"
  HasIndex(pl => new { pl.TenantId, pl.Name }).IsUnique()
  Property(pl => pl.CustomerSegments).HasColumnType("jsonb")
```

#### Repository Implementations

```
ProductRepository        -> EF Core with includes (Prices, Variants, Characteristics, Category)
CategoryRepository       -> EF Core with self-referencing hierarchy loading
PriceListRepository      -> EF Core
ProductVariantRepository -> EF Core
```

#### Integration Events

**Publishes:**
```
ProductCreatedEvent            -> Topic: "catalog.product.created"
ProductPublishedEvent          -> Topic: "catalog.product.published"
ProductDiscontinuedEvent       -> Topic: "catalog.product.discontinued"
ProductCategoryCreatedEvent    -> Topic: "catalog.category.created"
ProductCategoryDeletedEvent    -> Topic: "catalog.category.deleted"
ProductPriceChangedEvent       -> Topic: "catalog.product.price_changed"
PriceListCreatedEvent          -> Topic: "catalog.price_list.created"
```

**Subscribes to:**
```
orders.order.items_configured  -> Validates product configuration availability
```

#### Background Jobs

```
ProductStatusJob:
  Schedule: Hourly
  Automatically activates products with ValidFrom reached
  Automatically discontinues products with ValidTo reached

PriceExpiryJob:
  Schedule: Daily
  Deactivates expired prices
  Notifies if no active prices remain for a product
```

#### Caching Strategy

```
Cache: Category tree (Redis, 1 hour, evict on category change)
Cache: Active products list (Redis, 15 minutes, evict on product publish/discontinue)
Cache: Product by ID (Redis, 1 hour, evict on product change)
Cache: Product by slug (Redis, 1 hour, evict on product change)
Cache: Price lists (Redis, 1 hour, evict on price list change)
Cache: Product prices (Redis, 30 minutes, evict on price change)
```

### 3.4 API Layer

#### Endpoints

```
# Categories
GET    /api/v1/catalog/categories                      -> GetCategoryTree          [Authenticated]
GET    /api/v1/catalog/categories/{id}                 -> GetCategory              [Authenticated]
POST   /api/v1/catalog/categories                      -> CreateCategory           [Admin, Catalog.Admin]
PUT    /api/v1/catalog/categories/{id}                 -> UpdateCategory           [Admin, Catalog.Admin]
DELETE /api/v1/catalog/categories/{id}                 -> DeleteCategory           [Admin, Catalog.Admin]
POST   /api/v1/catalog/categories/{id}/move            -> MoveCategory             [Admin, Catalog.Admin]

# Products
GET    /api/v1/catalog/products                        -> SearchProducts           [Authenticated, Catalog.Read]
POST   /api/v1/catalog/products                        -> CreateProduct            [Admin, Catalog.Create]
GET    /api/v1/catalog/products/{id}                   -> GetProduct               [Authenticated, Catalog.Read]
PUT    /api/v1/catalog/products/{id}                   -> UpdateProduct            [Admin, Catalog.Update]
POST   /api/v1/catalog/products/{id}/publish           -> PublishProduct           [Admin, Catalog.Update]
POST   /api/v1/catalog/products/{id}/discontinue       -> DiscontinueProduct       [Admin, Catalog.Update]
POST   /api/v1/catalog/products/{id}/retire            -> RetireProduct            [Admin, Catalog.Admin]
GET    /api/v1/catalog/products/{id}/pricing           -> GetProductPricing        [Authenticated, Catalog.Read]
GET    /api/v1/catalog/products/{id}/variants          -> GetProductVariants       [Authenticated, Catalog.Read]
GET    /api/v1/catalog/products/{id}/characteristics    -> GetProductCharacteristics [Authenticated, Catalog.Read]
POST   /api/v1/catalog/products/{id}/prices            -> AddProductPrice          [Admin, Catalog.Update]
POST   /api/v1/catalog/products/{id}/variants          -> AddProductVariant        [Admin, Catalog.Update]
POST   /api/v1/catalog/products/{id}/characteristics   -> AddCharacteristic        [Admin, Catalog.Update]
DELETE /api/v1/catalog/products/{id}/prices/{priceId}  -> RemoveProductPrice       [Admin, Catalog.Update]
DELETE /api/v1/catalog/products/{id}/variants/{varId}  -> RemoveProductVariant     [Admin, Catalog.Update]
DELETE /api/v1/catalog/products/{id}/characteristics/{charId} -> RemoveCharacteristic [Admin, Catalog.Update]
GET    /api/v1/catalog/products/{id}/availability      -> CheckProductAvailability  [Authenticated, Catalog.Read]
POST   /api/v1/catalog/products/clone                 -> CloneProduct             [Admin, Catalog.Create]

# Price Lists
GET    /api/v1/catalog/price-lists                     -> ListPriceLists           [Admin, Catalog.Read]
POST   /api/v1/catalog/price-lists                     -> CreatePriceList          [Admin, Catalog.Admin]
GET    /api/v1/catalog/price-lists/{id}                -> GetPriceList             [Admin, Catalog.Read]
PUT    /api/v1/catalog/price-lists/{id}                -> UpdatePriceList          [Admin, Catalog.Admin]
DELETE /api/v1/catalog/price-lists/{id}                -> DeletePriceList          [Admin, Catalog.Admin]
POST   /api/v1/catalog/price-lists/{id}/set-default    -> SetDefaultPriceList      [Admin, Catalog.Admin]
```

#### Request/Response Models

```
CreateCategoryRequest    { Name, Slug, Description, ParentId, DisplayOrder, Attributes }
UpdateCategoryRequest    { Name, Description, DisplayOrder, Attributes }
CreateProductRequest     { CategoryId, Name, Slug, Description, Type, Tags, Attributes, Characteristics, Prices }
UpdateProductRequest     { Name, Description, Tags, Attributes }
AddPriceRequest          { Name, Type, Currency, Amount, Period, PeriodCount, TaxIncluded, Tiers, ValidFrom, ValidTo }
PriceTierModel           { From, To, UnitPrice, FlatFee }
AddVariantRequest        { Name, Sku, Attributes, Price }
AddCharacteristicRequest { Name, DataType, IsRequired, IsConfigurable, AllowedValues, DefaultValue, DisplayOrder }
CreatePriceListRequest   { Name, Description, Currency, IsDefault, Priority, CustomerSegmentIds }
PricingQueryRequest      { PriceListId, Quantity, Date, CustomerId, Characteristics }
CloneProductRequest      { ProductId, NewName, NewSlug }
```

#### Authorization Requirements

```
Catalog.Read: Browse catalog, view products and prices
Catalog.Create: Create products and variants
Catalog.Update: Modify product details, pricing, variants
Catalog.Admin: Full catalog administration including categories and price lists
```

#### Rate Limiting

```
Product search: 60 requests/minute per tenant
Product CRUD: 30 requests/minute per user
Category management: 20 requests/minute per user
Price list operations: 10 requests/minute per user
```

#### Versioning Strategy

```
URL-based: /api/v1/catalog
```

### 3.5 Events

#### Published Events

| Event | Topic | Schema |
|-------|-------|--------|
| ProductCreated | `catalog.product.created` | `{ ProductId, TenantId, CategoryId, Name, Slug, Type, Status, OccurredAt }` |
| ProductPublished | `catalog.product.published` | `{ ProductId, TenantId, Version, OccurredAt }` |
| ProductDiscontinued | `catalog.product.discontinued` | `{ ProductId, TenantId, OccurredAt }` |
| ProductRetired | `catalog.product.retired` | `{ ProductId, TenantId, OccurredAt }` |
| ProductPriceChanged | `catalog.product.price_changed` | `{ ProductId, PriceId, OldAmount, NewAmount, Currency, Type, OccurredAt }` |
| ProductCategoryCreated | `catalog.category.created` | `{ CategoryId, TenantId, Name, ParentId, OccurredAt }` |
| ProductCategoryDeleted | `catalog.category.deleted` | `{ CategoryId, TenantId, Name, OccurredAt }` |
| PriceListCreated | `catalog.price_list.created` | `{ PriceListId, TenantId, Name, Currency, IsDefault, OccurredAt }` |

#### Subscribed Events

| Event | Source | Action |
|-------|--------|--------|
| `orders.order.items_configured` | Orders | Validate product configuration |

### 3.6 Data Ownership

#### Owned Tables

| Table | Type | Purpose |
|-------|------|---------|
| `catalog_categories` | Primary | Product category hierarchy |
| `catalog_products` | Primary | Product definitions |
| `catalog_product_prices` | Primary | Product pricing |
| `catalog_product_variants` | Primary | Product variants/SKUs |
| `catalog_product_characteristics` | Primary | Product configurable characteristics |
| `catalog_price_lists` | Primary | Price list definitions |

#### Foreign Key Relationships

```
catalog_categories.tenant_id -> iam_tenants.id (RESTRICT)
catalog_categories.parent_id -> catalog_categories.id (RESTRICT)
catalog_products.tenant_id -> iam_tenants.id (RESTRICT)
catalog_products.category_id -> catalog_categories.id (RESTRICT)
catalog_product_prices.product_id -> catalog_products.id (CASCADE)
catalog_product_prices.price_list_id -> catalog_price_lists.id (SET NULL)
catalog_product_variants.product_id -> catalog_products.id (CASCADE)
catalog_product_characteristics.product_id -> catalog_products.id (CASCADE)
catalog_price_lists.tenant_id -> iam_tenants.id (RESTRICT)
```

#### Indexing Strategy

```
catalog_categories: (tenant_id, slug) UNIQUE, (tenant_id, parent_id)
catalog_products: (tenant_id, slug) UNIQUE, (tenant_id, status), (tenant_id, type), (tenant_id, category_id), (tenant_id, created_at)
catalog_products: gin((tags) jsonb_path_ops) for tag search
catalog_products: gin((attributes) jsonb_path_ops) for attribute search
catalog_product_prices: (product_id, valid_from, valid_to), (price_list_id)
catalog_product_variants: sku UNIQUE, (product_id)
catalog_product_characteristics: (product_id, name) UNIQUE
catalog_price_lists: (tenant_id, name) UNIQUE, (tenant_id, is_default)
```

### 3.7 Dependencies

#### Depends On

| Module | Reason |
|--------|--------|
| IAM | Tenant context |

#### Depended On By

| Module | Reason |
|--------|--------|
| Orders | Product references on order items |
| Subscriptions | Product references on subscriptions |
| Rating | Product configuration for usage rating |
| Billing | Product prices for invoice line items |
| Provisioning | Product characteristics for service provisioning |
| Reporting | Catalog analytics |

### 3.8 Security Requirements

#### RBAC Roles & Permissions

```
Module-level:
  Catalog.Read     -- View products, categories, prices
  Catalog.Create   -- Create new products and variants
  Catalog.Update   -- Modify products, add prices, manage characteristics
  Catalog.Admin    -- Manage categories, price lists, retire products

Role defaults:
  Admin: All permissions
  SalesAgent: Catalog.Read, Catalog.Create
  SupportAgent: Catalog.Read
  BillingAdmin: Catalog.Read
  ReadOnly: Catalog.Read
```

#### Data Isolation Rules

- Product catalogs are tenant-scoped
- Products pending publication (Draft) visible only to Admin roles
- Prices can be segment-specific (only visible to applicable customers)
- Discontinued products remain visible in customer subscriptions but not orderable

#### Audit Requirements

- All product version changes (Draft -> Active -> Discontinued -> Retired)
- All price changes (old and new amounts logged)
- Category creation and deletion
- Product variant SKU changes
- Price list changes

---

## 4. Orders — Order Management

### 4.1 Domain Layer

#### Aggregate: Order

```
Id: Guid
TenantId: Guid
OrderNumber: string
CustomerId: Guid
Status: OrderStatus
Type: OrderType               (New, Renewal, Change, Cancel, Transfer)
Source: string?
Items: List<OrderItem>
Payments: List<OrderPayment>
Notes: List<OrderNote>
Subtotal: decimal
DiscountAmount: decimal
TaxAmount: decimal
TotalAmount: decimal
Currency: string
BillingAddressId: Guid?
ShippingAddressId: Guid?
PriceListId: Guid?
SalesAgentId: Guid?
ChannelPartnerId: Guid?
CampaignId: Guid?
ExternalReference: string?
PlacedAt: DateTime
ActivationRequestedAt: DateTime?
CompletedAt: DateTime?
CancelledAt: DateTime?
CancellationReason: string?

Methods:
  AddItem(OrderItem item)
  RemoveItem(Guid itemId)
  UpdateItemQuantity(Guid itemId, int quantity)
  ApplyDiscount(decimal amount, string reason)
  ApplyTax(decimal amount, string taxCode)
  Place(): OrderPlacedResult
  Cancel(string reason)
  Complete()
  GetTotal(): decimal
  CanBeModified(): bool
```

#### Entity: OrderItem

```
Id: Guid
OrderId: Guid
ProductId: Guid
ProductName: string
ProductType: ProductType
Sku: string?
VariantId: Guid?
VariantName: string?
Quantity: int
UnitPrice: decimal
ListPrice: decimal
DiscountAmount: decimal
TaxAmount: decimal
TaxRate: decimal
TotalPrice: decimal
Currency: string
Configuration: Dictionary<string, object>
Characteristics: List<OrderItemCharacteristic>
BillingPeriod: RecurrencePeriod?
BillingCycles: int?
StartDate: DateTime?
EndDate: DateTime?
ProvisioningInstructions: string?
```

#### Entity: OrderItemCharacteristic

```
Id: Guid
OrderItemId: Guid
CharacteristicId: Guid
Name: string
Value: string
DataType: string
```

#### Entity: OrderPayment

```
Id: Guid
OrderId: Guid
Method: PaymentMethod
Amount: decimal
Currency: string
Status: PaymentStatus
TransactionId: string?
PaidAt: DateTime?
```

#### Entity: OrderNote

```
Id: Guid
OrderId: Guid
AuthorId: Guid
AuthorName: string
Content: string
Type: NoteType               (Internal, CustomerVisible)
CreatedAt: DateTime
```

#### Value Objects

```
OrderStatus: enum { Draft, PendingApproval, Approved, Placed, Processing, Completed, Cancelled, Failed }
OrderType: enum { New, Renewal, Change, Cancel, Transfer, Suspension, Unsuspension }
PaymentMethod: enum { CreditCard, DebitCard, BankTransfer, DirectDebit, Wallet, Voucher, Cash }
PaymentStatus: enum { Pending, Paid, Failed, Refunded, PartiallyRefunded }
NoteType: enum { Internal, CustomerVisible }
```

#### Domain Events

```
OrderPlacedEvent             { OrderId, OrderNumber, CustomerId, TenantId, TotalAmount, Items, PlacedAt }
OrderItemAddedEvent          { OrderId, ProductId, Quantity, UnitPrice, OccurredAt }
OrderItemRemovedEvent        { OrderId, ProductId, OccurredAt }
OrderItemQuantityChangedEvent { OrderId, ProductId, OldQuantity, NewQuantity, OccurredAt }
OrderApprovedEvent           { OrderId, ApprovedBy, OccurredAt }
OrderCompletedEvent          { OrderId, CompletedAt, OccurredAt }
OrderCancelledEvent          { OrderId, Reason, OccurredAt }
OrderPaymentAddedEvent       { OrderId, Amount, Method, OccurredAt }
OrderStatusChangedEvent      { OrderId, OldStatus, NewStatus, Reason, OccurredAt }
OrderPendingApprovalEvent    { OrderId, Items, TotalAmount, OccurredAt }
```

#### Domain Services

```
IOrderValidationService:
  ValidateOrder(Order order): OrderValidationResult
  ValidateItemConfiguration(OrderItem item): ValidationResult
  CheckCreditLimit(CustomerId, decimal amount): CreditCheckResult

IOrderPricingService:
  CalculateOrderTotals(Order order): OrderTotals
  ApplyPromotions(Order order): DiscountResult
  EstimateTax(Order order): TaxEstimate

IOrderWorkflowService:
  DetermineRequiredApprovals(Order order): List<ApprovalRequirement>
  CanTransition(OrderStatus from, OrderStatus to, string? userRole): bool
```

#### Repository Interfaces

```
IOrderRepository:
  GetByIdAsync(Guid id): Order?
  GetByNumberAsync(string orderNumber, Guid tenantId): Order?
  GetPagedAsync(Guid tenantId, OrderFilter filter): PagedResult<Order>
  GetByCustomerIdAsync(Guid customerId, int page, int size): PagedResult<Order>
  GetByStatusAsync(OrderStatus status, Guid tenantId): List<Order>
  AddAsync(Order order)
  UpdateAsync(Order order)
  CountByStatusAsync(Guid tenantId): Dictionary<OrderStatus, int>
  GetPendingApprovalAsync(Guid tenantId): List<Order>
```

#### Domain Rules / Invariants

- Order number must be unique per tenant (auto-generated)
- Draft orders have a configurable TTL before cleanup (e.g., 7 days)
- An order cannot be modified after completion or cancellation
- Order must contain at least one item to be placed
- Placed order total must match calculated total from items
- Cannot cancel a completed order -- must initiate refund process
- Order items must reference active products from the catalog
- Quantity must be within product MinQuantity/MaxQuantity range
- Order requires approval if total exceeds customer's credit limit
- Cancellation reason is required for cancellation

### 4.2 Application Layer

#### Commands

```
CreateOrderCommand             { TenantId, CustomerId, Type, Source, BillingAddressId, ShippingAddressId, PriceListId, SalesAgentId, ExternalReference }
AddItemToOrderCommand          { OrderId, ProductId, VariantId, Quantity, Configuration, Characteristics }
RemoveItemFromOrderCommand     { OrderId, ItemId }
UpdateItemQuantityCommand      { OrderId, ItemId, NewQuantity }
PlaceOrderCommand              { OrderId }
ApproveOrderCommand            { OrderId, ApprovedBy }
CompleteOrderCommand           { OrderId }
CancelOrderCommand             { OrderId, Reason }
AddOrderPaymentCommand         { OrderId, Method, Amount, TransactionId }
AddOrderNoteCommand            { OrderId, Content, Type, AuthorId }
ApplyDiscountCommand           { OrderId, Amount, Reason }
UpdateBillingAddressCommand    { OrderId, AddressId }
UpdateShippingAddressCommand   { OrderId, AddressId }
BulkOrderStatusCommand         { OrderIds[], NewStatus, Reason }
```

#### Queries

```
GetOrderQuery                  { OrderId }
GetOrderByNumberQuery          { OrderNumber, TenantId }
ListOrdersQuery                { TenantId, CustomerId?, Status?, Type?, DateFrom, DateTo, Page, Size }
GetCustomerOrdersQuery         { CustomerId, Page, Size }
GetPendingApprovalsQuery       { TenantId }
GetOrderHistoryQuery           { OrderId }
SearchOrdersQuery              { TenantId, SearchTerm, Page, Size }
GetOrderStatsQuery             { TenantId, DateFrom, DateTo }
ValidateOrderQuery             { OrderId }
GetOrderTimelineQuery          { OrderId }
```

#### Command Handlers

```
CreateOrderHandler             -> Creates draft order, assigns order number, returns OrderId
AddItemToOrderHandler          -> Validates product, calculates price, adds item to draft
RemoveItemFromOrderHandler     -> Validates order is modifiable, removes item, recalculates totals
PlaceOrderHandler              -> Validates order completeness, applies pricing, transitions -> Placed, publishes OrderPlacedEvent
ApproveOrderHandler            -> Transitions to Approved, publishes OrderApprovedEvent
CompleteOrderHandler           -> Transitions to Completed, publishes OrderCompletedEvent
CancelOrderHandler             -> Validates current status, transitions to Cancelled, publishes OrderCancelledEvent
AddOrderPaymentHandler         -> Adds payment record, publishes OrderPaymentAddedEvent
```

#### Query Handlers

```
GetOrderHandler                -> Returns OrderDetailDto with items, payments, timeline
ListOrdersHandler              -> Returns PagedResult<OrderSummaryDto>
GetCustomerOrdersHandler       -> Returns PagedResult<OrderSummaryDto> for customer
GetPendingApprovalsHandler     -> Returns List<OrderSummaryDto> needing approval
SearchOrdersHandler            -> Returns PagedResult<OrderSummaryDto> with search
GetOrderStatsHandler           -> Returns OrderStatsDto
```

#### Application Services

```
IOrderNumberGenerator:
  GenerateAsync(Guid tenantId): string

IOrderApprovalService:
  GetRequiredApprovals(Order order): List<ApprovalRequirement>
  SubmitForApproval(Order order)
  Approve(Order order, Guid approverId)
  Reject(Order order, Guid approverId, string reason)

IOrderCompletionService:
  CheckPreconditions(Order order): bool
  FinalizeOrder(Order order): Order
  TriggerProvisioning(Order order)
  TriggerBilling(Order order)
```

#### DTOs

```
OrderSummaryDto          { Id, OrderNumber, CustomerId, CustomerName, Status, Type, TotalAmount, Currency, ItemCount, PlacedAt, CompletedAt }
OrderDetailDto           { Id, OrderNumber, CustomerId, CustomerName, Status, Type, Source, Items, Payments, Subtotal, Discount, Tax, Total, Currency, BillingAddress, ShippingAddress, SalesAgent, Timeline, Notes, CreatedAt, PlacedAt, CompletedAt }
OrderItemDto             { Id, ProductId, ProductName, VariantName, Sku, Quantity, UnitPrice, ListPrice, Discount, Tax, Total, Configuration, Characteristics, BillingPeriod, StartDate, EndDate }
OrderPaymentDto          { Id, Method, Amount, Currency, Status, TransactionId, PaidAt }
OrderNoteDto             { Id, AuthorName, Content, Type, CreatedAt }
OrderTimelineDto         { Status, ChangedBy, ChangedAt, Note }
OrderStatsDto            { TotalOrders, OrdersByStatus, TotalRevenue, AverageValue, PendingApprovalCount }
```

#### Mapping Profiles

```
Order -> OrderSummaryDto, OrderDetailDto
OrderItem -> OrderItemDto
OrderPayment -> OrderPaymentDto
OrderNote -> OrderNoteDto
```

### 4.3 Infrastructure Layer

#### EF Core Entity Configurations

```
OrderConfiguration:
  Table: "orders_orders"
  HasIndex(o => new { o.TenantId, o.OrderNumber }).IsUnique()
  HasIndex(o => o.CustomerId)
  HasIndex(o => new { o.TenantId, o.Status })
  HasIndex(o => o.PlacedAt)
  HasMany(o => o.Items).WithOne().HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade)
  HasMany(o => o.Payments).WithOne().HasForeignKey(op => op.OrderId).OnDelete(DeleteBehavior.Cascade)

OrderItemConfiguration:
  Table: "orders_order_items"
  HasIndex(oi => oi.ProductId)
  Property(oi => oi.Configuration).HasColumnType("jsonb")
  Property(oi => oi.Characteristics).HasColumnType("jsonb")

OrderPaymentConfiguration:
  Table: "orders_order_payments"
  HasIndex(op => op.TransactionId).IsUnique().HasFilter("transaction_id IS NOT NULL")

OrderNoteConfiguration:
  Table: "orders_order_notes"
  HasIndex(on => on.OrderId)
  Property(on => on.Content).HasColumnType("text")

OrderStatusHistoryConfiguration:
  Table: "orders_status_history"
  HasIndex(osh => osh.OrderId)
  HasIndex(osh => osh.ChangedAt)
```

#### Repository Implementations

```
OrderRepository -> EF Core with items, payments, notes eager loading; filtering by status, date range, customer
```

#### Integration Events

**Publishes:**
```
OrderPlacedEvent            -> Topic: "orders.order.placed"
OrderCompletedEvent         -> Topic: "orders.order.completed"
OrderCancelledEvent         -> Topic: "orders.order.cancelled"
OrderItemAddedEvent         -> Topic: "orders.order.item_added"
OrderStatusChangedEvent     -> Topic: "orders.order.status_changed"
OrderPaymentAddedEvent      -> Topic: "orders.order.payment_added"
OrderPendingApprovalEvent   -> Topic: "orders.order.pending_approval"
```

**Subscribes to:**
```
crm.customer.status_changed  -> Cancel draft orders if customer suspended
catalog.product.discontinued -> Flag affected draft orders
provisioning.service.activated -> Update order status if service activation completes order
```

#### Background Jobs

```
DraftOrderCleanupJob:
  Schedule: Daily
  Cancels draft orders older than 7 days

OrderApprovalEscalationJob:
  Schedule: Every 30 minutes
  Escalates unapproved orders after threshold time

OrderCompletionJob:
  Schedule: Every 15 minutes
  Processes completed orders for downstream actions
```

#### Caching Strategy

```
Cache: Order by ID (Redis, 15 minutes, evict on order change)
Cache: Order count by status (Redis, 5 minutes)
Cache: Order statistics (Redis, 1 hour, evict on new order)
Cache: Order number sequence (Redis, persistent counter)
```

#### External Service Integrations

```
Inventory System (external warehouse):
  - Check stock availability
  - Reserve inventory on order placement

Fraud Detection Service:
  - Score order for fraud risk
  - Flag high-risk orders for manual review

Tax Calculation Service (Avalara, Vertex):
  - Calculate tax for order items
  - Validate tax codes
```

### 4.4 API Layer

#### Endpoints

```
GET    /api/v1/orders                              -> ListOrders                [Authenticated, Order.Read]
POST   /api/v1/orders                              -> CreateOrder               [Authenticated, Order.Create]
GET    /api/v1/orders/{id}                         -> GetOrder                  [Authenticated, Order.Read]
PUT    /api/v1/orders/{id}                         -> UpdateOrder               [Authenticated, Order.Update]
DELETE /api/v1/orders/{id}                         -> CancelOrder               [Authenticated, Order.Update]
POST   /api/v1/orders/{id}/items                   -> AddItem                   [Authenticated, Order.Update]
DELETE /api/v1/orders/{id}/items/{itemId}          -> RemoveItem                [Authenticated, Order.Update]
PATCH  /api/v1/orders/{id}/items/{itemId}/quantity -> UpdateQuantity            [Authenticated, Order.Update]
POST   /api/v1/orders/{id}/place                   -> PlaceOrder                [Authenticated, Order.Create]
POST   /api/v1/orders/{id}/approve                 -> ApproveOrder              [Admin, Order.Approve]
POST   /api/v1/orders/{id}/complete                -> CompleteOrder             [Admin, Order.Admin]
POST   /api/v1/orders/{id}/cancel                  -> CancelOrder               [Authenticated, Order.Update]
POST   /api/v1/orders/{id}/payments                -> AddPayment                [Authenticated, Order.Update]
POST   /api/v1/orders/{id}/notes                   -> AddNote                   [Authenticated, Order.Update]
POST   /api/v1/orders/{id}/discount                -> ApplyDiscount             [Admin, Order.Admin]
GET    /api/v1/orders/{id}/timeline                -> GetOrderTimeline          [Authenticated, Order.Read]
GET    /api/v1/orders/stats                        -> GetOrderStats             [Admin, Order.Read]
POST   /api/v1/orders/bulk-status                  -> BulkOrderStatus           [Admin, Order.Admin]
```

#### Request/Response Models

```
CreateOrderRequest       { CustomerId, Type, Source, BillingAddressId, ShippingAddressId, PriceListId, SalesAgentId, ExternalReference }
AddItemRequest           { ProductId, VariantId, Quantity, Configuration, Characteristics }
UpdateQuantityRequest    { NewQuantity }
PlaceOrderRequest        { } (empty or with confirmation)
ApproveOrderRequest      { Comments }
CancelOrderRequest       { Reason }
AddPaymentRequest        { Method, Amount, TransactionId }
AddNoteRequest           { Content, Type }
ApplyDiscountRequest     { Amount, Reason }
BulkStatusRequest        { OrderIds[], NewStatus, Reason }
```

#### Authorization Requirements

```
Order.Read: View orders, search, get timeline/history
Order.Create: Create orders, place orders
Order.Update: Modify draft orders, add items, cancel own orders
Order.Approve: Approve/reject orders pending approval
Order.Admin: Full order administration, bulk operations, discounts
```

#### Rate Limiting

```
Order creation: 20 requests/minute per user
Order placement: 10 requests/minute per user
Order search: 30 requests/minute
Order approval: 20 requests/minute per approver
Bulk operations: 5 requests/minute
```

#### Versioning Strategy

```
URL-based: /api/v1/orders
```

### 4.5 Events

#### Published Events

| Event | Topic | Schema |
|-------|-------|--------|
| OrderPlaced | `orders.order.placed` | `{ OrderId, OrderNumber, CustomerId, TenantId, Items[], TotalAmount, Currency, PlacedAt }` |
| OrderCompleted | `orders.order.completed` | `{ OrderId, OrderNumber, CustomerId, TenantId, CompletedAt }` |
| OrderCancelled | `orders.order.cancelled` | `{ OrderId, OrderNumber, CustomerId, TenantId, Reason, CancelledAt }` |
| OrderStatusChanged | `orders.order.status_changed` | `{ OrderId, OrderNumber, OldStatus, NewStatus, Reason, ChangedBy, OccurredAt }` |
| OrderPaymentAdded | `orders.order.payment_added` | `{ OrderId, Amount, Method, Currency, TransactionId, OccurredAt }` |
| OrderPendingApproval | `orders.order.pending_approval` | `{ OrderId, OrderNumber, CustomerId, TotalAmount, RequiredApprovers[], OccurredAt }` |

#### Subscribed Events

| Event | Source | Action |
|-------|--------|--------|
| `crm.customer.status_changed` | CRM | Cancel draft orders if customer suspended |
| `catalog.product.discontinued` | ProductCatalog | Flag affected draft orders |
| `catalog.product.price_changed` | ProductCatalog | Recalculate prices in draft orders |
| `provisioning.service.activated` | Provisioning | Mark relevant order items as provisioned |

### 4.6 Data Ownership

#### Owned Tables

| Table | Type | Purpose |
|-------|------|---------|
| `orders_orders` | Primary | Order headers |
| `orders_order_items` | Primary | Order line items |
| `orders_order_payments` | Secondary | Payment records on orders |
| `orders_order_notes` | Secondary | Order notes |
| `orders_status_history` | Secondary | Order status change audit trail |

#### Foreign Key Relationships

```
orders_orders.tenant_id -> iam_tenants.id (RESTRICT)
orders_orders.customer_id -> crm_customers.id (RESTRICT)
orders_orders.billing_address_id -> crm_addresses.id (NO ACTION)
orders_orders.shipping_address_id -> crm_addresses.id (NO ACTION)
orders_order_items.order_id -> orders_orders.id (CASCADE)
orders_order_items.product_id -> catalog_products.id (RESTRICT)
orders_order_payments.order_id -> orders_orders.id (CASCADE)
orders_order_notes.order_id -> orders_orders.id (CASCADE)
orders_status_history.order_id -> orders_orders.id (CASCADE)
```

#### Indexing Strategy

```
orders_orders: (tenant_id, order_number) UNIQUE, (tenant_id, customer_id), (tenant_id, status), (tenant_id, placed_at), (tenant_id, created_at)
orders_order_items: (order_id), (product_id), (variant_id)
orders_order_payments: (order_id), transaction_id (UNIQUE)
orders_order_notes: (order_id, created_at)
orders_status_history: (order_id, changed_at), (order_id, status)
```

### 4.7 Dependencies

#### Depends On

| Module | Reason |
|--------|--------|
| IAM | Tenant context, user/sales agent references |
| CRM | Customer reference on orders |
| ProductCatalog | Product references on order items |

#### Depended On By

| Module | Reason |
|--------|--------|
| Subscriptions | Orders trigger subscription creation |
| Provisioning | Orders trigger service provisioning |
| Billing | Orders trigger billing |
| Invoices | Order references on invoices |
| Workflow | Order status transitions trigger workflows |
| Reporting | Order analytics |

### 4.8 Security Requirements

#### RBAC Roles & Permissions

```
Module-level:
  Order.Read       -- View orders, search, timeline
  Order.Create     -- Create and place orders
  Order.Update     -- Modify draft orders, add items, cancel
  Order.Approve    -- Approve/reject orders
  Order.Admin      -- Full order management, bulk operations, discounts

Role defaults:
  Admin: All permissions
  SalesAgent: Order.Read, Order.Create, Order.Update
  SupportAgent: Order.Read
  BillingAdmin: Order.Read, Order.Update
  ReadOnly: Order.Read
```

#### Data Isolation Rules

- Orders are tenant-scoped
- Sales agents can only view/modify their own orders (configurable)
- Customer can only view their own orders
- Order payment details (transaction IDs) are access-restricted
- Draft orders are visible only to creator and admin

#### Audit Requirements

- Every order status change (with old/new status, timestamp, actor)
- Every item addition/removal/quantity change
- Payment addition
- Discount application (amount, reason, approver)
- Order cancellation (reason, actor, timestamp)
- Order approval/rejection (comments, actor)

---

## 5. Subscriptions — Subscription Management

### 5.1 Domain Layer

#### Aggregate: Subscription

```
Id: Guid
TenantId: Guid
SubscriptionNumber: string
CustomerId: Guid
OrderId: Guid?
Status: SubscriptionStatus
Type: SubscriptionType         (Recurring, Prepaid, Postpaid, Hybrid)
Products: List<SubscriptionProduct>
CurrentTermStart: DateTime
CurrentTermEnd: DateTime
NextBillingDate: DateTime?
LastBillingDate: DateTime?
ActivationDate: DateTime?
TerminationDate: DateTime?
CancellationDate: DateTime?
CancellationReason: string?
AutoRenew: bool
RenewalCount: int
BillingCycle: RecurrencePeriod
BillingDay: int
Currency: string
CurrentBalance: decimal
PrepaidBalance: decimal?
Notes: string?

Methods:
  Activate(DateTime activationDate)
  Suspend(string reason)
  Unsuspend()
  Terminate(DateTime terminationDate, string reason)
  Cancel(string reason, DateTime? effectiveDate)
  Renew()
  ChangeProduct(Guid productId, SubscriptionProductChange change)
  AddProduct(SubscriptionProduct product)
  RemoveProduct(Guid productId, DateTime effectiveDate)
  AddBalance(decimal amount, string reason)
  DeductBalance(decimal amount, string reason)
  SetNextBillingDate(DateTime date)
  IsActive(): bool
  CanBeModified(): bool
  DaysUntilRenewal(): int
```

#### Entity: SubscriptionProduct

```
Id: Guid
SubscriptionId: Guid
ProductId: Guid
ProductName: string
ProductType: string
Quantity: int
UnitPrice: decimal
Currency: string
Status: SubscriptionProductStatus      (Active, Suspended, Cancelled)
BillingPeriod: RecurrencePeriod
BillingCycles: int?
CyclesRemaining: int?
StartDate: DateTime
EndDate: DateTime?
ActivationDate: DateTime?
SuspensionDate: DateTime?
CancellationDate: DateTime?
Characteristics: Dictionary<string, object>
ServiceIds: List<Guid>?
Pricing: SubscriptionProductPricing
```

#### Value Object: SubscriptionProductPricing

```
UnitPrice: decimal
DiscountAmount: decimal
DiscountPercentage: decimal?
DiscountReason: string?
TaxRate: decimal
TaxIncluded: bool
SetupFee: decimal?
RecurringFee: decimal
```

#### Entity: SubscriptionCharge

```
Id: Guid
SubscriptionId: Guid
ProductId: Guid?
Type: ChargeType               (Recurring, Usage, OneTime, Setup, Adjustment, Credit)
Description: string
Amount: decimal
Currency: string
Quantity: int?
UnitPrice: decimal?
BillingPeriod: RecurrencePeriod?
Status: ChargeStatus            (Pending, Billed, Invoiced, Paid, WrittenOff)
FromDate: DateTime
ToDate: DateTime?
BilledAt: DateTime?
InvoicedAt: DateTime?
ExternalReference: string?
```

#### Entity: SubscriptionTerm

```
Id: Guid
SubscriptionId: Guid
StartDate: DateTime
EndDate: DateTime
DurationMonths: int
RenewalTerm: bool
Status: TermStatus              (Active, Completed, Cancelled)
BillingPeriods: int
CompletedPeriods: int
CommitmentAmount: decimal?
EarlyTerminationFee: decimal?
```

#### Value Objects

```
SubscriptionStatus: enum { Pending, Active, Suspended, Cancelled, Terminated, Expired }
SubscriptionType: enum { Recurring, Prepaid, Postpaid, Hybrid, OneTime }
SubscriptionProductStatus: enum { Pending, Active, Suspended, Cancelled }
ChargeType: enum { Recurring, Usage, OneTime, Setup, Adjustment, Credit, Discount, Termination }
ChargeStatus: enum { Pending, Billed, Invoiced, Paid, WrittenOff, Disputed }
TermStatus: enum { Active, Completed, Cancelled, Expired }
```

#### Domain Events

```
SubscriptionCreatedEvent          { SubscriptionId, CustomerId, TenantId, Products, OccurredAt }
SubscriptionActivatedEvent       { SubscriptionId, ActivationDate, OccurredAt }
SubscriptionSuspendedEvent       { SubscriptionId, Reason, OccurredAt }
SubscriptionUnsuspendedEvent     { SubscriptionId, OccurredAt }
SubscriptionCancelledEvent       { SubscriptionId, Reason, EffectiveDate, OccurredAt }
SubscriptionTerminatedEvent      { SubscriptionId, Reason, OccurredAt }
SubscriptionRenewedEvent         { SubscriptionId, NewTermStart, NewTermEnd, OccurredAt }
SubscriptionProductAddedEvent    { SubscriptionId, ProductId, Quantity, OccurredAt }
SubscriptionProductRemovedEvent  { SubscriptionId, ProductId, EffectiveDate, OccurredAt }
SubscriptionProductChangedEvent  { SubscriptionId, ProductId, ChangeType, OccurredAt }
SubscriptionBalanceAdjustedEvent { SubscriptionId, Amount, Reason, NewBalance, OccurredAt }
SubscriptionChargeGeneratedEvent { SubscriptionId, ChargeId, Amount, Type, OccurredAt }
```

#### Domain Services

```
ISubscriptionLifecycleService:
  ActivateSubscription(Subscription subscription, DateTime activationDate): Result
  ProcessRenewal(Subscription subscription): Subscription
  CalculateTermination(Subscription subscription, DateTime date): TerminationCalculation

ISubscriptionBillingService:
  GenerateRecurringCharges(Subscription subscription, DateTime billingDate): List<SubscriptionCharge>
  CalculateProration(SubscriptionProduct product, DateTime from, DateTime to): ProrationResult
  EstimateNextInvoice(Subscription subscription, DateTime billingDate): InvoiceEstimate

ISubscriptionProvisioningService:
  GetServicesToProvision(Subscription subscription): List<ServiceProvisioningRequest>
  GetServicesToDeprovision(Subscription subscription): List<ServiceDeprovisioningRequest>

IUsageIntegrationService:
  GetUsageForBilling(Subscription subscription, DateTime from, DateTime to): List<UsageRecord>
  CalculateUsageCharges(List<UsageRecord> usage, Product product): List<SubscriptionCharge>
```

#### Repository Interfaces

```
ISubscriptionRepository:
  GetByIdAsync(Guid id): Subscription?
  GetByNumberAsync(string number, Guid tenantId): Subscription?
  GetByCustomerIdAsync(Guid customerId): List<Subscription>
  GetPagedAsync(Guid tenantId, SubscriptionFilter filter): PagedResult<Subscription>
  GetActiveAsync(Guid tenantId): List<Subscription>
  GetDueForBillingAsync(Guid tenantId, DateTime billingDate): List<Subscription>
  GetDueForRenewalAsync(Guid tenantId, DateTime date): List<Subscription>
  AddAsync(Subscription subscription)
  UpdateAsync(Subscription subscription)
  CountByStatusAsync(Guid tenantId): Dictionary<SubscriptionStatus, int>

ISubscriptionChargeRepository:
  GetByIdAsync(Guid id): SubscriptionCharge?
  GetBySubscriptionIdAsync(Guid subscriptionId): List<SubscriptionCharge>
  GetByStatusAsync(ChargeStatus status, Guid tenantId): List<SubscriptionCharge>
  GetUnbilledChargesAsync(Guid subscriptionId): List<SubscriptionCharge>
  AddAsync(SubscriptionCharge charge)
  UpdateAsync(SubscriptionCharge charge)
  AddRangeAsync(List<SubscriptionCharge> charges)

ISubscriptionTermRepository:
  GetBySubscriptionIdAsync(Guid subscriptionId): List<SubscriptionTerm>
  GetActiveAsync(Guid subscriptionId): SubscriptionTerm?
  AddAsync(SubscriptionTerm term)
  UpdateAsync(SubscriptionTerm term)
```

#### Domain Rules / Invariants

- Subscription number must be unique per tenant
- A subscription must have at least one product
- Cannot activate a pending subscription without billing configuration
- Cannot suspend an already-suspended subscription
- Cannot terminate an active subscription without processing final charges
- Prepaid subscriptions cannot have negative balances
- Recurring subscriptions must have a defined billing cycle and billing day
- Product end date must be after product start date
- Cannot remove the last active product from a subscription
- Subscription cancellation requires a reason
- Auto-renew must be explicitly confirmed before renewal processing
- Billing day must be between 1-28

### 5.2 Application Layer

#### Commands

```
CreateSubscriptionCommand      { TenantId, CustomerId, OrderId, Type, BillingCycle, BillingDay, Currency, Products }
ActivateSubscriptionCommand    { SubscriptionId, ActivationDate }
SuspendSubscriptionCommand     { SubscriptionId, Reason }
UnsuspendSubscriptionCommand   { SubscriptionId }
CancelSubscriptionCommand      { SubscriptionId, Reason, EffectiveDate }
TerminateSubscriptionCommand   { SubscriptionId, Reason, TerminationDate }
RenewSubscriptionCommand       { SubscriptionId }
AddSubscriptionProductCommand  { SubscriptionId, ProductId, Quantity, Pricing, Characteristics }
RemoveSubscriptionProductCommand { SubscriptionId, ProductId, EffectiveDate }
ChangeProductQuantityCommand   { SubscriptionId, ProductId, NewQuantity, EffectiveDate }
AdjustBalanceCommand           { SubscriptionId, Amount, Reason, Type }
EnableAutoRenewCommand         { SubscriptionId }
DisableAutoRenewCommand        { SubscriptionId }
GenerateChargeCommand          { SubscriptionId, ProductId, Type, Amount, Description, FromDate, ToDate }
```

#### Queries

```
GetSubscriptionQuery           { SubscriptionId }
GetSubscriptionByNumberQuery   { SubscriptionNumber, TenantId }
ListSubscriptionsQuery         { TenantId, CustomerId, Status, Type, ProductId, Page, Size }
GetCustomerSubscriptionsQuery  { CustomerId }
SearchSubscriptionsQuery       { TenantId, SearchTerm, Page, Size }
GetSubscriptionChargesQuery    { SubscriptionId, Status, Type, FromDate, ToDate }
GetSubscriptionBalanceQuery    { SubscriptionId }
GetSubscriptionTimelineQuery   { SubscriptionId }
GetSubscriptionsDueForBillingQuery { TenantId, BillingDate }
GetSubscriptionStatsQuery      { TenantId }
EstimateNextInvoiceQuery       { SubscriptionId }
GetSubscriptionTermsQuery      { SubscriptionId }
```

#### Command Handlers

```
CreateSubscriptionHandler      -> Validates products, calculates initial charges, creates subscription, creates terms, publishes SubscriptionCreatedEvent
ActivateSubscriptionHandler    -> Sets products active, records activation date, publishes SubscriptionActivatedEvent
SuspendSubscriptionHandler     -> Validates current state, suspends, publishes SubscriptionSuspendedEvent
CancelSubscriptionHandler      -> Validates reason, calculates early termination if applicable, cancels, publishes SubscriptionCancelledEvent
TerminateSubscriptionHandler   -> Finalizes all charges, terminates, publishes SubscriptionTerminatedEvent
RenewSubscriptionHandler       -> Validates auto-renewal conditions, creates new term, publishes SubscriptionRenewedEvent
AddSubscriptionProductHandler  -> Validates product, adds with pricing, recalculates billing, publishes SubscriptionProductAddedEvent
AdjustBalanceHandler           -> Adjusts balance, publishes SubscriptionBalanceAdjustedEvent
GenerateChargeHandler          -> Creates charge, publishes SubscriptionChargeGeneratedEvent
```

#### Query Handlers

```
GetSubscriptionHandler         -> Returns SubscriptionDetailDto with products, charges, terms, balance
ListSubscriptionsHandler       -> Returns PagedResult<SubscriptionSummaryDto>
GetCustomerSubscriptionsHandler -> Returns List<SubscriptionSummaryDto>
GetSubscriptionChargesHandler  -> Returns PagedResult<SubscriptionChargeDto>
GetSubscriptionBalanceHandler  -> Returns SubscriptionBalanceDto
EstimateNextInvoiceHandler     -> Returns InvoiceEstimateDto
GetSubscriptionStatsHandler    -> Returns SubscriptionStatsDto
```

#### Application Services

```
ISubscriptionNumberGenerator:
  GenerateAsync(Guid tenantId): string

ISubscriptionValidatorService:
  ValidateForBilling(Subscription subscription): ValidationResult
  ValidateProductCompatibility(Subscription subscription, Guid productId): ValidationResult
  CheckForOverlappingProduct(Subscription subscription, Guid productId): bool
  CalculateProrationCredit(Subscription subscription, Guid productId, DateTime effectiveDate): decimal

ISubscriptionNotificationService:
  NotifyUpcomingRenewal(Subscription subscription)
  NotifySuspension(Subscription subscription, string reason)
  NotifyCancellation(Subscription subscription, string reason)
  NotifyUpcomingExpiry(Subscription subscription)
```

#### DTOs

```
SubscriptionSummaryDto     { Id, SubscriptionNumber, CustomerId, CustomerName, Status, Type, ProductNames, NextBillingDate, CurrentBalance, Currency, AutoRenew, CreatedAt }
SubscriptionDetailDto      { Id, SubscriptionNumber, CustomerId, CustomerName, Status, Type, Products, Terms, Charges, Balance, NextBillingDate, LastBillingDate, BillingCycle, BillingDay, AutoRenew, ActivationDate, Timeline, CreatedAt }
SubscriptionProductDto     { Id, ProductId, ProductName, Quantity, UnitPrice, Currency, Status, StartDate, EndDate, Characteristics, ServiceCount }
SubscriptionChargeDto      { Id, ProductId, ProductName, Type, Description, Amount, Currency, Quantity, Status, FromDate, ToDate, BilledAt }
SubscriptionTermDto        { Id, StartDate, EndDate, DurationMonths, RenewalTerm, Status, CommitmentAmount }
SubscriptionBalanceDto     { CurrentBalance, PrepaidBalance, Currency, PendingCharges, LastChargeDate }
SubscriptionStatsDto       { TotalActive, TotalByStatus, TotalByType, MRR, ARR, ChurnedLastMonth, ActiveByProduct }
InvoiceEstimateDto         { SubscriptionId, BillingDate, Items[], Subtotal, Discount, Tax, Total }
SubscriptionTimelineDto    { Event, Description, OccurredAt }
```

#### Mapping Profiles

```
Subscription -> SubscriptionSummaryDto, SubscriptionDetailDto
SubscriptionProduct -> SubscriptionProductDto
SubscriptionCharge -> SubscriptionChargeDto
SubscriptionTerm -> SubscriptionTermDto
```

### 5.3 Infrastructure Layer

#### EF Core Entity Configurations

```
SubscriptionConfiguration:
  Table: "subscriptions"
  HasIndex(s => new { s.TenantId, s.SubscriptionNumber }).IsUnique()
  HasIndex(s => s.CustomerId)
  HasIndex(s => new { s.TenantId, s.Status })
  HasIndex(s => new { s.TenantId, s.NextBillingDate }).HasFilter("next_billing_date IS NOT NULL")
  HasIndex(s => s.NextBillingDate)
  Property(s => s.Notes).HasColumnType("text")
  HasMany(s => s.Products).WithOne().HasForeignKey(sp => sp.SubscriptionId).OnDelete(DeleteBehavior.Cascade)
  HasMany(s => s.Charges).WithOne().HasForeignKey(sc => sc.SubscriptionId).OnDelete(DeleteBehavior.Cascade)
  HasMany(s => s.Terms).WithOne().HasForeignKey(st => st.SubscriptionId).OnDelete(DeleteBehavior.Cascade)

SubscriptionProductConfiguration:
  Table: "subscription_products"
  HasIndex(sp => new { sp.SubscriptionId, sp.ProductId })
  Property(sp => sp.Characteristics).HasColumnType("jsonb")
  OwnsOne(sp => sp.Pricing)

SubscriptionChargeConfiguration:
  Table: "subscription_charges"
  HasIndex(sc => sc.SubscriptionId)
  HasIndex(sc => new { sc.Status, sc.BilledAt })
  Property(sc => sc.Description).HasMaxLength(500)

SubscriptionTermConfiguration:
  Table: "subscription_terms"
  HasIndex(st => new { st.SubscriptionId, st.Status })
  HasIndex(st => st.EndDate)
```

#### Repository Implementations

```
SubscriptionRepository         -> EF Core implementation with products, charges, terms
SubscriptionChargeRepository   -> EF Core with filtering by status/date
SubscriptionTermRepository     -> EF Core
```

#### Integration Events

**Publishes:**
```
SubscriptionCreatedEvent            -> Topic: "subscription.created"
SubscriptionActivatedEvent          -> Topic: "subscription.activated"
SubscriptionSuspendedEvent          -> Topic: "subscription.suspended"
SubscriptionCancelledEvent          -> Topic: "subscription.cancelled"
SubscriptionTerminatedEvent         -> Topic: "subscription.terminated"
SubscriptionRenewedEvent            -> Topic: "subscription.renewed"
SubscriptionProductAddedEvent       -> Topic: "subscription.product_added"
SubscriptionProductRemovedEvent     -> Topic: "subscription.product_removed"
SubscriptionChargeGeneratedEvent    -> Topic: "subscription.charge_generated"
SubscriptionBalanceAdjustedEvent    -> Topic: "subscription.balance_adjusted"
```

**Subscribes to:**
```
orders.order.completed              -> Create subscription from order items
billing.invoice.generated           -> Mark subscription charges as billed
billing.invoice.paid                -> Update subscription balance
billing.invoice.failed              -> Flag subscription for suspension
payments.payment.received           -> Update prepaid balance
rating.usage.rated                  -> Generate usage-based charges
provisioning.service.activated      -> Update subscription product status
collections.debt.status_changed     -> Suspend subscription if collection initiated
```

#### Background Jobs

```
RecurringChargeGenerationJob:
  Schedule: Daily (runs at midnight)
  Identifies subscriptions due for billing
  Generates recurring charges for each

SubscriptionRenewalJob:
  Schedule: Daily
  Identifies subscriptions expiring within window
  Processes auto-renewals

SubscriptionSuspensionJob:
  Schedule: Hourly
  Suspends subscriptions past due threshold

SubscriptionExpiryJob:
  Schedule: Daily
  Terminates expired subscriptions

PrepaidBalanceWarningJob:
  Schedule: Daily
  Identifies prepaid subscriptions below threshold
```

#### Caching Strategy

```
Cache: Subscription by ID (Redis, 15 minutes, evict on change)
Cache: Subscription by number (Redis, 15 minutes, evict on change)
Cache: Customer active subscriptions (Redis, 30 minutes, evict on change)
Cache: Subscription counts by status (Redis, 1 hour)
Cache: MRR/ARR calculations (Redis, 1 hour, evict on billing event)
```

### 5.4 API Layer

#### Endpoints

```
GET    /api/v1/subscriptions                           -> ListSubscriptions           [Authenticated, Subscription.Read]
POST   /api/v1/subscriptions                           -> CreateSubscription          [Authenticated, Subscription.Create]
GET    /api/v1/subscriptions/{id}                      -> GetSubscription             [Authenticated, Subscription.Read]
PATCH  /api/v1/subscriptions/{id}                      -> UpdateSubscription          [Authenticated, Subscription.Update]
DELETE /api/v1/subscriptions/{id}                      -> CancelSubscription          [Authenticated, Subscription.Update]
POST   /api/v1/subscriptions/{id}/activate             -> ActivateSubscription        [Admin, Subscription.Update]
POST   /api/v1/subscriptions/{id}/suspend              -> SuspendSubscription         [Admin, Subscription.Update]
POST   /api/v1/subscriptions/{id}/unsuspend            -> UnsuspendSubscription       [Admin, Subscription.Update]
POST   /api/v1/subscriptions/{id}/terminate            -> TerminateSubscription       [Admin, Subscription.Admin]
POST   /api/v1/subscriptions/{id}/renew                -> RenewSubscription           [Admin, Subscription.Update]
POST   /api/v1/subscriptions/{id}/auto-renew/enable    -> EnableAutoRenew             [Authenticated, Subscription.Update]
POST   /api/v1/subscriptions/{id}/auto-renew/disable   -> DisableAutoRenew            [Authenticated, Subscription.Update]
POST   /api/v1/subscriptions/{id}/products             -> AddSubscriptionProduct      [Authenticated, Subscription.Update]
DELETE /api/v1/subscriptions/{id}/products/{productId} -> RemoveSubscriptionProduct   [Authenticated, Subscription.Update]
PATCH  /api/v1/subscriptions/{id}/products/{productId}/quantity -> ChangeProductQuantity [Authenticated, Subscription.Update]
GET    /api/v1/subscriptions/{id}/charges              -> GetSubscriptionCharges      [Authenticated, Subscription.Read]
POST   /api/v1/subscriptions/{id}/charges              -> GenerateCharge              [Admin, Subscription.Update]
GET    /api/v1/subscriptions/{id}/balance              -> GetSubscriptionBalance      [Authenticated, Subscription.Read]
POST   /api/v1/subscriptions/{id}/balance/adjust       -> AdjustBalance               [Admin, Subscription.Admin]
GET    /api/v1/subscriptions/{id}/timeline             -> SubscriptionTimeline        [Authenticated, Subscription.Read]
GET    /api/v1/subscriptions/{id}/terms                -> GetSubscriptionTerms        [Authenticated, Subscription.Read]
GET    /api/v1/subscriptions/{id}/estimate             -> EstimateNextInvoice         [Authenticated, Subscription.Read]
GET    /api/v1/subscriptions/stats                     -> GetSubscriptionStats        [Admin, Subscription.Read]
PATCH  /api/v1/subscriptions/{id}/billing-day          -> UpdateBillingDay            [Admin, Subscription.Update]
```

#### Request/Response Models

```
CreateSubscriptionRequest      { CustomerId, OrderId, Type, BillingCycle, BillingDay, Currency, Products[] }
SubscriptionProductRequest     { ProductId, Quantity, Characteristics, Pricing }
SubscriptionPricingRequest     { UnitPrice, DiscountAmount, DiscountPercentage, TaxRate, SetupFee }
SuspendRequest                 { Reason }
CancelRequest                  { Reason, EffectiveDate }
TerminateRequest               { Reason, TerminationDate }
AdjustBalanceRequest           { Amount, Reason, Type }
GenerateChargeRequest          { ProductId, Type, Amount, Description, FromDate, ToDate }
ChangeQuantityRequest          { NewQuantity, EffectiveDate }
```

#### Authorization Requirements

```
Subscription.Read: View subscriptions, charges, balance, timeline
Subscription.Create: Create new subscriptions
Subscription.Update: Modify subscriptions, manage products, cancel
Subscription.Admin: Terminate, adjust balances, force renewal, override billing settings
```

#### Rate Limiting

```
Subscription creation: 20 requests/minute
Subscription modification: 30 requests/minute
Charge generation: Manual 10/minute; Bulk 5/minute
Balance adjustments: 5 requests/minute
```

#### Versioning Strategy

```
URL-based: /api/v1/subscriptions
```

### 5.5 Events

#### Published Events

| Event | Topic | Schema |
|-------|-------|--------|
| SubscriptionCreated | `subscription.created` | `{ SubscriptionId, SubscriptionNumber, CustomerId, TenantId, Products[], Type, OccurredAt }` |
| SubscriptionActivated | `subscription.activated` | `{ SubscriptionId, CustomerId, TenantId, ActivationDate, Products[], OccurredAt }` |
| SubscriptionSuspended | `subscription.suspended` | `{ SubscriptionId, CustomerId, TenantId, Reason, OccurredAt }` |
| SubscriptionCancelled | `subscription.cancelled` | `{ SubscriptionId, CustomerId, TenantId, Reason, EffectiveDate, OccurredAt }` |
| SubscriptionTerminated | `subscription.terminated` | `{ SubscriptionId, CustomerId, TenantId, Reason, TerminationDate, FinalCharges[], OccurredAt }` |
| SubscriptionRenewed | `subscription.renewed` | `{ SubscriptionId, CustomerId, TenantId, NewTermStart, NewTermEnd, RenewalCount, OccurredAt }` |
| SubscriptionProductAdded | `subscription.product_added` | `{ SubscriptionId, ProductId, ProductName, Quantity, EffectiveDate, OccurredAt }` |
| SubscriptionProductRemoved | `subscription.product_removed` | `{ SubscriptionId, ProductId, EffectiveDate, OccurredAt }` |
| SubscriptionChargeGenerated | `subscription.charge_generated` | `{ SubscriptionId, ChargeId, ProductId, Amount, Type, Currency, FromDate, ToDate, OccurredAt }` |
| SubscriptionBalanceAdjusted | `subscription.balance_adjusted` | `{ SubscriptionId, Amount, Reason, NewBalance, OccurredAt }` |

#### Subscribed Events

| Event | Source | Action |
|-------|--------|--------|
| `orders.order.completed` | Orders | Create subscription from order items |
| `billing.invoice.generated` | Billing | Mark charges as billed |
| `billing.invoice.paid` | Billing | Update subscription balance |
| `billing.invoice.failed` | Billing | Flag for suspension after threshold |
| `payments.payment.received` | Payments | Update prepaid balance |
| `rating.usage.rated` | Rating | Generate usage-based charges |
| `provisioning.service.activated` | Provisioning | Update subscription product status |
| `collections.debt.status_changed` | Collections | Suspend subscription |

### 5.6 Data Ownership

#### Owned Tables

| Table | Type | Purpose |
|-------|------|---------|
| `subscriptions` | Primary | Subscription records |
| `subscription_products` | Primary | Products on subscription |
| `subscription_charges` | Primary | Generated charges for billing |
| `subscription_terms` | Primary | Term definitions (commitment periods) |
| `subscription_status_history` | Secondary | Status change audit |

#### Foreign Key Relationships

```
subscriptions.tenant_id -> iam_tenants.id (RESTRICT)
subscriptions.customer_id -> crm_customers.id (RESTRICT)
subscriptions.order_id -> orders_orders.id (SET NULL)
subscription_products.subscription_id -> subscriptions.id (CASCADE)
subscription_products.product_id -> catalog_products.id (RESTRICT)
subscription_charges.subscription_id -> subscriptions.id (CASCADE)
subscription_terms.subscription_id -> subscriptions.id (CASCADE)
subscription_status_history.subscription_id -> subscriptions.id (CASCADE)
```

#### Indexing Strategy

```
subscriptions: (tenant_id, subscription_number) UNIQUE, (tenant_id, customer_id), (tenant_id, status), (tenant_id, next_billing_date)
subscription_products: (subscription_id, product_id), (product_id, tenant_id)
subscription_charges: (subscription_id, status), (subscription_id, type, from_date), (status, billed_at)
subscription_terms: (subscription_id, status), (subscription_id, end_date)
subscription_status_history: (subscription_id, changed_at DESC)
```

### 5.7 Dependencies

#### Depends On

| Module | Reason |
|--------|--------|
| IAM | Tenant context |
| CRM | Customer reference |
| ProductCatalog | Product reference for subscription products |
| Orders | Optional order reference for subscription origin |

#### Depended On By

| Module | Reason |
|--------|--------|
| Billing | Subscriptions are the basis for recurring billing |
| Rating | Usage-based rating charges reference subscriptions |
| Invoices | Subscription references on invoice line items |
| Provisioning | Subscription activation triggers provisioning |
| Reporting | Subscription metrics and analytics |
| Notifications | Subscription lifecycle notifications |
| Collections | Subscription suspension for collections |

### 5.8 Security Requirements

#### RBAC Roles & Permissions

```
Module-level:
  Subscription.Read     -- View subscriptions, charges, balance
  Subscription.Create   -- Create new subscriptions
  Subscription.Update   -- Modify subscriptions, manage products, cancel
  Subscription.Admin    -- Terminate, adjust balances, manage billing settings

Role defaults:
  Admin: All permissions
  SalesAgent: Subscription.Read, Subscription.Create
  SupportAgent: Subscription.Read, Subscription.Update (suspend/unsuspend)
  BillingAdmin: Subscription.Read, Subscription.Update, Subscription.Admin
  ReadOnly: Subscription.Read
```

#### Data Isolation Rules

- Subscriptions are tenant-scoped
- Customer can view only their own subscriptions
- Balance adjustments require two-factor auth for amounts above threshold
- Prepaid balances are read-only to non-admin roles
- Subscription charges are read-only after billing (no deletion, only credit)

#### Audit Requirements

- All status changes with reason and actor
- All product additions/removals
- All balance adjustments (amount, reason, before/after)
- Charge generation (who triggered, amount, period)
- Auto-renew setting changes
- Billing day changes
- Subscription termination processing

---

## 6. Rating — Usage Rating Engine

### 6.1 Domain Layer

#### Aggregate: RatingPlan

```
Id: Guid
TenantId: Guid
Name: string
Description: string?
Type: RatingPlanType           (Voice, Data, SMS, Generic, Bundle)
Status: RatingPlanStatus       (Draft, Active, Inactive)
Priority: int
Currency: string
ValidFrom: DateTime
ValidTo: DateTime?
ProductIds: List<Guid>?
CustomerSegmentIds: List<Guid>?
Tiers: List<RatingTier>
Rates: List<RatingRate>
BundleDefinitions: List<BundleDefinition>
CreatedAt: DateTime
UpdatedAt: DateTime

Methods:
  Activate()
  Deactivate()
  AddRate(RatingRate rate)
  RemoveRate(Guid rateId)
  AddTier(RatingTier tier)
  RemoveTier(Guid tierId)
  IsApplicable(Product product, Customer customer): bool
  RateUsage(UsageRecord usage): RatingResult
  ApplyBundles(UsageRecord usage): BundleApplicationResult
```

#### Entity: RatingTier

```
Id: Guid
RatingPlanId: Guid
Name: string
FromUnit: decimal
ToUnit: decimal?
UnitPrice: decimal
FlatFee: decimal?
Type: TierType           (Volume, Time, Event)
Sequence: int
```

#### Entity: RatingRate

```
Id: Guid
RatingPlanId: Guid
Name: string
UsageType: UsageType         (Voice, Data, Sms, Mms, Generic, Roaming, Video)
Direction: UsageDirection    (Inbound, Outbound, Both, Any)
UnitType: UnitType           (PerSecond, PerMinute, PerMB, PerGB, PerEvent, PerMessage)
UnitPrice: decimal
Currency: string
MinUnits: decimal?
MaxUnits: decimal?
Rounding: RoundingMode       (Up, Down, Nearest, NotRequired)
ConnectionFee: decimal?
IntervalCharging: IntervalCharging?
TimeOfDayRates: List<TimeOfDayRate>?
DestinationRates: List<DestinationRate>?
RoamingRates: List<RoamingRate>?
```

#### Value Object: IntervalCharging

```
IntervalSeconds: int
IntervalRate: decimal
InitialIntervalSeconds: int
InitialIntervalRate: decimal?
```

#### Value Object: TimeOfDayRate

```
DaysOfWeek: List<DayOfWeek>
StartTime: TimeOnly
EndTime: TimeOnly
UnitPrice: decimal
Priority: int
```

#### Value Object: DestinationRate

```
Prefix: string
MatchType: MatchType         (Exact, Prefix, Regex)
Description: string
UnitPrice: decimal
Priority: int
```

#### Value Object: RoamingRate

```
CountryCode: string
NetworkCode: string?
UnitPrice: decimal
Zone: string
```

#### Entity: BundleDefinition

```
Id: Guid
RatingPlanId: Guid
Name: string
UsageType: UsageType
IncludedUnits: decimal
Period: RecurrencePeriod
ResetOnRenew: bool
Rollover: bool
RolloverLimit: decimal?
OverageRate: decimal?
Sequence: int
```

#### Aggregate: UsageRecord

```
Id: Guid
TenantId: Guid
SubscriptionId: Guid
ProductId: Guid
CustomerId: Guid
ServiceId: Guid?
RecordType: UsageType
Direction: UsageDirection
Units: decimal
UnitType: UnitType
ChargeableUnits: decimal?
Amount: decimal?
Currency: string?
RatingPlanId: Guid?
NetworkFields: NetworkUsageFields
EventTimestamp: DateTime
RecordingTimestamp: DateTime
RatingTimestamp: DateTime?
RawRecord: string?
Status: UsageStatus               (Unrated, Rated, Invoiced, Disputed)
ErrorCode: string?
CorrelationId: string?

Methods:
  MarkRated(decimal amount, Guid ratingPlanId)
  MarkInvoiced()
  Adjust(decimal newAmount, string reason)
```

#### Value Object: NetworkUsageFields

```
CallerNumber: string?
CalleeNumber: string?
CallDuration: int?
DataVolumeBytes: long?
SmsCount: int?
MmsCount: int?
CellId: string?
Imsi: string?
Imei: string?
Apn: string?
RadioAccessTechnology: string?
DestinationCountry: string?
RoamingCountry: string?
RoamingNetwork: string?
IpAddress: string?
ServiceType: string?
```

#### Value Objects

```
RatingPlanType: enum { Voice, Data, SMS, MMS, Generic, Bundle, Roaming, Corporate }
RatingPlanStatus: enum { Draft, Active, Inactive }
UsageType: enum { Voice, Data, Sms, Mms, Generic, Roaming, Video, IoT, Vpn, Cloud }
UsageDirection: enum { Inbound, Outbound, Both, Any }
UnitType: enum { PerSecond, PerMinute, PerEvent, PerMessage, PerMB, PerGB, PerKB, PerToken }
RoundingMode: enum { Up, Down, Nearest, NotRequired }
TierType: enum { Volume, Time, Event, Value }
UsageStatus: enum { Received, Unrated, Rated, Invoiced, Disputed, Error }
MatchType: enum { Exact, Prefix, Regex }
```

#### Domain Events

```
UsageRecordReceivedEvent         { RecordId, SubscriptionId, ProductId, Units, UnitType, EventTimestamp, OccurredAt }
UsageRecordRatedEvent            { RecordId, CalculatedAmount, Currency, RatingPlanId, OccurredAt }
UsageRecordErroredEvent          { RecordId, ErrorCode, OccurredAt }
RatingPlanActivatedEvent        { PlanId, Name, OccurredAt }
RatingPlanDeactivatedEvent      { PlanId, Name, OccurredAt }
BundleExhaustedEvent            { SubscriberId, BundleId, UsageType, OccurredAt }
ThresholdReachedEvent           { SubscriberId, UsageType, Threshold, CurrentUsage, OccurredAt }
```

#### Domain Services

```
IRatingEngine:
  RateSingle(UsageRecord record, Subscription subscription): RatingResult
  RateBatch(List<UsageRecord> records, Subscription subscription): List<RatingResult>
  FindBestPlan(UsageRecord record, Subscription subscription): RatingPlan?
  ApplyBundles(UsageRecord record, Subscription subscription, RatingPlan plan): BundleResult

IUsageValidationService:
  ValidateUsageRecord(UsageRecord record): ValidationResult
  NormalizeUnits(UsageRecord record, UnitType targetUnit): decimal
  DetectDuplicates(UsageRecord record): bool

IBundleManagementService:
  GetBundleUsage(Subscription subscription, Guid bundleId): BundleUsageSummary
  CheckBundleStatus(Subscription subscription, Guid bundleDefId): BundleStatus
  CalculateOverage(Subscription subscription, UsageRecord record, BundleDefinition bundle): decimal
  ApplyRollover(Subscription subscription, Guid bundleDefId): decimal
```

#### Repository Interfaces

```
IRatingPlanRepository:
  GetByIdAsync(Guid id): RatingPlan?
  GetActiveAsync(Guid tenantId): List<RatingPlan>
  GetByProductIdAsync(Guid productId): List<RatingPlan>
  GetByTypeAsync(RatingPlanType type, Guid tenantId): List<RatingPlan>
  AddAsync(RatingPlan plan)
  UpdateAsync(RatingPlan plan)
  DeleteAsync(RatingPlan plan)
  GetApplicablePlansAsync(Subscription subscription, UsageType usageType): List<RatingPlan>

IUsageRecordRepository:
  GetByIdAsync(Guid id): UsageRecord?
  GetBySubscriptionIdAsync(Guid subscriptionId, UsageFilter filter): PagedResult<UsageRecord>
  GetUnratedAsync(Guid tenantId, int batchSize): List<UsageRecord>
  AddAsync(UsageRecord record)
  AddRangeAsync(List<UsageRecord> records)
  UpdateAsync(UsageRecord record)
  CountByStatusAsync(Guid tenantId): Dictionary<UsageStatus, int>
  GetUsageForBillingAsync(Guid subscriptionId, DateTime from, DateTime to): List<UsageRecord>
  DeleteOlderThanAsync(DateTime cutoff)

IBundleUsageRepository:
  GetBySubscriptionAsync(Guid subscriptionId, Guid bundleDefId): BundleUsage?
  GetBySubscriptionAllAsync(Guid subscriptionId): List<BundleUsage>
  AddOrUpdateAsync(BundleUsage usage)
```

#### Domain Rules / Invariants

- Rating plan names must be unique per tenant
- Only one active rating plan per product/usage type combination
- Rating plan tiers must not have overlapping ranges
- Usage records must have a valid subscription reference
- Usage records cannot be modified after invoicing
- Rating plans with type Bundle must have at least one bundle definition
- Rating tier sequences must be contiguous
- Destination rate prefixes must not overlap for the same priority
- Usage records older than 90 days cannot be re-rated (configurable)

### 6.2 Application Layer

#### Commands

```
CreateRatingPlanCommand     { TenantId, Name, Description, Type, Priority, Currency, ValidFrom, ValidTo, ProductIds, CustomerSegmentIds, Tiers, Rates, BundleDefinitions }
UpdateRatingPlanCommand     { PlanId, Name, Description, Priority, ValidTo }
ActivateRatingPlanCommand   { PlanId }
DeactivateRatingPlanCommand { PlanId }
AddRateCommand              { PlanId, Rate }
RemoveRateCommand           { PlanId, RateId }
AddTierCommand              { PlanId, Tier }
RemoveTierCommand           { PlanId, TierId }
AddBundleDefinitionCommand  { PlanId, BundleDefinition }
RemoveBundleDefinitionCommand { PlanId, BundleDefinitionId }
SubmitUsageRecordCommand    { TenantId, SubscriptionId, ProductId, ServiceId, RecordType, Direction, Units, UnitType, NetworkFields, EventTimestamp, CorrelationId }
SubmitUsageBatchCommand     { TenantId, Records[] }
ReRateUsageCommand          { RecordIds[] }
RatePendingUsageCommand     { TenantId }
AdjustUsageCommand          { RecordId, Units, Reason }
```

#### Queries

```
GetRatingPlanQuery          { PlanId }
ListRatingPlansQuery        { TenantId, Type, Status, ProductId, Page, Size }
GetUsageRecordQuery         { RecordId }
SearchUsageRecordsQuery     { TenantId, SubscriptionId, Status, Type, FromDate, ToDate, Page, Size }
GetUnratedCountQuery        { TenantId }
GetUsageStatsQuery          { TenantId, FromDate, ToDate }
GetBundleUsageQuery         { SubscriptionId }
GetUsageForInvoiceQuery     { SubscriptionId, FromDate, ToDate }
```

#### Command Handlers

```
CreateRatingPlanHandler     -> Validates dates, tiers, rates; creates plan; publishes RatingPlanCreated
ActivateRatingPlanHandler   -> Deactivates conflicting plans, activates, publishes RatingPlanActivatedEvent
SubmitUsageRecordHandler    -> Validates record, stores, publishes UsageRecordReceivedEvent
SubmitUsageBatchHandler     -> Batch validates and stores, publishes batch event
RatePendingUsageHandler     -> Fetches unrated records, applies rating engine, marks rated, publishes UsageRecordRatedEvent
ReRateUsageHandler          -> Validates re-rating window, re-rates specified records
AdjustUsageHandler          -> Adjusts usage units (only if unrated), publishes adjustment event
```

#### Query Handlers

```
GetRatingPlanHandler        -> Returns RatingPlanDetailDto
ListRatingPlansHandler      -> Returns PagedResult<RatingPlanSummaryDto>
GetUsageRecordHandler       -> Returns UsageRecordDto with rating details
SearchUsageRecordsHandler   -> Returns PagedResult<UsageRecordDto>
GetUsageForInvoiceHandler   -> Returns List<UsageChargeDto>
```

#### Application Services

```
IRatingOrchestrator:
  ProcessUsageRecord(UsageRecord record): RatingResult
  ProcessBatch(List<UsageRecord> records): BatchRatingResult
  GetBatchForRating(Guid tenantId, int size): List<UsageRecord>

IUsageFileParser:
  ParseCdrFile(Stream file, FileFormat format): ParseResult
  ParseRadiusAccounting(Stream file): ParseResult
  ParseUsageApiJson(string json): ParseResult

IUsageDeduplicationService:
  CheckForDuplicate(UsageRecord record): bool
  MarkAsDuplicate(UsageRecord record, Guid originalId)
  GetDuplicatesByCorrelationId(string correlationId): List<UsageRecord>
```

#### DTOs

```
RatingPlanSummaryDto     { Id, Name, Type, Status, Priority, Currency, ProductCount, TierCount, ValidFrom, ValidTo, CreatedAt }
RatingPlanDetailDto      { Id, Name, Description, Type, Status, Priority, Currency, ProductIds, CustomerSegmentIds, Tiers, Rates, BundleDefinitions, ValidFrom, ValidTo, CreatedAt }
RatingTierDto            { Id, Name, FromUnit, ToUnit, UnitPrice, FlatFee, Type, Sequence }
RatingRateDto            { Id, Name, UsageType, Direction, UnitType, UnitPrice, MinUnits, MaxUnits, Rounding, ConnectionFee, IntervalCharging, TimeOfDayRates, DestinationRates, RoamingRates }
BundleDefinitionDto      { Id, Name, UsageType, IncludedUnits, Period, Rollover, RolloverLimit, OverageRate, Sequence }
UsageRecordDto           { Id, SubscriptionId, ProductId, CustomerId, ServiceId, RecordType, Direction, Units, UnitType, ChargeableUnits, Amount, Currency, Status, RatingPlanId, EventTimestamp, RecordingTimestamp, RatingTimestamp, NetworkFields }
BundleUsageDto           { BundleDefinitionId, BundleName, TotalIncluded, UsedUnits, RolloverUnits, RemainingUnits, ResetDate, UsagePercentage }
UsageChargeDto           { RecordId, SubscriptionId, ProductId, UsageType, Units, UnitPrice, Amount, FromDate, ToDate, RatingPlanId }
```

### 6.3 Infrastructure Layer

#### EF Core Entity Configurations

```
RatingPlanConfiguration:
  Table: "rating_plans"
  HasIndex(rp => new { rp.TenantId, rp.Name }).IsUnique()
  HasIndex(rp => new { rp.TenantId, rp.Status })
  HasIndex(rp => new { rp.TenantId, rp.Type, rp.Priority })
  Property(rp => rp.ProductIds).HasColumnType("jsonb")
  Property(rp => rp.CustomerSegmentIds).HasColumnType("jsonb")
  HasMany(rp => rp.Tiers).WithOne().HasForeignKey(rt => rt.RatingPlanId).OnDelete(DeleteBehavior.Cascade)
  HasMany(rp => rp.Rates).WithOne().HasForeignKey(rr => rr.RatingPlanId).OnDelete(DeleteBehavior.Cascade)
  HasMany(rp => rp.BundleDefinitions).WithOne().HasForeignKey(bd => bd.RatingPlanId).OnDelete(DeleteBehavior.Cascade)

RatingRateConfiguration:
  Table: "rating_rates"
  OwnsOne(rr => rr.IntervalCharging)
  Property(rr => rr.TimeOfDayRates).HasColumnType("jsonb")
  Property(rr => rr.DestinationRates).HasColumnType("jsonb")
  Property(rr => rr.RoamingRates).HasColumnType("jsonb")

UsageRecordConfiguration:
  Table: "usage_records"
  HasIndex(ur => new { ur.TenantId, ur.Status })
  HasIndex(ur => new { ur.SubscriptionId, ur.EventTimestamp })
  HasIndex(ur => ur.CorrelationId).IsUnique().HasFilter("correlation_id IS NOT NULL")
  HasIndex(ur => ur.EventTimestamp)
  Property(ur => ur.NetworkFields).HasColumnType("jsonb")
  Property(ur => ur.RawRecord).HasColumnType("text")

BundleUsageConfiguration:
  Table: "rating_bundle_usage"
  HasIndex(bu => new { bu.SubscriptionId, bu.BundleDefinitionId }).IsUnique()
```

#### Repository Implementations

```
RatingPlanRepository      -> EF Core with tiers, rates, bundle definitions
UsageRecordRepository     -> EF Core with status/date filtering, batch operations
BundleUsageRepository     -> EF Core
```

#### Integration Events

**Publishes:**
```
UsageRecordReceivedEvent      -> Topic: "rating.usage.received"
UsageRecordRatedEvent         -> Topic: "rating.usage.rated"
UsageRecordErroredEvent       -> Topic: "rating.usage.errored"
RatingPlanActivatedEvent      -> Topic: "rating.plan.activated"
RatingPlanDeactivatedEvent    -> Topic: "rating.plan.deactivated"
BundleExhaustedEvent          -> Topic: "rating.bundle.exhausted"
ThresholdReachedEvent         -> Topic: "rating.threshold.reached"
```

**Subscribes to:**
```
provisioning.service.activated       -> Start usage recording for service
provisioning.service.deactivated     -> Stop usage recording for service
subscription.suspended               -> Queue pending usage records
subscription.terminated              -> Final rating of pending records
```

#### Background Jobs

```
UsageRatingJob:
  Schedule: Every 5 minutes
  Fetches unrated usage records, rates them, publishes results

UsageRecordCleanupJob:
  Schedule: Monthly
  Archives/removes usage records older than retention period

BundleResetJob:
  Schedule: Daily
  Resets bundle usage counters for new billing periods

RatingPlanExpiryJob:
  Schedule: Daily
  Deactivates expired rating plans
```

#### Caching Strategy

```
Cache: Active rating plans by tenant (Redis, 1 hour, evict on plan change)
Cache: Rating plan by ID (Redis, 1 hour, evict on plan change)
Cache: Destination rate prefix index (Redis, 1 hour)
Cache: Bundle usage by subscription (Redis, 15 minutes, evict on usage)
Cache: Unrated count by tenant (Redis, 5 minutes)
```

#### External Service Integrations

```
CDR Ingestion:
  - FTP/SFTP polling for CDR files
  - REST API for real-time usage events
  - Kafka/RabbitMQ for streaming usage data
  - RADIUS accounting protocol support

ENUM/Database Lookups:
  - Number portability database
  - Number prefix database for destination rating
```

### 6.4 API Layer

#### Endpoints

```
# Rating Plans
GET    /api/v1/rating/plans                           -> ListRatingPlans          [Admin, Rating.Read]
POST   /api/v1/rating/plans                           -> CreateRatingPlan         [Admin, Rating.Admin]
GET    /api/v1/rating/plans/{id}                      -> GetRatingPlan            [Admin, Rating.Read]
PUT    /api/v1/rating/plans/{id}                      -> UpdateRatingPlan         [Admin, Rating.Admin]
POST   /api/v1/rating/plans/{id}/activate             -> ActivateRatingPlan       [Admin, Rating.Admin]
POST   /api/v1/rating/plans/{id}/deactivate           -> DeactivateRatingPlan     [Admin, Rating.Admin]
POST   /api/v1/rating/plans/{id}/rates                -> AddRate                  [Admin, Rating.Admin]
DELETE /api/v1/rating/plans/{id}/rates/{rateId}       -> RemoveRate               [Admin, Rating.Admin]
POST   /api/v1/rating/plans/{id}/tiers                -> AddTier                  [Admin, Rating.Admin]
DELETE /api/v1/rating/plans/{id}/tiers/{tierId}       -> RemoveTier               [Admin, Rating.Admin]
POST   /api/v1/rating/plans/{id}/bundles              -> AddBundleDefinition      [Admin, Rating.Admin]
DELETE /api/v1/rating/plans/{id}/bundles/{bundleId}   -> RemoveBundleDefinition   [Admin, Rating.Admin]

# Usage Records
POST   /api/v1/rating/usage                           -> SubmitUsageRecord        [Admin, Rating.Create]
POST   /api/v1/rating/usage/batch                     -> SubmitUsageBatch         [Admin, Rating.Create]
GET    /api/v1/rating/usage/{id}                      -> GetUsageRecord           [Authenticated, Rating.Read]
GET    /api/v1/rating/usage                           -> SearchUsageRecords       [Authenticated, Rating.Read]
POST   /api/v1/rating/usage/rate                      -> RatePendingUsage         [Admin, Rating.Admin]
POST   /api/v1/rating/usage/rerate                    -> ReRateUsage              [Admin, Rating.Admin]
PATCH  /api/v1/rating/usage/{id}                      -> AdjustUsage              [Admin, Rating.Admin]
GET    /api/v1/rating/usage/unrated-count             -> GetUnratedCount          [Admin, Rating.Read]
GET    /api/v1/rating/usage/stats                     -> GetUsageStats            [Admin, Rating.Read]

# Bundle Usage
GET    /api/v1/rating/bundles/{subscriptionId}        -> GetBundleUsage           [Authenticated, Rating.Read]
```

#### Request/Response Models

```
CreateRatingPlanRequest  { Name, Description, Type, Priority, Currency, ValidFrom, ValidTo, ProductIds, CustomerSegmentIds, Tiers, Rates, BundleDefinitions }
RatingTierModel          { Name, FromUnit, ToUnit, UnitPrice, FlatFee, Type, Sequence }
RatingRateModel          { Name, UsageType, Direction, UnitType, UnitPrice, MinUnits, MaxUnits, Rounding, ConnectionFee, IntervalCharging, TimeOfDayRates, DestinationRates, RoamingRates }
BundleDefinitionModel    { Name, UsageType, IncludedUnits, Period, Rollover, RolloverLimit, OverageRate, Sequence }
SubmitUsageRequest       { SubscriptionId, ProductId, ServiceId, RecordType, Direction, Units, UnitType, NetworkFields, EventTimestamp, CorrelationId }
SubmitUsageBatchRequest  { Records: List<SubmitUsageRequest> }
AdjustUsageRequest       { Units, Reason }
ReRateRequest            { RecordIds[] }
```

#### Authorization Requirements

```
Rating.Read: View rating plans, usage records, bundle usage
Rating.Create: Submit usage records, batches
Rating.Admin: Create/manage rating plans, rate/re-rate usage, adjust records
```

#### Rate Limiting

```
Usage submission: 1000 requests/minute (high volume expected)
Batch usage submission: 10 requests/minute (10000 records max per batch)
Rating plan operations: 20 requests/minute
Usage queries: 30 requests/minute per user
```

#### Versioning Strategy

```
URL-based: /api/v1/rating
```

### 6.5 Events

#### Published Events

| Event | Topic | Schema |
|-------|-------|--------|
| UsageRecordReceived | `rating.usage.received` | `{ RecordId, SubscriptionId, ProductId, Units, UnitType, EventTimestamp, OccurredAt }` |
| UsageRecordRated | `rating.usage.rated` | `{ RecordId, SubscriptionId, ProductId, Amount, Currency, RatingPlanId, Units, UnitPrice, OccurredAt }` |
| UsageRecordErrored | `rating.usage.errored` | `{ RecordId, ErrorCode, Description, OccurredAt }` |
| RatingPlanActivated | `rating.plan.activated` | `{ PlanId, Name, TenantId, Type, OccurredAt }` |
| RatingPlanDeactivated | `rating.plan.deactivated` | `{ PlanId, Name, TenantId, OccurredAt }` |
| BundleExhausted | `rating.bundle.exhausted` | `{ SubscriptionId, BundleDefId, UsageType, IncludedUnits, OccurredAt }` |
| ThresholdReached | `rating.threshold.reached` | `{ SubscriptionId, UsageType, Threshold, CurrentUsage, OccurredAt }` |

#### Subscribed Events

| Event | Source | Action |
|-------|--------|--------|
| `provisioning.service.activated` | Provisioning | Enable usage recording for service |
| `provisioning.service.deactivated` | Provisioning | Disable usage recording for service |
| `subscription.suspended` | Subscriptions | Queue pending usage records |
| `subscription.terminated` | Subscriptions | Final rating of pending records |

### 6.6 Data Ownership

#### Owned Tables

| Table | Type | Purpose |
|-------|------|---------|
| `rating_plans` | Primary | Rating plan definitions |
| `rating_rates` | Primary | Rating rates with pricing rules |
| `rating_tiers` | Primary | Rating tier definitions |
| `rating_bundle_definitions` | Primary | Bundle definitions |
| `usage_records` | Primary | Usage/CDR records (high volume) |
| `rating_bundle_usage` | Secondary | Bundle consumption tracking |

#### Foreign Key Relationships

```
rating_plans.tenant_id -> iam_tenants.id (RESTRICT)
rating_rates.rating_plan_id -> rating_plans.id (CASCADE)
rating_tiers.rating_plan_id -> rating_plans.id (CASCADE)
rating_bundle_definitions.rating_plan_id -> rating_plans.id (CASCADE)
usage_records.tenant_id -> iam_tenants.id (RESTRICT)
usage_records.subscription_id -> subscriptions.id (RESTRICT)
usage_records.product_id -> catalog_products.id (RESTRICT)
```

#### Indexing Strategy

```
rating_plans: (tenant_id, name) UNIQUE, (tenant_id, status), (tenant_id, type, priority)
usage_records: (tenant_id, status), (subscription_id, event_timestamp DESC), correlation_id UNIQUE, event_timestamp
usage_records: PARTITION BY RANGE (event_timestamp) for monthly partitions
rating_bundle_usage: (subscription_id, bundle_definition_id) UNIQUE
```

### 6.7 Dependencies

#### Depends On

| Module | Reason |
|--------|--------|
| IAM | Tenant context |
| Subscriptions | Subscription reference on usage records |
| ProductCatalog | Product reference on usage records |

#### Depended On By

| Module | Reason |
|--------|--------|
| Billing | Rated usage feeds into billing engine |
| Invoices | Usage charges appear on invoices |
| Notifications | Threshold alerts for usage |
| Reporting | Usage analytics and revenue reporting |

### 6.8 Security Requirements

#### RBAC Roles & Permissions

```
Module-level:
  Rating.Read     -- View rating plans, usage records, bundle usage
  Rating.Create   -- Submit usage records and batches
  Rating.Admin    -- Manage rating plans, rate/re-rate, adjust records

Role defaults:
  Admin: All permissions
  BillingAdmin: Rating.Read, Rating.Admin
  SupportAgent: Rating.Read
  ReadOnly: Rating.Read
```

#### Data Isolation Rules

- Rating plans and usage records are tenant-scoped
- Usage records are partitioned by date for performance
- CDR data may contain PII (phone numbers, IMSI, IMEI) -- encrypted at rest
- Usage data access logged separately for privacy compliance
- Bulk usage submission restricted to system integrations (API key auth)

#### Audit Requirements

- Rating plan creation, activation, deactivation
- Rate/tier/bundle definition changes
- Usage record adjustments and re-rating
- Bulk usage submissions (who submitted, record count, time)
- Threshold configuration changes

---

## 7. Billing — Billing Engine

### 7.1 Domain Layer

#### Aggregate: BillingAccount

```
Id: Guid
TenantId: Guid
CustomerId: Guid
AccountNumber: string
Name: string
Type: BillingAccountType        (Individual, Corporate, Prepaid, Postpaid)
Status: BillingAccountStatus    (Active, Inactive, Frozen, Closed)
Currency: string
BillingCycle: BillingCycleType  (Monthly, Quarterly, Yearly, Weekly, Custom)
BillingDay: int
PaymentTerms: PaymentTerms
TaxId: string?
TaxExempt: bool
TaxExemptionReason: string?
BillingAddressId: Guid?
Email: string?
InvoiceDelivery: InvoiceDeliveryMethod (Email, Post, Portal, Both)
CreditLimit: decimal?
CurrentBalance: decimal
OutstandingBalance: decimal
LastInvoiceDate: DateTime?
NextInvoiceDate: DateTime?
DunningLevel: int
ParentAccountId: Guid?
SubAccounts: List<BillingAccount>?

Methods:
  AddBalance(decimal amount, string reason)
  DeductBalance(decimal amount, string reason)
  UpdateBillingCycle(BillingCycleType cycle, int billingDay)
  Freeze()
  Unfreeze()
  Close()
  CalculateNextInvoiceDate(): DateTime
  CanBeBilled(): bool
  IsOverdue(): bool
  DaysOverdue(): int
```

#### Entity: BillingPeriod

```
Id: Guid
BillingAccountId: Guid
SubscriptionId: Guid?
PeriodStart: DateTime
PeriodEnd: DateTime
Status: BillingPeriodStatus      (Open, Closed, Adjusted)
TotalAmount: decimal
Currency: string
IsInvoiced: bool
InvoicedAt: DateTime?
```

#### Value Objects

```
BillingAccountType: enum { Individual, Corporate, Prepaid, Postpaid }
BillingAccountStatus: enum { Active, Inactive, Frozen, Closed }
BillingCycleType: enum { Weekly, Monthly, Quarterly, Yearly, Custom }
BillingPeriodStatus: enum { Open, Closed, Adjusted }
InvoiceDeliveryMethod: enum { Email, Post, Portal, Both }

PaymentTerms:
  NetDays: int               (e.g., Net30)
  DueOnDay: int?             (specific day of month)
  DiscountPercentage: decimal?
  DiscountDays: int?
  LateFeePercentage: decimal?
  LateFeeFlat: decimal?
  GracePeriodDays: int
```

#### Domain Events

```
BillingAccountCreatedEvent          { AccountId, CustomerId, TenantId, AccountNumber, OccurredAt }
BillingAccountStatusChangedEvent    { AccountId, OldStatus, NewStatus, OccurredAt }
BillingPeriodOpenedEvent            { PeriodId, AccountId, PeriodStart, PeriodEnd, OccurredAt }
BillingPeriodClosedEvent            { PeriodId, AccountId, TotalAmount, OccurredAt }
BillingRunStartedEvent              { TenantId, PeriodStart, PeriodEnd, AccountCount, OccurredAt }
BillingRunCompletedEvent            { TenantId, PeriodStart, PeriodEnd, TotalBilled, SuccessCount, FailureCount, OccurredAt }
BalanceChangedEvent                 { AccountId, OldBalance, NewBalance, Amount, Reason, OccurredAt }
CreditLimitExceededEvent            { AccountId, CurrentBalance, CreditLimit, OccurredAt }
DunningLevelChangedEvent            { AccountId, OldLevel, NewLevel, OccurredAt }
```

#### Domain Services

```
IBillingCycleService:
  OpenBillingPeriod(BillingAccount account, DateTime periodStart, DateTime periodEnd): BillingPeriod
  CloseBillingPeriod(BillingPeriod period): Result
  CalculateBillingDate(BillingAccount account, DateTime referenceDate): DateTime

IBillingCalculationService:
  CalculateInvoiceAmount(BillingAccount account, BillingPeriod period): InvoiceCalculation
  AggregateCharges(List<SubscriptionCharge> charges): AggregatedCharges
  ApplyTaxes(InvoiceCalculation calculation): TaxCalculationResult
  CalculateLateFees(BillingAccount account): decimal
  ApplyDiscounts(BillingAccount account, decimal amount): DiscountedAmount

IBillingRunService:
  ExecuteBillingRun(Guid tenantId, DateTime billingDate): BillingRunResult
  ValidateBillingRunPreconditions(): ValidationResult
  GenerateDunningActions(BillingAccount account): List<DunningAction>
```

#### Repository Interfaces

```
IBillingAccountRepository:
  GetByIdAsync(Guid id): BillingAccount?
  GetByCustomerIdAsync(Guid customerId): List<BillingAccount>
  GetByAccountNumberAsync(string accountNumber, Guid tenantId): BillingAccount?
  GetDueForBillingAsync(Guid tenantId, DateTime billingDate): List<BillingAccount>
  GetOverdueAsync(Guid tenantId): List<BillingAccount>
  GetPagedAsync(Guid tenantId, BillingAccountFilter filter): PagedResult<BillingAccount>
  AddAsync(BillingAccount account)
  UpdateAsync(BillingAccount account)
  CountByStatusAsync(Guid tenantId): Dictionary<BillingAccountStatus, int>

IBillingPeriodRepository:
  GetByIdAsync(Guid id): BillingPeriod?
  GetByAccountIdAsync(Guid accountId, BillingPeriodFilter filter): PagedResult<BillingPeriod>
  GetOpenByAccountAsync(Guid accountId): BillingPeriod?
  AddAsync(BillingPeriod period)
  UpdateAsync(BillingPeriod period)
  GetByDateRangeAsync(Guid accountId, DateTime from, DateTime to): List<BillingPeriod>
```

#### Domain Rules / Invariants

- Account number must be unique per tenant
- A customer can have multiple billing accounts
- Open billing period must exist before charges can be applied
- Only one open billing period per account at a time
- Billing day must be between 1-28
- Primary billing account cannot be closed while sub-accounts exist
- Cannot freeze a billing account with overdue balance
- Billing run cannot execute if previous run for same period is incomplete
- Credit limit exceeded triggers dunning process

### 7.2 Application Layer

#### Commands

```
CreateBillingAccountCommand     { TenantId, CustomerId, Name, Type, Currency, BillingCycle, BillingDay, PaymentTerms, BillingAddressId, InvoiceDelivery, CreditLimit, ParentAccountId }
UpdateBillingAccountCommand     { AccountId, Name, PaymentTerms, BillingAddressId, InvoiceDelivery, CreditLimit }
FreezeBillingAccountCommand     { AccountId, Reason }
UnfreezeBillingAccountCommand   { AccountId }
CloseBillingAccountCommand      { AccountId, Reason }
UpdateBillingCycleCommand       { AccountId, BillingCycle, BillingDay }
OpenBillingPeriodCommand        { AccountId, PeriodStart, PeriodEnd }
CloseBillingPeriodCommand       { PeriodId }
ExecuteBillingRunCommand        { TenantId, BillingDate }
ExecuteSingleBillingCommand     { AccountId }
ApplyCreditCommand              { AccountId, Amount, Reason }
ApplyDebitCommand               { AccountId, Amount, Reason }
ApplyAdjustmentCommand          { AccountId, Amount, Description, Type }
UpdatePaymentTermsCommand       { AccountId, PaymentTerms }
SetCreditLimitCommand           { AccountId, CreditLimit }
BulkDunningCommand              { TenantId }
```

#### Queries

```
GetBillingAccountQuery          { AccountId }
GetBillingAccountByNumberQuery  { AccountNumber, TenantId }
ListBillingAccountsQuery        { TenantId, CustomerId, Status, Type, Page, Size }
SearchBillingAccountsQuery      { TenantId, SearchTerm, Page, Size }
GetCustomerBillingAccountsQuery { CustomerId }
GetBillingPeriodsQuery          { AccountId, Page, Size }
GetOpenBillingPeriodQuery       { AccountId }
GetBillingAccountBalanceQuery   { AccountId }
GetBillingAccountSummaryQuery   { AccountId }
GetBillingStatsQuery            { TenantId }
GetOverdueAccountsQuery         { TenantId }
GetAccountsDueForBillingQuery   { TenantId, BillingDate }
```

#### Command Handlers

```
CreateBillingAccountHandler     -> Validates, creates account, publishes BillingAccountCreatedEvent
UpdateBillingAccountHandler     -> Validates and updates
FreezeBillingAccountHandler     -> Validates no overdue balance, freezes, publishes BillingAccountStatusChangedEvent
ExecuteBillingRunHandler        -> Orchestrates billing for all due accounts, publishes BillingRunStarted/CompletedEvents
ExecuteSingleBillingHandler     -> Bills single account, triggers invoice generation
ApplyCreditHandler               -> Adjusts balance, publishes BalanceChangedEvent
ApplyDebitHandler                -> Adjusts balance, publishes BalanceChangedEvent
BulkDunningHandler               -> Applies dunning level changes to overdue accounts
```

#### Query Handlers

```
GetBillingAccountHandler        -> Returns BillingAccountDetailDto
ListBillingAccountsHandler      -> Returns PagedResult<BillingAccountSummaryDto>
GetCustomerBillingAccountsHandler -> Returns List<BillingAccountSummaryDto>
GetBillingPeriodsHandler        -> Returns PagedResult<BillingPeriodDto>
GetBillingAccountBalanceHandler -> Returns BalanceDto
GetBillingStatsHandler          -> Returns BillingStatsDto (total AR, overdue %, average invoice)
```

#### Application Services

```
IBillingOrchestrator:
  ExecuteBillingRunAsync(Guid tenantId, DateTime billingDate): BillingRunResult
  ExecuteSingleAccountBillingAsync(Guid accountId): SingleBillingResult
  PrepareInvoiceData(BillingAccount account, BillingPeriod period): InvoiceData

IInvoiceGenerationService:
  GenerateInvoice(BillingAccount account, BillingPeriod period): Invoice
  GenerateInvoicePdf(Invoice invoice): byte[]
  PreviewInvoice(BillingAccount account, BillingPeriod period): InvoicePreview
  RegenerateInvoice(Guid invoiceId): Invoice

ITaxCalculationService:
  CalculateTax(BillingAccount account, decimal amount, string taxCode): TaxResult
  ValidateTaxConfiguration(BillingAccount account): ValidationResult
  GetApplicableTaxRates(Address billingAddress): List<TaxRate>
```

#### DTOs

```
BillingAccountSummaryDto  { Id, AccountNumber, CustomerId, CustomerName, Type, Status, Currency, CurrentBalance, OutstandingBalance, NextInvoiceDate, DaysOverdue, DunningLevel }
BillingAccountDetailDto   { Id, AccountNumber, CustomerId, CustomerName, Type, Status, Currency, BillingCycle, BillingDay, PaymentTerms, TaxId, TaxExempt, CreditLimit, CurrentBalance, OutstandingBalance, LastInvoiceDate, NextInvoiceDate, DunningLevel, ParentAccountId, SubAccounts, CreatedAt }
BillingPeriodDto          { Id, AccountId, PeriodStart, PeriodEnd, Status, TotalAmount, Currency, IsInvoiced, InvoicedAt }
BalanceDto                { CurrentBalance, OutstandingBalance, Currency, AvailableCredit, PendingCharges, LastTransactionDate }
BillingStatsDto           { TotalAR, OverdueAR, OverduePercentage, TotalAccounts, ActiveAccounts, AverageInvoiceAmount, TotalBilledThisMonth, DunningAccounts }
BillingRunResultDto       { RunId, TenantId, PeriodStart, PeriodEnd, TotalAccounts, SuccessCount, FailureCount, TotalAmount, Duration, Errors }
```

### 7.3 Infrastructure Layer

#### EF Core Entity Configurations

```
BillingAccountConfiguration:
  Table: "billing_accounts"
  HasIndex(ba => new { ba.TenantId, ba.AccountNumber }).IsUnique()
  HasIndex(ba => ba.CustomerId)
  HasIndex(ba => new { ba.TenantId, ba.Status })
  HasIndex(ba => ba.NextInvoiceDate)
  HasIndex(ba => ba.ParentAccountId)
  Property(ba => ba.PaymentTerms).HasColumnType("jsonb")
  HasMany(ba => ba.Periods).WithOne().HasForeignKey(bp => bp.BillingAccountId).OnDelete(DeleteBehavior.Cascade)
  HasMany(ba => ba.SubAccounts).WithOne().HasForeignKey(ba => ba.ParentAccountId)

BillingPeriodConfiguration:
  Table: "billing_periods"
  HasIndex(bp => bp.BillingAccountId)
  HasIndex(bp => new { bp.BillingAccountId, bp.Status })
  HasIndex(bp => new { bp.PeriodStart, bp.PeriodEnd })
```

#### Repository Implementations

```
BillingAccountRepository   -> EF Core with periods, sub-accounts; filtering by status, due date
BillingPeriodRepository     -> EF Core
```

#### Integration Events

**Publishes:**
```
BillingAccountCreatedEvent        -> Topic: "billing.account.created"
BillingAccountStatusChangedEvent  -> Topic: "billing.account.status_changed"
BillingPeriodOpenedEvent          -> Topic: "billing.period.opened"
BillingPeriodClosedEvent          -> Topic: "billing.period.closed"
BillingRunStartedEvent            -> Topic: "billing.run.started"
BillingRunCompletedEvent          -> Topic: "billing.run.completed"
BalanceChangedEvent               -> Topic: "billing.balance.changed"
CreditLimitExceededEvent          -> Topic: "billing.credit_limit.exceeded"
DunningLevelChangedEvent          -> Topic: "billing.dunning_level.changed"
```

**Subscribes to:**
```
subscription.charge_generated       -> Add charges to open billing period
subscription.subscription.activated -> Create billing account if not exists
subscription.subscription.suspended -> Recalculate billing status
orders.order.completed              -> Create billing account from order
payments.payment.received           -> Update account balance
crm.customer.status_changed         -> Update billing account status
subscription.subscription.balance_adjusted -> Sync balance
```

#### Background Jobs

```
BillingRunJob:
  Schedule: Daily (configurable)
  Executes billing run for accounts due for billing
  Generates invoices

OverdueAccountJob:
  Schedule: Daily
  Applies dunning actions to overdue accounts
  Sends overdue notifications

BillingAccountStatusJob:
  Schedule: Daily
  Freezes accounts after X days overdue
  Closes accounts after X days frozen

LateFeeJob:
  Schedule: Daily
  Applies late fees to overdue accounts per payment terms
```

#### Caching Strategy

```
Cache: Billing account by ID (Redis, 15 minutes, evict on change)
Cache: Billing account by number (Redis, 15 minutes, evict on change)
Cache: Customer billing accounts (Redis, 30 minutes, evict on change)
Cache: Billing account balances (Redis, 5 minutes)
Cache: Billing runs in progress (Redis, to prevent duplicate runs)
```

#### External Service Integrations

```
Tax Calculation Service (Avalara, Vertex):
  - Real-time tax calculation
  - Tax exemption validation
  - Tax filing reports

Credit Rating Service:
  - Credit limit validation
  - Credit score checks for credit limit increases
```

### 7.4 API Layer

#### Endpoints

```
GET    /api/v1/billing/accounts                           -> ListBillingAccounts          [Authenticated, Billing.Read]
POST   /api/v1/billing/accounts                           -> CreateBillingAccount         [Authenticated, Billing.Create]
GET    /api/v1/billing/accounts/{id}                      -> GetBillingAccount            [Authenticated, Billing.Read]
PUT    /api/v1/billing/accounts/{id}                      -> UpdateBillingAccount         [Authenticated, Billing.Update]
POST   /api/v1/billing/accounts/{id}/freeze               -> FreezeBillingAccount         [Admin, Billing.Admin]
POST   /api/v1/billing/accounts/{id}/unfreeze             -> UnfreezeBillingAccount       [Admin, Billing.Admin]
POST   /api/v1/billing/accounts/{id}/close                -> CloseBillingAccount          [Admin, Billing.Admin]
PATCH  /api/v1/billing/accounts/{id}/cycle                -> UpdateBillingCycle           [Admin, Billing.Update]
PATCH  /api/v1/billing/accounts/{id}/payment-terms        -> UpdatePaymentTerms           [Admin, Billing.Admin]
PUT    /api/v1/billing/accounts/{id}/credit-limit         -> SetCreditLimit               [Admin, Billing.Admin]
GET    /api/v1/billing/accounts/{id}/balance              -> GetBillingAccountBalance     [Authenticated, Billing.Read]
GET    /api/v1/billing/accounts/{id}/periods              -> GetBillingPeriods            [Authenticated, Billing.Read]
POST   /api/v1/billing/accounts/{id}/periods              -> OpenBillingPeriod            [Admin, Billing.Admin]
POST   /api/v1/billing/accounts/{id}/credit               -> ApplyCredit                  [Admin, Billing.Admin]
POST   /api/v1/billing/accounts/{id}/debit                -> ApplyDebit                   [Admin, Billing.Admin]
POST   /api/v1/billing/accounts/{id}/adjustment           -> ApplyAdjustment              [Admin, Billing.Admin]
GET    /api/v1/billing/accounts/{id}/summary              -> GetBillingAccountSummary     [Authenticated, Billing.Read]
GET    /api/v1/billing/accounts/by-number/{number}        -> GetBillingAccountByNumber    [Authenticated, Billing.Read]

GET    /api/v1/billing/customers/{customerId}/accounts    -> GetCustomerBillingAccounts   [Authenticated, Billing.Read]

POST   /api/v1/billing/run                                -> ExecuteBillingRun            [Admin, Billing.Admin]
POST   /api/v1/billing/run/single                         -> ExecuteSingleBilling         [Admin, Billing.Admin]
POST   /api/v1/billing/dunning                            -> BulkDunning                  [Admin, Billing.Admin]

GET    /api/v1/billing/stats                              -> GetBillingStats              [Admin, Billing.Read]
GET    /api/v1/billing/overdue                            -> GetOverdueAccounts           [Admin, Billing.Read]
GET    /api/v1/billing/due                                -> GetAccountsDueForBilling     [Admin, Billing.Read]
```

#### Request/Response Models

```
CreateBillingAccountRequest { CustomerId, Name, Type, Currency, BillingCycle, BillingDay, PaymentTerms, BillingAddressId, InvoiceDelivery, CreditLimit, ParentAccountId }
UpdateBillingAccountRequest { Name, PaymentTerms, BillingAddressId, InvoiceDelivery, CreditLimit }
FreezeAccountRequest        { Reason }
CloseAccountRequest         { Reason }
PaymentTermsModel           { NetDays, DueOnDay, DiscountPercentage, DiscountDays, LateFeePercentage, LateFeeFlat, GracePeriodDays }
ApplyAdjustmentRequest      { Amount, Description, Type }
SetCreditLimitRequest       { CreditLimit }
ExecuteBillingRunRequest    { BillingDate (optional, defaults to today) }
```

#### Authorization Requirements

```
Billing.Read: View billing accounts, balances, periods
Billing.Create: Create billing accounts
Billing.Update: Update billing account details, change cycle
Billing.Admin: Freeze/unfreeze/close, adjust balances, execute billing runs, dunning
```

#### Rate Limiting

```
Billing account operations: 30 requests/minute
Balance adjustments: 10 requests/minute
Billing run execution: 1 request/5 minutes (prevent concurrent runs)
Billing queries: 30 requests/minute
```

#### Versioning Strategy

```
URL-based: /api/v1/billing
```

### 7.5 Events

#### Published Events

| Event | Topic | Schema |
|-------|-------|--------|
| BillingAccountCreated | `billing.account.created` | `{ AccountId, AccountNumber, CustomerId, TenantId, Type, Currency, BillingCycle, OccurredAt }` |
| BillingAccountStatusChanged | `billing.account.status_changed` | `{ AccountId, CustomerId, TenantId, OldStatus, NewStatus, Reason, OccurredAt }` |
| BillingPeriodOpened | `billing.period.opened` | `{ PeriodId, AccountId, PeriodStart, PeriodEnd, OccurredAt }` |
| BillingPeriodClosed | `billing.period.closed` | `{ PeriodId, AccountId, TotalAmount, Currency, OccurredAt }` |
| BillingRunCompleted | `billing.run.completed` | `{ TenantId, PeriodStart, PeriodEnd, TotalAmount, InvoicesGenerated, SuccessCount, FailureCount, OccurredAt }` |
| BalanceChanged | `billing.balance.changed` | `{ AccountId, OldBalance, NewBalance, Amount, Currency, Reason, OccurredAt }` |
| CreditLimitExceeded | `billing.credit_limit.exceeded` | `{ AccountId, CurrentBalance, CreditLimit, Currency, OccurredAt }` |
| DunningLevelChanged | `billing.dunning_level.changed` | `{ AccountId, OldLevel, NewLevel, OccurredAt }` |

#### Subscribed Events

| Event | Source | Action |
|-------|--------|--------|
| `subscription.charge_generated` | Subscriptions | Add charge to open billing period |
| `subscription.activated` | Subscriptions | Ensure billing account exists |
| `orders.order.completed` | Orders | Create billing account if needed |
| `payments.payment.received` | Payments | Update account balance |
| `crm.customer.status_changed` | CRM | Update billing account status |
| `rating.usage.rated` | Rating | Add usage charges to billing period |

### 7.6 Data Ownership

#### Owned Tables

| Table | Type | Purpose |
|-------|------|---------|
| `billing_accounts` | Primary | Billing account records |
| `billing_periods` | Primary | Billing period tracking |
| `billing_adjustments` | Secondary | Manual debits/credits |
| `billing_runs` | Secondary | Billing run audit trail |
| `billing_dunning_actions` | Secondary | Dunning action records |

#### Foreign Key Relationships

```
billing_accounts.tenant_id -> iam_tenants.id (RESTRICT)
billing_accounts.customer_id -> crm_customers.id (RESTRICT)
billing_accounts.parent_account_id -> billing_accounts.id (RESTRICT)
billing_accounts.billing_address_id -> crm_addresses.id (SET NULL)
billing_periods.billing_account_id -> billing_accounts.id (CASCADE)
billing_adjustments.account_id -> billing_accounts.id (CASCADE)
billing_runs.tenant_id -> iam_tenants.id (RESTRICT)
billing_dunning_actions.account_id -> billing_accounts.id (CASCADE)
```

#### Indexing Strategy

```
billing_accounts: (tenant_id, account_number) UNIQUE, (tenant_id, customer_id), (tenant_id, status), (tenant_id, next_invoice_date), (tenant_id, dunning_level)
billing_periods: (billing_account_id, status), (billing_account_id, period_start DESC)
billing_adjustments: (account_id, created_at DESC)
billing_runs: (tenant_id, created_at DESC)
billing_dunning_actions: (account_id, created_at DESC), (account_id, action_type)
```

### 7.7 Dependencies

#### Depends On

| Module | Reason |
|--------|--------|
| IAM | Tenant context |
| CRM | Customer reference |
| Subscriptions | Charge data for billing |
| Invoices | Invoice generation from billing periods |

#### Depended On By

| Module | Reason |
|--------|--------|
| Invoices | Billing triggers invoice generation |
| Collections | Overdue accounts feeding into collections |
| Reporting | Billing analytics and AR reporting |
| Notifications | Billing and overdue notifications |
| Payments | Payment allocation to invoices |

### 7.8 Security Requirements

#### RBAC Roles & Permissions

```
Module-level:
  Billing.Read     -- View billing accounts, balances, periods, runs
  Billing.Create   -- Create billing accounts
  Billing.Update   -- Modify billing account settings, change cycles
  Billing.Admin    -- Freeze/unfreeze/close, adjust balances, execute runs, dunning

Role defaults:
  Admin: All permissions
  BillingAdmin: All permissions
  SupportAgent: Billing.Read
  SalesAgent: Billing.Read
  ReadOnly: Billing.Read
```

#### Data Isolation Rules

- Billing accounts are tenant-scoped
- Financial adjustments require two-factor approval above threshold
- Credit limit changes logged with reason and approver
- Balance adjustments immutable after posting (reversal entries only)
- Billing runs are idempotent (prevent duplicate billing)

#### Audit Requirements

- All financial adjustments (credit/debit) with amount, reason, approver
- Billing run execution (who triggered, when, results)
- Billing account status changes (freeze, unfreeze, close)
- Credit limit changes (old limit, new limit, reason)
- Payment term changes
- Dunning level changes and actions taken

---

## 8. Invoices — Invoice Management

### 8.1 Domain Layer

#### Aggregate: Invoice

```
Id: Guid
TenantId: Guid
InvoiceNumber: string
BillingAccountId: Guid
CustomerId: Guid
Status: InvoiceStatus          (Draft, Generated, Sent, Paid, Partial, Overdue, Cancelled, CreditNote)
Type: InvoiceType              (Standard, CreditNote, DebitNote, Proforma, Corrective)
Currency: string
IssueDate: DateTime
DueDate: DateTime
PaidDate: DateTime?
CancelledDate: DateTime?
Lines: List<InvoiceLine>
Payments: List<InvoicePayment>
TaxAmount: decimal
DiscountAmount: decimal
Subtotal: decimal
TotalAmount: decimal
AmountDue: decimal
AmountPaid: decimal
BillingPeriodStart: DateTime?
BillingPeriodEnd: DateTime?
PdfUrl: string?
HtmlUrl: string?
Notes: string?
InternalNotes: string?
PoNumber: string?              (purchase order reference)
BillingAddress: AddressSnapshot
PaymentTerms: PaymentTerms

Methods:
  Generate()
  Send()
  RecordPayment(decimal amount, Guid paymentId)
  MarkOverdue()
  Cancel(string reason)
  IssueCreditNote(decimal amount, string reason)
  IsFullyPaid(): bool
  IsOverdue(): bool
  CalculateRemainingBalance(): decimal
  CanBeCancelled(): bool
```

#### Entity: InvoiceLine

```
Id: Guid
InvoiceId: Guid
LineNumber: int
Description: string
ProductId: Guid?
ProductName: string?
SubscriptionId: Guid?
SubscriptionProductId: Guid?
ChargeId: Guid?                  (reference to SubscriptionCharge)
Type: InvoiceLineType            (Recurring, Usage, OneTime, Adjustment, Tax, Discount, Credit)
Quantity: int
UnitPrice: decimal
UnitOfMeasure: string?
DiscountAmount: decimal
DiscountPercentage: decimal?
TaxAmount: decimal
TaxRate: decimal
TaxCode: string?
TotalPrice: decimal
Currency: string
StartDate: DateTime?
EndDate: DateTime?
```

#### Entity: InvoicePayment

```
Id: Guid
InvoiceId: Guid
PaymentId: Guid
Amount: decimal
Currency: string
AllocatedAt: DateTime
```

#### Value Object: AddressSnapshot

```
Line1: string
Line2: string?
City: string
State: string?
PostalCode: string
Country: string
```

#### Value Objects

```
InvoiceStatus: enum { Draft, Generated, Sent, Paid, Partial, Overdue, Cancelled, CreditNote }
InvoiceType: enum { Standard, CreditNote, DebitNote, Proforma, Corrective }
InvoiceLineType: enum { Recurring, Usage, OneTime, Setup, Adjustment, Tax, Discount, Credit, LateFee }
```

#### Domain Events

```
InvoiceGeneratedEvent        { InvoiceId, InvoiceNumber, CustomerId, BillingAccountId, TotalAmount, Currency, IssueDate, DueDate, OccurredAt }
InvoiceSentEvent             { InvoiceId, SentAt, Method, OccurredAt }
InvoicePaidEvent             { InvoiceId, AmountPaid, PaymentDate, OccurredAt }
InvoicePartiallyPaidEvent    { InvoiceId, AmountPaid, RemainingBalance, OccurredAt }
InvoiceOverdueEvent          { InvoiceId, DaysOverdue, OccurredAt }
InvoiceCancelledEvent        { InvoiceId, Reason, OccurredAt }
InvoiceCreditNoteIssuedEvent { InvoiceId, CreditNoteId, Amount, Reason, OccurredAt }
InvoiceReminderSentEvent     { InvoiceId, ReminderType, OccurredAt }
```

#### Domain Services

```
IInvoiceNumberGenerator:
  GenerateAsync(Guid tenantId, InvoiceType type): string

IInvoiceDocumentService:
  GeneratePdf(Invoice invoice): byte[]
  GenerateHtml(Invoice invoice): string
  EmailInvoice(Invoice invoice, string emailAddress)
  GenerateInvoiceSummary(Invoice invoice): InvoiceSummary

IInvoicePaymentAllocationService:
  AllocatePayment(Invoice invoice, decimal amount): AllocationResult
  AutoAllocate(List<Invoice> outstandingInvoices, decimal paymentAmount): List<AllocationResult>
  CalculateAllocationOrder(Customer customer): List<Invoice> (oldest first, smallest first)
```

#### Repository Interfaces

```
IInvoiceRepository:
  GetByIdAsync(Guid id): Invoice? (with lines, payments)
  GetByNumberAsync(string invoiceNumber, Guid tenantId): Invoice?
  GetByCustomerIdAsync(Guid customerId, InvoiceFilter filter): PagedResult<Invoice>
  GetByBillingAccountIdAsync(Guid accountId, InvoiceFilter filter): PagedResult<Invoice>
  GetOutstandingByCustomerAsync(Guid customerId): List<Invoice>
  GetOverdueAsync(Guid tenantId): List<Invoice>
  GetDueForReminderAsync(Guid tenantId, int reminderDays): List<Invoice>
  GetByDateRangeAsync(Guid tenantId, DateTime from, DateTime to): List<Invoice>
  AddAsync(Invoice invoice)
  UpdateAsync(Invoice invoice)

IInvoiceLineRepository:
  GetByInvoiceIdAsync(Guid invoiceId): List<InvoiceLine>
  AddRangeAsync(List<InvoiceLine> lines)
```

#### Domain Rules / Invariants

- Invoice number must be unique per tenant (auto-generated)
- Invoice line total must match invoice total (prevent tampering)
- Cannot pay a cancelled invoice
- Credit notes must reference an original invoice
- Credit note amount cannot exceed original invoice remaining balance
- Invoice cannot be cancelled if fully paid
- Proforma invoices cannot be marked as paid
- Due date must be after issue date
- Tax amount must equal sum of line tax amounts

### 8.2 Application Layer

#### Commands

```
GenerateInvoiceCommand          { BillingAccountId, BillingPeriodStart, BillingPeriodEnd, IssueDate, DueDate, Notes }
GenerateInvoiceFromChargesCommand { BillingAccountId, ChargeIds[], IssueDate, DueDate }
PreviewInvoiceCommand           { BillingAccountId, BillingPeriodStart, BillingPeriodEnd }
SendInvoiceCommand              { InvoiceId, Method }
CancelInvoiceCommand            { InvoiceId, Reason }
IssueCreditNoteCommand          { InvoiceId, Amount, Reason, LineIds[] }
RecordInvoicePaymentCommand     { InvoiceId, PaymentId, Amount }
MarkInvoiceOverdueCommand       { InvoiceId }
RegenerateInvoiceCommand        { InvoiceId }
SendInvoiceReminderCommand      { InvoiceId, ReminderType }
BulkSendInvoicesCommand         { InvoiceIds[] }
CreateManualInvoiceCommand      { BillingAccountId, CustomerId, Lines[], IssueDate, DueDate, Notes }
ApplyCreditNoteCommand          { InvoiceId, CreditNoteId }
```

#### Queries

```
GetInvoiceQuery                 { InvoiceId }
GetInvoiceByNumberQuery         { InvoiceNumber, TenantId }
ListInvoicesQuery               { TenantId, CustomerId, BillingAccountId, Status, Type, DateFrom, DateTo, Page, Size }
GetCustomerInvoicesQuery        { CustomerId, Status, Page, Size }
GetOutstandingInvoicesQuery     { CustomerId }
GetOverdueInvoicesQuery         { TenantId }
GetInvoiceStatsQuery            { TenantId, FromDate, ToDate }
SearchInvoicesQuery             { TenantId, SearchTerm, Page, Size }
GetInvoicePdfQuery              { InvoiceId }
PreviewInvoiceQuery             { BillingAccountId, PeriodStart, PeriodEnd }
```

#### Command Handlers

```
GenerateInvoiceCommandHandler       -> Validates period, aggregates charges, creates invoice, generates PDF, publishes InvoiceGeneratedEvent
GenerateInvoiceFromChargesHandler   -> Creates invoice from selected charges
SendInvoiceHandler                  -> Emails/prints invoice, updates status, publishes InvoiceSentEvent
CancelInvoiceHandler                -> Validates can cancel, updates status, publishes InvoiceCancelledEvent
IssueCreditNoteHandler              -> Validates amount, creates credit note, links to original, publishes InvoiceCreditNoteIssuedEvent
RecordInvoicePaymentHandler         -> Updates amount paid, determines if fully/partially paid, publishes corresponding event
MarkInvoiceOverdueHandler           -> Updates status to Overdue, publishes InvoiceOverdueEvent
```

#### Query Handlers

```
GetInvoiceHandler               -> Returns InvoiceDetailDto with lines, payments
ListInvoicesHandler              -> Returns PagedResult<InvoiceSummaryDto>
GetCustomerInvoicesHandler       -> Returns PagedResult<InvoiceSummaryDto>
GetOutstandingInvoicesHandler    -> Returns List<InvoiceSummaryDto>
GetInvoiceStatsHandler           -> Returns InvoiceStatsDto (count by status, total outstanding, average amount)
SearchInvoicesHandler            -> Returns PagedResult<InvoiceSummaryDto>
PreviewInvoiceHandler            -> Returns InvoicePreviewDto
```

#### Application Services

```
IInvoiceGenerationOrchestrator:
  GenerateInvoiceFromBilling(BillingAccount account, BillingPeriod period): Invoice
  PreviewInvoiceFromBilling(BillingAccount account, BillingPeriod period): InvoicePreview
  GenerateCreditNote(Invoice originalInvoice, decimal amount, string reason): Invoice

IInvoiceDeliveryService:
  SendByEmail(Invoice invoice, string email, byte[] pdf): DeliveryResult
  SendByPost(Invoice invoice): DeliveryResult
  PublishToPortal(Invoice invoice): DeliveryResult
  BatchSend(List<Invoice> invoices, InvoiceDeliveryMethod method): BatchDeliveryResult
```

#### DTOs

```
InvoiceSummaryDto        { Id, InvoiceNumber, CustomerId, CustomerName, Type, Status, IssueDate, DueDate, TotalAmount, AmountDue, Currency, BillingPeriod, IsOverdue }
InvoiceDetailDto         { Id, InvoiceNumber, CustomerId, CustomerName, BillingAccountId, Type, Status, IssueDate, DueDate, PaidDate, Lines, Payments, Subtotal, DiscountAmount, TaxAmount, TotalAmount, AmountDue, AmountPaid, Currency, BillingPeriodStart, BillingPeriodEnd, BillingAddress, PaymentTerms, Notes, PdfUrl, CreatedAt }
InvoiceLineDto           { Id, LineNumber, Description, ProductName, Type, Quantity, UnitPrice, DiscountAmount, TaxAmount, TotalPrice, StartDate, EndDate }
InvoicePaymentDto        { Id, PaymentId, Amount, Currency, AllocatedAt }
InvoiceStatsDto          { TotalInvoices, TotalByStatus, TotalOutstanding, TotalOverdue, TotalBilled, AverageAmount, TotalCreditNotes, CollectionRate }
InvoicePreviewDto        { BillingAccountId, PeriodStart, PeriodEnd, Lines[], Subtotal, DiscountAmount, TaxAmount, TotalAmount, Estimate }
```

### 8.3 Infrastructure Layer

#### EF Core Entity Configurations

```
InvoiceConfiguration:
  Table: "invoices"
  HasIndex(i => new { i.TenantId, i.InvoiceNumber }).IsUnique()
  HasIndex(i => i.CustomerId)
  HasIndex(i => i.BillingAccountId)
  HasIndex(i => new { i.TenantId, i.Status })
  HasIndex(i => new { i.TenantId, i.DueDate })
  HasIndex(i => i.IssueDate)
  Property(i => i.Notes).HasColumnType("text")
  Property(i => i.InternalNotes).HasColumnType("text")
  OwnsOne(i => i.BillingAddress)
  OwnsOne(i => i.PaymentTerms)
  HasMany(i => i.Lines).WithOne().HasForeignKey(il => il.InvoiceId).OnDelete(DeleteBehavior.Cascade)
  HasMany(i => i.Payments).WithOne().HasForeignKey(ip => ip.InvoiceId).OnDelete(DeleteBehavior.Cascade)

InvoiceLineConfiguration:
  Table: "invoice_lines"
  HasIndex(il => il.InvoiceId)
  HasIndex(il => il.ChargeId).HasFilter("charge_id IS NOT NULL")
  Property(il => il.Description).HasMaxLength(500)

InvoicePaymentConfiguration:
  Table: "invoice_payments"
  HasIndex(ip => new { ip.InvoiceId, ip.PaymentId }).IsUnique()
```

#### Repository Implementations

```
InvoiceRepository       -> EF Core with lines, payments; filtering by status, date, customer
InvoiceLineRepository   -> EF Core
```

#### Integration Events

**Publishes:**
```
InvoiceGeneratedEvent        -> Topic: "invoices.invoice.generated"
InvoiceSentEvent             -> Topic: "invoices.invoice.sent"
InvoicePaidEvent             -> Topic: "invoices.invoice.paid"
InvoicePartiallyPaidEvent    -> Topic: "invoices.invoice.partially_paid"
InvoiceOverdueEvent          -> Topic: "invoices.invoice.overdue"
InvoiceCancelledEvent        -> Topic: "invoices.invoice.cancelled"
InvoiceCreditNoteIssuedEvent -> Topic: "invoices.invoice.credit_note_issued"
InvoiceReminderSentEvent     -> Topic: "invoices.invoice.reminder_sent"
```

**Subscribes to:**
```
billing.period.closed           -> Generate invoice from closed billing period
payments.payment.received       -> Allocate payment to invoices
subscription.charge_generated  -> Include charge in next invoice
```

#### Background Jobs

```
OverdueInvoiceJob:
  Schedule: Daily
  Marks invoices past due as overdue
  Publishes InvoiceOverdueEvent
  Triggers dunning process

InvoiceReminderJob:
  Schedule: Daily
  Sends reminders for invoices approaching due date
  Configurable: X days before due, X days after due

InvoicePdfGenerationJob:
  Schedule: On demand
  Generates PDFs for invoices in Generated status

InvoiceArchiveJob:
  Schedule: Monthly
  Archives invoices older than 7 years
```

#### Caching Strategy

```
Cache: Invoice by ID (Redis, 30 minutes, evict on change)
Cache: Invoice by number (Redis, 30 minutes, evict on change)
Cache: Outstanding invoices by customer (Redis, 15 minutes, evict on payment)
Cache: Invoice statistics (Redis, 1 hour)
Cache: Invoice PDF (Redis or filesystem cache, long TTL)
```

#### External Service Integrations

```
PDF Generation Service:
  - Invoice PDF generation with templates
  - Support for multiple languages/locales
  - Company branding and logo

Postal Mail Service:
  - Physical invoice printing and mailing
  - Tracking delivery status

E-Invoicing Service:
  - PEPPOL network integration
  - EDI invoice format generation
  - Government e-invoicing compliance
```

### 8.4 API Layer

#### Endpoints

```
GET    /api/v1/invoices                              -> ListInvoices              [Authenticated, Invoice.Read]
POST   /api/v1/invoices                              -> GenerateInvoice           [Admin, Invoice.Create]
POST   /api/v1/invoices/manual                       -> CreateManualInvoice       [Admin, Invoice.Create]
GET    /api/v1/invoices/{id}                         -> GetInvoice                [Authenticated, Invoice.Read]
GET    /api/v1/invoices/{id}/pdf                     -> GetInvoicePdf             [Authenticated, Invoice.Read]
POST   /api/v1/invoices/{id}/send                    -> SendInvoice               [Admin, Invoice.Update]
POST   /api/v1/invoices/{id}/cancel                  -> CancelInvoice             [Admin, Invoice.Admin]
POST   /api/v1/invoices/{id}/credit-note             -> IssueCreditNote           [Admin, Invoice.Admin]
POST   /api/v1/invoices/{id}/reminder                -> SendInvoiceReminder       [Admin, Invoice.Update]
POST   /api/v1/invoices/{id}/payment                 -> RecordInvoicePayment      [Admin, Invoice.Update]
POST   /api/v1/invoices/{id}/mark-overdue            -> MarkInvoiceOverdue        [Admin, Invoice.Admin]
POST   /api/v1/invoices/{id}/regenerate              -> RegenerateInvoice         [Admin, Invoice.Admin]
GET    /api/v1/invoices/{id}/preview                 -> PreviewInvoice            [Authenticated, Invoice.Read]
GET    /api/v1/invoices/by-number/{number}           -> GetInvoiceByNumber        [Authenticated, Invoice.Read]
GET    /api/v1/invoices/customer/{customerId}        -> GetCustomerInvoices       [Authenticated, Invoice.Read]
GET    /api/v1/invoices/outstanding                  -> GetOutstandingInvoices    [Authenticated, Invoice.Read]
GET    /api/v1/invoices/overdue                      -> GetOverdueInvoices        [Admin, Invoice.Read]
GET    /api/v1/invoices/stats                        -> GetInvoiceStats           [Admin, Invoice.Read]
POST   /api/v1/invoices/bulk-send                    -> BulkSendInvoices          [Admin, Invoice.Admin]
```

#### Request/Response Models

```
GenerateInvoiceRequest       { BillingAccountId, PeriodStart, PeriodEnd, IssueDate, DueDate, Notes }
CreateManualInvoiceRequest   { BillingAccountId, CustomerId, Lines[], IssueDate, DueDate, Notes }
InvoiceLineRequest           { Description, ProductId, Type, Quantity, UnitPrice, DiscountAmount, TaxRate, TaxCode, StartDate, EndDate }
SendInvoiceRequest           { Method (Email, Post, Portal) }
CancelInvoiceRequest         { Reason }
IssueCreditNoteRequest       { Amount, Reason, LineIds[] }
SendReminderRequest          { ReminderType (BeforeDue, AfterDue, Final) }
RecordPaymentRequest         { PaymentId, Amount }
```

#### Authorization Requirements

```
Invoice.Read: View invoices, download PDFs, preview
Invoice.Create: Generate invoices, create manual invoices
Invoice.Update: Send invoices, record payments, send reminders
Invoice.Admin: Cancel invoices, issue credit notes, mark overdue, regenerate, bulk send
```

#### Rate Limiting

```
Invoice generation: 30 requests/minute
Bulk send: 5 requests/minute
Invoice PDF download: 60 requests/minute per user
Invoice queries: 30 requests/minute
```

#### Versioning Strategy

```
URL-based: /api/v1/invoices
```

### 8.5 Events

#### Published Events

| Event | Topic | Schema |
|-------|-------|--------|
| InvoiceGenerated | `invoices.invoice.generated` | `{ InvoiceId, InvoiceNumber, CustomerId, BillingAccountId, TenantId, TotalAmount, Currency, IssueDate, DueDate, OccurredAt }` |
| InvoiceSent | `invoices.invoice.sent` | `{ InvoiceId, InvoiceNumber, CustomerId, Method, SentAt, OccurredAt }` |
| InvoicePaid | `invoices.invoice.paid` | `{ InvoiceId, InvoiceNumber, CustomerId, AmountPaid, PaymentDate, OccurredAt }` |
| InvoicePartiallyPaid | `invoices.invoice.partially_paid` | `{ InvoiceId, InvoiceNumber, CustomerId, AmountPaid, RemainingBalance, OccurredAt }` |
| InvoiceOverdue | `invoices.invoice.overdue` | `{ InvoiceId, InvoiceNumber, CustomerId, DaysOverdue, OccurredAt }` |
| InvoiceCancelled | `invoices.invoice.cancelled` | `{ InvoiceId, InvoiceNumber, Reason, OccurredAt }` |
| InvoiceCreditNoteIssued | `invoices.invoice.credit_note_issued` | `{ InvoiceId, CreditNoteId, OriginalInvoiceId, Amount, Reason, OccurredAt }` |

#### Subscribed Events

| Event | Source | Action |
|-------|--------|--------|
| `billing.period.closed` | Billing | Generate invoice from billing period |
| `payments.payment.received` | Payments | Allocate payment to outstanding invoices |

### 8.6 Data Ownership

#### Owned Tables

| Table | Type | Purpose |
|-------|------|---------|
| `invoices` | Primary | Invoice records |
| `invoice_lines` | Primary | Invoice line items |
| `invoice_payments` | Join | Payment-invoice allocation |

#### Foreign Key Relationships

```
invoices.tenant_id -> iam_tenants.id (RESTRICT)
invoices.customer_id -> crm_customers.id (RESTRICT)
invoices.billing_account_id -> billing_accounts.id (RESTRICT)
invoice_lines.invoice_id -> invoices.id (CASCADE)
invoice_lines.subscription_id -> subscriptions.id (SET NULL)
invoice_lines.charge_id -> subscription_charges.id (SET NULL)
invoice_payments.invoice_id -> invoices.id (CASCADE)
invoice_payments.payment_id -> payments_payments.id (RESTRICT)
```

### 8.7 Dependencies

#### Depends On

| Module | Reason |
|--------|--------|
| IAM | Tenant context |
| CRM | Customer reference |
| Billing | Billing account reference |
| Subscriptions | Charge and subscription references on lines |

#### Depended On By

| Module | Reason |
|--------|--------|
| Payments | Invoice references for payment allocation |
| Collections | Overdue invoices for collections |
| Reporting | Invoice analytics |
| Notifications | Invoice delivery notifications |

### 8.8 Security Requirements

#### RBAC Roles & Permissions

```
Module-level:
  Invoice.Read     -- View invoices, download PDFs
  Invoice.Create   -- Generate invoices
  Invoice.Update   -- Send invoices, record payments, send reminders
  Invoice.Admin    -- Cancel invoices, issue credit notes, bulk operations

Role defaults:
  Admin: All permissions
  BillingAdmin: All permissions
  SupportAgent: Invoice.Read
  Customer: Invoice.Read (own invoices only)
```

#### Data Isolation Rules

- Invoices are tenant-scoped
- Customer can view only their own invoices
- Invoice PDFs are access-controlled (temporary signed URLs)
- Payment allocation details restricted to BillingAdmin+
- Cancelled and credit note invoices retained for audit

#### Audit Requirements

- Invoice generation (who triggered, source billing period)
- Invoice sending (method, timestamp, recipient)
- Invoice cancellations (reason, approver)
- Credit note issuance (amount, reason, original invoice)
- Payment allocation changes
- Invoice status changes

---

## 9. Payments — Payment Processing

### 9.1 Domain Layer

#### Aggregate: Payment

```
Id: Guid
TenantId: Guid
CustomerId: Guid
BillingAccountId: Guid?
PaymentNumber: string
Amount: decimal
Currency: string
Method: PaymentMethod            (CreditCard, DebitCard, BankTransfer, DirectDebit, Wallet, Cash, Cheque, Voucher)
Status: PaymentStatus            (Pending, Processing, Completed, Failed, Refunded, PartiallyRefunded, Cancelled)
Type: PaymentType                (OneTime, Recurring, Installment, Deposit)
Provider: PaymentProvider?       (Stripe, PayPal, Adyen, Internal, Bank)
ProviderPaymentId: string?
ProviderTransactionId: string?
FeeAmount: decimal?
NetAmount: decimal?
Description: string?
Reference: string?
InvoiceAllocations: List<PaymentAllocation>
PaidAt: DateTime?
FailedAt: DateTime?
FailureReason: string?
RefundedAt: DateTime?
RefundReason: string?
Metadata: Dictionary<string, string>
CreatedAt: DateTime
UpdatedAt: DateTime

Methods:
  Complete(string providerPaymentId, string providerTransactionId)
  Fail(string reason)
  Refund(decimal amount, string reason)
  AllocateToInvoice(Guid invoiceId, decimal amount)
  Cancel()
  CanBeRefunded(): bool
  GetRemainingRefundableAmount(): decimal
```

#### Entity: PaymentAllocation

```
Id: Guid
PaymentId: Guid
InvoiceId: Guid
Amount: decimal
Currency: string
AllocatedAt: DateTime
```

#### Entity: PaymentMethod (saved payment method / wallet)

```
Id: Guid
TenantId: Guid
CustomerId: Guid
Type: PaymentMethodType          (CreditCard, DebitCard, BankAccount, Wallet, Voucher)
Provider: PaymentProvider
ProviderToken: string            (tokenized card/account reference)
DisplayName: string              (e.g., "Visa ending in 4242")
ExpiryDate: string?
LastFourDigits: string?
CardBrand: string?
BankName: string?
AccountNumberLastFour: string?
IsDefault: bool
IsVerified: bool
BillingAddressId: Guid?
Metadata: Dictionary<string, string>
CreatedAt: DateTime
UpdatedAt: DateTime

Methods:
  SetAsDefault()
  Verify()
  Expire()
  IsExpired(): bool
```

#### Entity: PaymentGatewayTransaction

```
Id: Guid
PaymentId: Guid
Gateway: PaymentProvider
GatewayTransactionId: string
GatewayPaymentId: string?
Amount: decimal
Currency: string
Status: GatewayTransactionStatus     (Initiated, Authorized, Captured, Failed, Refunded, Settled)
RequestPayload: string?
ResponsePayload: string?
ErrorCode: string?
ErrorMessage: string?
AttemptNumber: int
InitiatedAt: DateTime
CompletedAt: DateTime?
```

#### Value Objects

```
PaymentStatus: enum { Pending, Processing, Completed, Failed, Refunded, PartiallyRefunded, Cancelled }
PaymentMethodType: enum { CreditCard, DebitCard, BankAccount, Wallet, Voucher, Ussd, MobileMoney }
PaymentType: enum { OneTime, Recurring, Installment, Deposit }
PaymentProvider: enum { Stripe, PayPal, Adyen, Flutterwave, Internal, Bank, MobileMoney }
GatewayTransactionStatus: enum { Initiated, Authorized, Captured, Failed, Refunded, PartiallyRefunded, Settled, ChargedBack }
```

#### Domain Events

```
PaymentInitiatedEvent          { PaymentId, PaymentNumber, CustomerId, Amount, Currency, Method, OccurredAt }
PaymentCompletedEvent          { PaymentId, PaymentNumber, CustomerId, Amount, Currency, Provider, ProviderTransactionId, PaidAt, OccurredAt }
PaymentFailedEvent             { PaymentId, PaymentNumber, CustomerId, Amount, Currency, FailureReason, OccurredAt }
PaymentRefundedEvent           { PaymentId, Amount, Currency, Reason, OccurredAt }
PaymentCancelledEvent          { PaymentId, OccurredAt }
PaymentAllocatedEvent          { PaymentId, InvoiceId, Amount, OccurredAt }
PaymentMethodAddedEvent       { PaymentMethodId, CustomerId, Type, DisplayName, OccurredAt }
PaymentMethodDeletedEvent     { PaymentMethodId, CustomerId, OccurredAt }
PaymentMethodDefaultChangedEvent { PaymentMethodId, CustomerId, OccurredAt }
RecurringPaymentInitiatedEvent { PaymentId, SubscriptionId, Amount, OccurredAt }
ChargebackReceivedEvent        { PaymentId, TransactionId, Amount, Reason, OccurredAt }
```

#### Domain Services

```
IPaymentGatewayService:
  Authorize(Payment payment, PaymentMethod method): GatewayResult
  Capture(Payment payment, string authorizationId): GatewayResult
  Refund(Payment payment, decimal amount): GatewayResult
  Void(Payment payment, string transactionId): GatewayResult
  GetTransactionStatus(string transactionId): GatewayStatus
  ValidateWebhookSignature(string payload, string signature): bool

IPaymentAllocationService:
  AllocateToOldestInvoices(Payment payment): List<AllocationResult>
  AllocateToSpecificInvoice(Payment payment, Guid invoiceId): AllocationResult
  ReverseAllocation(Payment payment, PaymentAllocation allocation): Result
  CalculateRemainingUnallocated(Payment payment): decimal

IRefundService:
  CalculateRefundableAmount(Payment payment): decimal
  ProcessRefund(Payment payment, decimal amount, string reason): RefundResult
  ValidateRefund(Payment payment, decimal amount): ValidationResult
```

#### Repository Interfaces

```
IPaymentRepository:
  GetByIdAsync(Guid id): Payment?
  GetByNumberAsync(string number, Guid tenantId): Payment?
  GetByCustomerIdAsync(Guid customerId, PaymentFilter filter): PagedResult<Payment>
  GetByInvoiceIdAsync(Guid invoiceId): List<Payment>
  GetByProviderTransactionIdAsync(string transactionId, PaymentProvider provider): Payment?
  AddAsync(Payment payment)
  UpdateAsync(Payment payment)
  CountByStatusAsync(Guid tenantId): Dictionary<PaymentStatus, int>

IPaymentMethodRepository:
  GetByIdAsync(Guid id): PaymentMethod?
  GetByCustomerIdAsync(Guid customerId): List<PaymentMethod>
  GetDefaultAsync(Guid customerId): PaymentMethod?
  AddAsync(PaymentMethod method)
  UpdateAsync(PaymentMethod method)
  DeleteAsync(PaymentMethod method)

IGatewayTransactionRepository:
  GetByPaymentIdAsync(Guid paymentId): List<GatewayTransaction>
  AddAsync(GatewayTransaction transaction)
  UpdateAsync(GatewayTransaction transaction)
```

#### Domain Rules / Invariants

- Payment number must be unique per tenant
- Payment amount must be positive
- Refund amount cannot exceed original payment amount minus previous refunds
- A payment method can only be deleted if not associated with active recurring payments
- Failed payments cannot be refunded
- Allocated amount cannot exceed payment amount
- Payment cannot be allocated to already-paid invoices (full payment)
- Gateway transactions are immutable after completion
- Recurring payments require a saved payment method

### 9.2 Application Layer

#### Commands

```
InitiatePaymentCommand           { TenantId, CustomerId, BillingAccountId, Amount, Currency, Method, PaymentMethodId, Description, Reference, Metadata }
ProcessPaymentCommand            { PaymentId, PaymentMethodId, Cvv?, SavePaymentMethod }
CompletePaymentCommand           { PaymentId, ProviderPaymentId, ProviderTransactionId }
FailPaymentCommand               { PaymentId, FailureReason }
RefundPaymentCommand             { PaymentId, Amount, Reason }
AllocatePaymentCommand           { PaymentId, InvoiceId, Amount }
CancelPaymentCommand             { PaymentId }
RecordOfflinePaymentCommand      { TenantId, CustomerId, Amount, Currency, Method, Reference, PaidAt, InvoiceIds[] }
AddPaymentMethodCommand          { TenantId, CustomerId, ProviderToken, Type, DisplayName, LastFourDigits, ExpiryDate, CardBrand, IsDefault, BillingAddressId }
UpdatePaymentMethodCommand       { PaymentMethodId, DisplayName, IsDefault }
DeletePaymentMethodCommand       { PaymentMethodId }
ProcessRecurringPaymentCommand   { SubscriptionId, Amount }
ProcessRefundCommand             { PaymentId, Amount, Reason }
HandleGatewayWebhookCommand      { Provider, Payload, Signature }
SyncPaymentStatusCommand         { PaymentId }
```

#### Queries

```
GetPaymentQuery                  { PaymentId }
GetPaymentByNumberQuery          { PaymentNumber, TenantId }
ListPaymentsQuery                { TenantId, CustomerId, Status, Method, DateFrom, DateTo, Page, Size }
GetCustomerPaymentsQuery         { CustomerId, Page, Size }
GetInvoicePaymentsQuery          { InvoiceId }
GetPaymentMethodsQuery           { CustomerId }
GetDefaultPaymentMethodQuery     { CustomerId }
GetPaymentStatsQuery             { TenantId, FromDate, ToDate }
SearchPaymentsQuery              { TenantId, SearchTerm, Page, Size }
GetPaymentTimelineQuery          { PaymentId }
GetGatewayTransactionsQuery      { PaymentId }
```

#### Command Handlers

```
InitiatePaymentHandler           -> Creates payment record, generates payment number, publishes PaymentInitiatedEvent
ProcessPaymentHandler            -> Calls gateway to process, updates status, publishes PaymentCompletedEvent or PaymentFailedEvent
CompletePaymentHandler           -> Confirms gateway success, triggers allocation
FailPaymentHandler               -> Updates status, publishes PaymentFailedEvent
RefundPaymentHandler             -> Validates refundable amount, processes through gateway, publishes PaymentRefundedEvent
AllocatePaymentHandler           -> Allocates to invoice(s), publishes PaymentAllocatedEvent
RecordOfflinePaymentHandler      -> Creates completed payment without gateway
AddPaymentMethodHandler          -> Tokenizes and saves, publishes PaymentMethodAddedEvent
DeletePaymentMethodHandler       -> Validates no active recurring payments, publishes PaymentMethodDeletedEvent
ProcessRecurringPaymentHandler   -> Creates and processes payment for recurring subscription
HandleGatewayWebhookHandler      -> Validates webhook, updates payment status
```

#### Query Handlers

```
GetPaymentHandler                -> Returns PaymentDetailDto with allocations, timeline
ListPaymentsHandler              -> Returns PagedResult<PaymentSummaryDto>
GetCustomerPaymentsHandler       -> Returns PagedResult<PaymentSummaryDto>
GetInvoicePaymentsHandler        -> Returns List<PaymentDto> (with allocation details)
GetPaymentMethodsHandler         -> Returns List<PaymentMethodDto>
GetPaymentStatsHandler           -> Returns PaymentStatsDto
SearchPaymentsHandler            -> Returns PagedResult<PaymentSummaryDto>
```

#### Application Services

```
IPaymentProcessingService:
  ProcessPayment(Payment payment, PaymentMethod method): PaymentResult
  ProcessRefund(Payment payment, decimal amount): PaymentResult
  RetryFailedPayment(Payment payment): PaymentResult
  SavePaymentMethodFromPayment(Payment payment): PaymentMethod

IPaymentWebhookService:
  ValidateWebhook(PaymentProvider provider, string payload, string signature): bool
  ProcessWebhook(PaymentProvider provider, string payload): WebhookResult
  HandleChargeback(string transactionId, decimal amount, string reason)

IReconciliationService:
  ReconcilePayments(DateTime date): ReconciliationResult
  MatchBankStatement(List<BankTransaction> transactions): List<MatchResult>
  GenerateReconciliationReport(DateTime from, DateTime to): Report
```

#### DTOs

```
PaymentSummaryDto        { Id, PaymentNumber, CustomerId, CustomerName, Amount, Currency, Method, Status, Type, PaidAt, FailureReason, Reference }
PaymentDetailDto         { Id, PaymentNumber, CustomerId, CustomerName, BillingAccountId, Amount, Currency, NetAmount, FeeAmount, Method, Status, Type, Provider, ProviderPaymentId, ProviderTransactionId, Description, Reference, Invoices, Timeline, Metadata, CreatedAt }
PaymentDto               { Id, PaymentNumber, Amount, Currency, Method, Status, PaidAt }
PaymentMethodDto         { Id, CustomerId, Type, DisplayName, LastFourDigits, ExpiryDate, CardBrand, IsDefault, IsVerified, CreatedAt }
PaymentAllocationDto     { Id, InvoiceId, InvoiceNumber, Amount, AllocatedAt }
GatewayTransactionDto    { Id, Gateway, GatewayTransactionId, Amount, Status, AttemptNumber, ErrorMessage, InitiatedAt, CompletedAt }
PaymentStatsDto          { TotalPayments, TotalByStatus, TotalByMethod, TotalAmount, TotalFees, RefundRate, AverageAmount }
```

### 9.3 Infrastructure Layer

#### EF Core Entity Configurations

```
PaymentConfiguration:
  Table: "payments_payments"
  HasIndex(p => new { p.TenantId, p.PaymentNumber }).IsUnique()
  HasIndex(p => p.CustomerId)
  HasIndex(p => new { p.TenantId, p.Status })
  HasIndex(p => p.PaidAt)
  HasIndex(p => p.ProviderTransactionId).HasFilter("provider_transaction_id IS NOT NULL")
  Property(p => p.Metadata).HasColumnType("jsonb")
  Property(p => p.Description).HasMaxLength(500)
  HasMany(p => p.Allocations).WithOne().HasForeignKey(pa => pa.PaymentId).OnDelete(DeleteBehavior.Cascade)

PaymentAllocationConfiguration:
  Table: "payments_allocations"
  HasIndex(pa => new { pa.PaymentId, pa.InvoiceId }).IsUnique()

PaymentMethodConfiguration:
  Table: "payments_payment_methods"
  HasIndex(pm => new { pm.CustomerId, pm.Type })
  HasIndex(pm => pm.ProviderToken).IsUnique()
  Property(pm => pm.Metadata).HasColumnType("jsonb")

GatewayTransactionConfiguration:
  Table: "payments_gateway_transactions"
  HasIndex(gt => new { gt.Gateway, gt.GatewayTransactionId }).IsUnique()
  HasIndex(gt => gt.PaymentId)
  Property(gt => gt.RequestPayload).HasColumnType("text")
  Property(gt => gt.ResponsePayload).HasColumnType("text")
```

#### Repository Implementations

```
PaymentRepository               -> EF Core with allocations, gateway transactions
PaymentMethodRepository          -> EF Core
GatewayTransactionRepository   -> EF Core
```

#### Integration Events

**Publishes:**
```
PaymentInitiatedEvent           -> Topic: "payments.payment.initiated"
PaymentCompletedEvent           -> Topic: "payments.payment.received"
PaymentFailedEvent              -> Topic: "payments.payment.failed"
PaymentRefundedEvent            -> Topic: "payments.payment.refunded"
PaymentCancelledEvent           -> Topic: "payments.payment.cancelled"
PaymentAllocatedEvent           -> Topic: "payments.payment.allocated"
PaymentMethodAddedEvent        -> Topic: "payments.payment_method.added"
PaymentMethodDeletedEvent      -> Topic: "payments.payment_method.deleted"
RecurringPaymentInitiatedEvent  -> Topic: "payments.recurring.initiated"
ChargebackReceivedEvent         -> Topic: "payments.chargeback.received"
```

**Subscribes to:**
```
invoices.invoice.overdue           -> Trigger automatic payment attempt
invoices.invoice.generated         -> Process payment if auto-pay enabled
subscription.subscription.renewed  -> Process recurring payment
billing.dunning_level.changed      -> Trigger payment attempt based on dunning
crm.customer.status_changed        -> Cancel pending payments if customer suspended
```

#### Background Jobs

```
RecurringPaymentJob:
  Schedule: Daily
  Processes recurring payments due for billing

PaymentRetryJob:
  Schedule: Every 6 hours
  Retries failed payments (up to 3 attempts)

PaymentReconciliationJob:
  Schedule: Daily
  Reconciles payments with bank statements
  Matches offline payments

ExpiredPaymentMethodJob:
  Schedule: Weekly
  Identifies expired payment methods
  Sends update reminders

ChargebackJob:
  Schedule: Hourly
  Polls gateway for chargeback notifications
  Processes chargebacks
```

#### Caching Strategy

```
Cache: Payment by ID (Redis, 15 minutes, evict on change)
Cache: Customer payment methods (Redis, 30 minutes, evict on method change)
Cache: Payment statistics (Redis, 1 hour)
Cache: Gateway transaction by ID (Redis, 30 minutes)
```

#### External Service Integrations

```
Payment Gateway (Stripe, Adyen, PayPal):
  - Payment authorization and capture
  - Refunds and voids
  - Tokenization for saved payment methods
  - Webhook handling for async events
  - Recurring payment setup
  - 3D Secure authentication

Bank Integration:
  - Direct debit (SEPA, ACH, BACS)
  - Bank file generation (pain.001, NACHA)
  - Bank statement import
  - BACS/EFT file processing

Mobile Money Integration (MPesa, Airtel Money):
  - STK Push for mobile payments
  - Payment confirmation via callback
  - Balance inquiry
```

### 9.4 API Layer

#### Endpoints

```
GET    /api/v1/payments                              -> ListPayments              [Authenticated, Payment.Read]
POST   /api/v1/payments                              -> InitiatePayment           [Authenticated, Payment.Create]
POST   /api/v1/payments/offline                      -> RecordOfflinePayment      [Admin, Payment.Admin]
GET    /api/v1/payments/{id}                         -> GetPayment                [Authenticated, Payment.Read]
POST   /api/v1/payments/{id}/process                 -> ProcessPayment            [Authenticated, Payment.Create]
POST   /api/v1/payments/{id}/complete                -> CompletePayment           [Admin, Payment.Update]
POST   /api/v1/payments/{id}/fail                    -> FailPayment               [Admin, Payment.Update]
POST   /api/v1/payments/{id}/refund                  -> RefundPayment             [Admin, Payment.Admin]
POST   /api/v1/payments/{id}/cancel                  -> CancelPayment             [Authenticated, Payment.Update]
POST   /api/v1/payments/{id}/allocate                -> AllocatePayment           [Admin, Payment.Update]
GET    /api/v1/payments/{id}/timeline                -> GetPaymentTimeline        [Authenticated, Payment.Read]
GET    /api/v1/payments/{id}/transactions            -> GetGatewayTransactions    [Admin, Payment.Read]
GET    /api/v1/payments/by-number/{number}           -> GetPaymentByNumber        [Authenticated, Payment.Read]
GET    /api/v1/payments/customer/{customerId}        -> GetCustomerPayments       [Authenticated, Payment.Read]
GET    /api/v1/payments/invoice/{invoiceId}          -> GetInvoicePayments        [Authenticated, Payment.Read]
GET    /api/v1/payments/stats                        -> GetPaymentStats           [Admin, Payment.Read]
POST   /api/v1/payments/process-recurring            -> ProcessRecurringPayment   [Admin, Payment.Admin]

# Payment Methods
GET    /api/v1/payments/methods                      -> GetPaymentMethods         [Authenticated, Payment.Read]
POST   /api/v1/payments/methods                      -> AddPaymentMethod          [Authenticated, Payment.Create]
PUT    /api/v1/payments/methods/{id}                 -> UpdatePaymentMethod       [Authenticated, Payment.Update]
DELETE /api/v1/payments/methods/{id}                 -> DeletePaymentMethod       [Authenticated, Payment.Update]

# Webhooks
POST   /api/v1/payments/webhook/{provider}           -> HandleGatewayWebhook      [Public (gateway IPs only)]
```

#### Request/Response Models

```
InitiatePaymentRequest       { CustomerId, BillingAccountId, Amount, Currency, Method, PaymentMethodId, Description, Reference, Metadata }
ProcessPaymentRequest        { PaymentMethodId, Cvv, SavePaymentMethod }
RecordOfflinePaymentRequest  { CustomerId, Amount, Currency, Method, Reference, PaidAt, InvoiceIds[] }
RefundRequest                { Amount, Reason }
AllocatePaymentRequest       { InvoiceId, Amount }
CancelPaymentRequest         { Reason }
AddPaymentMethodRequest      { ProviderToken, Type, DisplayName, LastFourDigits, ExpiryDate, CardBrand, IsDefault, BillingAddressId }
UpdatePaymentMethodRequest   { DisplayName, IsDefault }
ProcessRecurringRequest      { SubscriptionId, Amount }
```

#### Authorization Requirements

```
Payment.Read: View payments, methods, timelines
Payment.Create: Initiate payments, process payments, add payment methods
Payment.Update: Cancel payments, update payment methods, delete methods
Payment.Admin: Record offline payments, refund, allocate, process recurring
Webhook: Public (restricted to known gateway IPs via firewall/middleware)
```

#### Rate Limiting

```
Payment initiation: 10 requests/minute per customer
Payment processing: 20 requests/minute
Payment refunds: 5 requests/minute per user
Payment method management: 10 requests/minute
Recurring processing: 30 requests/minute (system)
Webhooks: 100 requests/minute (from gateway IPs)
```

### 9.5 Events

#### Published Events

| Event | Topic | Schema |
|-------|-------|--------|
| PaymentInitiated | `payments.payment.initiated` | `{ PaymentId, PaymentNumber, CustomerId, Amount, Currency, Method, OccurredAt }` |
| PaymentCompleted | `payments.payment.received` | `{ PaymentId, PaymentNumber, CustomerId, Amount, Currency, NetAmount, Provider, ProviderTransactionId, PaidAt, OccurredAt }` |
| PaymentFailed | `payments.payment.failed` | `{ PaymentId, PaymentNumber, CustomerId, Amount, Currency, FailureReason, AttemptNumber, OccurredAt }` |
| PaymentRefunded | `payments.payment.refunded` | `{ PaymentId, Amount, Currency, Reason, OccurredAt }` |
| PaymentAllocated | `payments.payment.allocated` | `{ PaymentId, InvoiceId, Amount, OccurredAt }` |
| PaymentMethodAdded | `payments.payment_method.added` | `{ PaymentMethodId, CustomerId, Type, DisplayName, OccurredAt }` |
| ChargebackReceived | `payments.chargeback.received` | `{ PaymentId, TransactionId, Amount, Currency, Reason, OccurredAt }` |

#### Subscribed Events

| Event | Source | Action |
|-------|--------|--------|
| `invoices.invoice.overdue` | Invoices | Trigger auto-payment attempt |
| `invoices.invoice.generated` | Invoices | Process payment if auto-pay enabled |
| `subscription.renewed` | Subscriptions | Process recurring payment |
| `billing.dunning_level.changed` | Billing | Trigger payment attempt |
| `crm.customer.status_changed` | CRM | Cancel pending payments |

### 9.6 Data Ownership

#### Owned Tables

| Table | Type | Purpose |
|-------|------|---------|
| `payments_payments` | Primary | Payment records |
| `payments_allocations` | Join | Payment-to-invoice allocation |
| `payments_payment_methods` | Primary | Saved payment methods |
| `payments_gateway_transactions` | Secondary | Gateway transaction audit trail |
| `payments_recurring_profiles` | Secondary | Recurring payment configurations |

#### Foreign Key Relationships

```
payments_payments.tenant_id -> iam_tenants.id (RESTRICT)
payments_payments.customer_id -> crm_customers.id (RESTRICT)
payments_payments.billing_account_id -> billing_accounts.id (SET NULL)
payments_allocations.payment_id -> payments_payments.id (CASCADE)
payments_allocations.invoice_id -> invoices.id (RESTRICT)
payments_payment_methods.tenant_id -> iam_tenants.id (RESTRICT)
payments_payment_methods.customer_id -> crm_customers.id (CASCADE)
payments_gateway_transactions.payment_id -> payments_payments.id (CASCADE)
payments_recurring_profiles.subscription_id -> subscriptions.id (CASCADE)
```

### 9.7 Dependencies

#### Depends On

| Module | Reason |
|--------|--------|
| IAM | Tenant context |
| CRM | Customer reference |
| Invoices | Invoice reference for allocations |

#### Depended On By

| Module | Reason |
|--------|--------|
| Billing | Payment updates account balance |
| Collections | Payment history for collection decisions |
| Reporting | Payment analytics |
| Notifications | Payment confirmation notifications |

### 9.8 Security Requirements

#### RBAC Roles & Permissions

```
Module-level:
  Payment.Read     -- View payments, methods, transaction history
  Payment.Create   -- Initiate payments, add payment methods
  Payment.Update   -- Cancel payments, manage methods
  Payment.Admin    -- Record offline payments, refund, allocate, manage recurring

Role defaults:
  Admin: All permissions
  BillingAdmin: All permissions
  SupportAgent: Payment.Read
  Customer: Payment.Read, Payment.Create, Payment.Update (own data only)
```

#### Data Isolation Rules

- Payment data is tenant-scoped
- PCI DSS compliance: credit card numbers never stored; tokens only
- Payment method tokens are provider-specific and non-reversible
- Gateway transaction logs may contain sensitive data -- restricted access
- Refunds require two-factor auth for amounts above threshold
- Offline payments require documented proof of receipt
- Webhook endpoints restricted to known gateway IPs

#### Audit Requirements

- All payment lifecycle events (initiate, complete, fail, refund, cancel)
- Payment allocation changes
- Payment method additions and deletions
- Recurring payment profile changes
- Gateway webhook processing (payload logged for debugging)
- Refund approvals (who approved, amount, reason)
- Offline payment recording (who recorded, proof reference)

---

## 10. Collections — Debt Collection

### 10.1 Domain Layer

#### Aggregate: CollectionCase

```
Id: Guid
TenantId: Guid
CustomerId: Guid
CaseNumber: string
Status: CollectionStatus           (Open, InProgress, Resolved, Closed, Escalated)
Priority: CollectionPriority       (Low, Medium, High, Urgent)
TotalOutstanding: decimal
Currency: string
Debts: List<Debt>
Actions: List<CollectionAction>
AssignedAgentId: Guid?
AssignedAt: DateTime?
OpenedAt: DateTime
ClosedAt: DateTime?
CloseReason: string?
ExternalAgencyId: string?
ExternalCaseId: string?
Notes: string?
Metadata: Dictionary<string, string>

Methods:
  AssignAgent(Guid agentId)
  AddDebt(Debt debt)
  RemoveDebt(Guid debtId)          (when debt resolved)
  RecordAction(CollectionAction action)
  Escalate(string reason)
  Close(string reason, CollectionResult result)
  Reopen(string reason)
  CalculateTotalOutstanding(): decimal
  GetRemainingBalance(): decimal
```

#### Entity: Debt

```
Id: Guid
CollectionCaseId: Guid
InvoiceId: Guid?
InvoiceNumber: string?
OriginalAmount: decimal
RemainingAmount: decimal
Currency: string
Status: DebtStatus                (Active, PartiallyPaid, Paid, WrittenOff, Disputed)
DueDate: DateTime
DaysOverdue: int
AgingBucket: AgingBucket          (Bucket1:1-30, Bucket2:31-60, Bucket3:61-90, Bucket4:91-180, Bucket5:180+)
LastPaymentDate: DateTime?
LastPromiseDate: DateTime?
PromiseCount: int
WriteOffDate: DateTime?
WriteOffReason: string?
DisputeReason: string?
DisputeDate: DateTime?
```

#### Entity: CollectionAction

```
Id: Guid
CollectionCaseId: Guid
Type: CollectionActionType        (Call, Email, SMS, Letter, Visit, Promise, Payment, Escalation, WriteOff, Settlement)
Description: string
AgentId: Guid
AgentName: string
Amount: decimal?                  (payment amount if applicable)
Result: ActionResult              (Success, Failed, Pending, NoResponse, PromiseToPay, Disputed)
ContactMethod: string?
ContactNumber: string?
Duration: int?                    (minutes)
Outcome: string?
CreatedAt: DateTime
```

#### Entity: CollectionStrategy

```
Id: Guid
TenantId: Guid
Name: string
Description: string?
Priority: int
AgingBucket: AgingBucket
ActionSequence: List<CollectionActionTemplate>
EscalationAfterDays: int?
EscalationToAgent: bool?
EscalationToAgency: bool?
AutoCloseAfterDays: int?
WriteOffAfterDays: int?
```

#### Value Objects

```
CollectionStatus: enum { Open, InProgress, Resolved, Closed, Escalated, OnHold }
CollectionPriority: enum { Low, Medium, High, Urgent }
DebtStatus: enum { Active, PartiallyPaid, Paid, WrittenOff, Disputed }
AgingBucket: enum { Bucket1_30, Bucket31_60, Bucket61_90, Bucket91_180, Bucket180Plus }
CollectionActionType: enum { Call, Email, SMS, Letter, Visit, Promise, Payment, Escalation, WriteOff, Settlement, PaymentPlan }
ActionResult: enum { Success, Failed, Pending, NoResponse, PromiseToPay, Disputed, WrongNumber, DoNotContact }
CollectionResult: enum { FullyPaid, PartialPayment, WrittenOff, SentToAgency, Settled, CustomerClosed }
```

#### Domain Events

```
CollectionCaseOpenedEvent       { CaseId, CaseNumber, CustomerId, TenantId, TotalOutstanding, OccurredAt }
CollectionCaseAssignedEvent     { CaseId, AgentId, OccurredAt }
CollectionCaseEscalatedEvent   { CaseId, Reason, OccurredAt }
CollectionCaseClosedEvent      { CaseId, Reason, Result, OccurredAt }
DebtAddedEvent                  { CaseId, DebtId, InvoiceId, Amount, OccurredAt }
DebtStatusChangedEvent          { CaseId, DebtId, OldStatus, NewStatus, OccurredAt }
DebtWrittenOffEvent             { CaseId, DebtId, Amount, Reason, OccurredAt }
CollectionActionRecordedEvent   { CaseId, ActionId, ActionType, OccurredAt }
PromiseToPayReceivedEvent       { CaseId, PromiseDate, Amount, OccurredAt }
PaymentPlanCreatedEvent         { CaseId, PlanId, Installments, Amount, OccurredAt }
SettlementAgreedEvent           { CaseId, SettlementAmount, OriginalAmount, OccurredAt }
```

#### Domain Services

```
ICollectionStrategyService:
  DetermineStrategy(CollectionCase collectionCase): CollectionStrategy
  GetNextAction(CollectionCase collectionCase): CollectionActionTemplate?
  EvaluateEscalation(CollectionCase collectionCase): bool
  AutoCloseCase(CollectionCase collectionCase): bool

IPromiseToPayService:
  RecordPromise(CollectionCase collectionCase, decimal amount, DateTime promiseDate, string contactMethod): PromiseResult
  VerifyPromise(CollectionCase collectionCase): PromiseStatus
  HandleBrokenPromise(CollectionCase collectionCase): Action

ISettlementService:
  CalculateSettlementAmount(CollectionCase collectionCase, decimal percentage): decimal
  ProposeSettlement(CollectionCase collectionCase, SettlementProposal proposal): SettlementResult
  AcceptSettlement(CollectionCase collectionCase, SettlementProposal proposal): SettlementResult
```

#### Repository Interfaces

```
ICollectionCaseRepository:
  GetByIdAsync(Guid id): CollectionCase?
  GetByCustomerIdAsync(Guid customerId): List<CollectionCase>
  GetByCaseNumberAsync(string caseNumber, Guid tenantId): CollectionCase?
  GetByStatusAsync(CollectionStatus status, Guid tenantId): List<CollectionCase>
  GetPagedAsync(Guid tenantId, CollectionFilter filter): PagedResult<CollectionCase>
  GetByAgentIdAsync(Guid agentId, CollectionStatus status): List<CollectionCase>
  GetOverdueActionAsync(Guid tenantId): List<CollectionCase>
  AddAsync(CollectionCase case)
  UpdateAsync(CollectionCase case)
  CountByStatusAsync(Guid tenantId): Dictionary<CollectionStatus, int>

IDebtRepository:
  GetByCaseIdAsync(Guid caseId): List<Debt>
  GetByInvoiceIdAsync(Guid invoiceId): Debt?
  AddAsync(Debt debt)
  UpdateAsync(Debt debt)

ICollectionActionRepository:
  GetByCaseIdAsync(Guid caseId): List<CollectionAction>
  AddAsync(CollectionAction action)

ICollectionStrategyRepository:
  GetByIdAsync(Guid id): CollectionStrategy?
  GetByTenantAndBucketAsync(Guid tenantId, AgingBucket bucket): List<CollectionStrategy>
  GetActiveAsync(Guid tenantId): List<CollectionStrategy>
  AddAsync(CollectionStrategy strategy)
  UpdateAsync(CollectionStrategy strategy)
  DeleteAsync(CollectionStrategy strategy)
```

#### Domain Rules / Invariants

- Case number must be unique per tenant
- A debt can only belong to one active collection case at a time
- Write-off requires management approval above configurable amount
- Promise to pay count is tracked per debt; excessive broken promises escalate case
- Settlement amount cannot be less than minimum threshold percentage
- Collection case cannot be closed while debts remain active (unless write-off)
- External agency cases have different SLA and reporting requirements
- Cases in escalated status must be reviewed weekly
- Payment plans require at least 2 installments

### 10.2 Application Layer

#### Commands

```
OpenCollectionCaseCommand       { TenantId, CustomerId, InvoiceIds[], Notes }
AssignCollectionAgentCommand   { CaseId, AgentId }
RecordCollectionActionCommand   { CaseId, Type, Description, Amount, ContactMethod, Duration, Outcome }
AddDebtToCaseCommand            { CaseId, InvoiceId }
RemoveDebtFromCaseCommand       { CaseId, DebtId }
RecordPromiseToPayCommand       { CaseId, Amount, PromiseDate, ContactMethod }
RecordBrokenPromiseCommand      { CaseId, DebtId }
ProposeSettlementCommand        { CaseId, Amount, PaymentDate }
AcceptSettlementCommand         { CaseId, Amount, PaymentDate }
WriteOffDebtCommand             { CaseId, DebtId, Reason, ApprovedBy }
EscalateCaseCommand             { CaseId, Reason }
CloseCollectionCaseCommand      { CaseId, Reason, Result }
ReopenCollectionCaseCommand     { CaseId, Reason }
CreatePaymentPlanCommand        { CaseId, Installments[], Notes }
UpdatePaymentPlanCommand        { PlanId, Installments[], Status }
CreateCollectionStrategyCommand { TenantId, Name, Description, AgingBucket, ActionSequence, EscalationAfterDays, WriteOffAfterDays }
UpdateCollectionStrategyCommand { StrategyId, Name, Description, ActionSequence }
DeleteCollectionStrategyCommand { StrategyId }
BulkCreateFromOverdueInvoicesCommand { TenantId, ThresholdDays }
SendCollectionLetterCommand     { CaseId, LetterType }
GenerateCollectionReportCommand { TenantId, FromDate, ToDate }
```

#### Queries

```
GetCollectionCaseQuery          { CaseId }
GetCollectionCaseByNumberQuery  { CaseNumber, TenantId }
ListCollectionCasesQuery        { TenantId, Status, Priority, AgentId, CustomerId, Page, Size }
GetCustomerCollectionCasesQuery { CustomerId }
GetCollectionCaseDebtsQuery     { CaseId }
GetCollectionCaseActionsQuery   { CaseId, Page, Size }
GetCollectionCaseTimelineQuery  { CaseId }
ListCollectionStrategiesQuery   { TenantId }
GetCollectionStatsQuery         { TenantId }
GetAgingReportQuery             { TenantId }
GetAgentPerformanceQuery        { TenantId, AgentId, FromDate, ToDate }
GetCollectionForecastQuery      { TenantId }
```

#### Command Handlers

```
OpenCollectionCaseHandler       -> Validates invoices are overdue, creates case with debts, publishes CollectionCaseOpenedEvent
AssignCollectionAgentHandler    -> Assigns agent, publishes CollectionCaseAssignedEvent
RecordCollectionActionHandler   -> Records action, triggers strategy evaluation, publishes CollectionActionRecordedEvent
RecordPromiseToPayHandler       -> Records promise, updates debt, publishes PromiseToPayReceivedEvent
WriteOffDebtHandler             -> Validates approval, writes off, publishes DebtWrittenOffEvent
EscalateCaseHandler             -> Escalates, publishes CollectionCaseEscalatedEvent
CloseCollectionCaseHandler      -> Validates all debts resolved, closes, publishes CollectionCaseClosedEvent
AcceptSettlementHandler         -> Validates settlement, records agreement, publishes SettlementAgreedEvent
```

#### Query Handlers

```
GetCollectionCaseHandler        -> Returns CollectionCaseDetailDto
ListCollectionCasesHandler      -> Returns PagedResult<CollectionCaseSummaryDto>
GetCollectionCaseDebtsHandler   -> Returns List<DebtDto>
GetCollectionCaseActionsHandler -> Returns PagedResult<CollectionActionDto>
GetCollectionStatsHandler       -> Returns CollectionStatsDto
GetAgingReportHandler           -> Returns AgingReportDto (by bucket, totals, percentages)
GetAgentPerformanceHandler      -> Returns AgentPerformanceDto (cases resolved, amounts collected, actions per case)
```

#### Application Services

``` 
ICollectionOrchestrator:
  OpenCaseForOverdueInvoice(Invoice invoice): CollectionCase
  EvaluateAndExecuteNextAction(CollectionCase case): ActionResult
  GenerateDunningLetter(CollectionCase case, string letterType): byte[]
  CalculateCollectionsForecast(DateTime fromDate): ForecastResult

ICollectionLetterService:
  GenerateLetter(CollectionCase case, string template): byte[]
  SendLetter(CollectionCase case, string template, string method): DeliveryResult
  BatchSendLetters(List<CollectionCase> cases, string template): BatchDeliveryResult

IExternalAgencyIntegration:
  SendCaseToAgency(CollectionCase case, string agencyId): ExternalCaseResult
  UpdateAgencyStatus(CollectionCase case, ExternalCaseStatus status)
  ReceiveAgencyReport(string agencyId, DateTime from, DateTime to): AgencyReport
```

#### DTOs

```
CollectionCaseSummaryDto  { Id, CaseNumber, CustomerId, CustomerName, Status, Priority, TotalOutstanding, Currency, DaysOpen, AssignedAgent, DebtCount, LastActionDate }
CollectionCaseDetailDto   { Id, CaseNumber, CustomerId, CustomerName, Status, Priority, TotalOutstanding, Currency, Debts, Actions, AssignedAgent, OpenedAt, ClosedAt, CloseReason, Notes, Timeline }
DebtDto                   { Id, InvoiceId, InvoiceNumber, OriginalAmount, RemainingAmount, Currency, Status, DueDate, DaysOverdue, AgingBucket, LastPaymentDate, LastPromiseDate, PromiseCount }
CollectionActionDto       { Id, Type, Description, Amount, AgentName, Result, ContactMethod, Duration, Outcome, CreatedAt }
CollectionStrategyDto     { Id, Name, Description, AgingBucket, ActionSequence, EscalationAfterDays, WriteOffAfterDays, IsActive }
CollectionStatsDto        { TotalCases, OpenCases, ResolvedCases, WriteOffAmount, TotalCollected, RecoveryRate, AverageRecoveryTime, CasesByBucket }
AgingReportDto            { Bucket, Count, TotalAmount, Percentage, ActionsInPeriod }
AgentPerformanceDto       { AgentId, AgentName, CasesAssigned, CasesResolved, AmountsCollected, ActionsRecorded, AvgResolutionDays }
```

### 10.3 Infrastructure Layer

#### EF Core Entity Configurations

```
CollectionCaseConfiguration:
  Table: "collections_cases"
  HasIndex(cc => new { cc.TenantId, cc.CaseNumber }).IsUnique()
  HasIndex(cc => cc.CustomerId)
  HasIndex(cc => new { cc.TenantId, cc.Status })
  HasIndex(cc => new { cc.TenantId, cc.Priority, cc.CreatedAt })
  HasIndex(cc => cc.AssignedAgentId).HasFilter("assigned_agent_id IS NOT NULL")
  Property(cc => cc.Notes).HasColumnType("text")
  Property(cc => cc.Metadata).HasColumnType("jsonb")
  HasMany(cc => cc.Debts).WithOne().HasForeignKey(d => d.CollectionCaseId).OnDelete(DeleteBehavior.Cascade)
  HasMany(cc => cc.Actions).WithOne().HasForeignKey(ca => ca.CollectionCaseId).OnDelete(DeleteBehavior.Cascade)

DebtConfiguration:
  Table: "collections_debts"
  HasIndex(d => d.InvoiceId)
  HasIndex(d => d.Status)
  HasIndex(d => d.AgingBucket)

CollectionActionConfiguration:
  Table: "collections_actions"
  HasIndex(ca => ca.CollectionCaseId)
  HasIndex(ca => ca.CreatedAt)
  Property(ca => ca.Description).HasMaxLength(1000)
  Property(ca => ca.Outcome).HasMaxLength(500)

CollectionStrategyConfiguration:
  Table: "collections_strategies"
  HasIndex(cs => new { cs.TenantId, cs.Name }).IsUnique()
```

#### Repository Implementations

```
CollectionCaseRepository   -> EF Core with debts, actions; filtering by status, agent, priority
DebtRepository             -> EF Core
CollectionActionRepository -> EF Core
CollectionStrategyRepository -> EF Core
```

#### Integration Events

**Publishes:**
```
CollectionCaseOpenedEvent        -> Topic: "collections.case.opened"
CollectionCaseAssignedEvent      -> Topic: "collections.case.assigned"
CollectionCaseEscalatedEvent     -> Topic: "collections.case.escalated"
CollectionCaseClosedEvent        -> Topic: "collections.case.closed"
DebtWrittenOffEvent              -> Topic: "collections.debt.written_off"
DebtStatusChangedEvent           -> Topic: "collections.debt.status_changed"
PromiseToPayReceivedEvent        -> Topic: "collections.promise.received"
SettlementAgreedEvent            -> Topic: "collections.settlement.agreed"
```

**Subscribes to:**
```
invoices.invoice.overdue            -> Open collection case
billing.dunning_level.changed       -> Update collection priority
payments.payment.received           -> Update debt status if payment allocated to overdue invoice
crm.customer.status_changed         -> Update case based on customer status
```

#### Background Jobs

```
CollectionActionJob:
  Schedule: Daily
  Evaluates open cases for next action based on strategy
  Executes automated actions (SMS, email)
  Updates case status

PromiseVerificationJob:
  Schedule: Daily
  Verifies promises to pay
  Handles broken promises

AgingUpdateJob:
  Schedule: Daily
  Updates aging buckets for all debts
  Triggers strategy evaluation

WriteOffJob:
  Schedule: Monthly
  Identifies debts beyond write-off threshold
  Generates write-off report for approval

ExternalAgencySyncJob:
  Schedule: Hourly
  Syncs case status with external collection agencies
  Receives agency reports

CollectionReportJob:
  Schedule: Weekly
  Generates collection performance reports
  Sends to management
```

#### Caching Strategy

```
Cache: Collection case by ID (Redis, 15 minutes, evict on update)
Cache: Collection strategies (Redis, 1 hour)
Cache: Collection statistics (Redis, 1 hour, evict on case change)
Cache: Aging report (Redis, 1 hour)
```

#### External Service Integrations

```
External Collection Agencies:
  - Case referral API
  - Status update webhooks
  - Settlement reporting
  - Commission calculation

Credit Bureau (Experian, TransUnion):
  - Credit report pull for collection decisions
  - Debt reporting to credit bureaus
  - Credit score update on payment

SMS/Email Service:
  - Automated collection communications
  - Payment reminders
  - Settlement offers

Legal Case Management:
  - Case referral for legal action
  - Document generation (demand letters)
  - Court filing integration
```

### 10.4 API Layer

#### Endpoints

```
# Collection Cases
GET    /api/v1/collections/cases                          -> ListCollectionCases         [Authenticated, Collections.Read]
POST   /api/v1/collections/cases                          -> OpenCollectionCase          [Admin, Collections.Create]
POST   /api/v1/collections/cases/bulk                     -> BulkCreateFromOverdueInvoices [Admin, Collections.Admin]
GET    /api/v1/collections/cases/{id}                     -> GetCollectionCase           [Authenticated, Collections.Read]
POST   /api/v1/collections/cases/{id}/assign              -> AssignCollectionAgent       [Admin, Collections.Update]
POST   /api/v1/collections/cases/{id}/actions             -> RecordCollectionAction      [Authenticated, Collections.Update]
POST   /api/v1/collections/cases/{id}/promise             -> RecordPromiseToPay          [Authenticated, Collections.Update]
POST   /api/v1/collections/cases/{id}/broken-promise      -> RecordBrokenPromise         [Authenticated, Collections.Update]
POST   /api/v1/collections/cases/{id}/settlement          -> ProposeSettlement           [Admin, Collections.Admin]
POST   /api/v1/collections/cases/{id}/settlement/accept   -> AcceptSettlement            [Admin, Collections.Admin]
POST   /api/v1/collections/cases/{id}/write-off           -> WriteOffDebt                [Admin, Collections.Admin]
POST   /api/v1/collections/cases/{id}/escalate            -> EscalateCase                [Admin, Collections.Admin]
POST   /api/v1/collections/cases/{id}/close               -> CloseCollectionCase         [Admin, Collections.Admin]
POST   /api/v1/collections/cases/{id}/reopen              -> ReopenCollectionCase        [Admin, Collections.Admin]
GET    /api/v1/collections/cases/{id}/debts               -> GetCollectionCaseDebts      [Authenticated, Collections.Read]
GET    /api/v1/collections/cases/{id}/actions             -> GetCollectionCaseActions    [Authenticated, Collections.Read]
GET    /api/v1/collections/cases/{id}/timeline            -> GetCollectionCaseTimeline   [Authenticated, Collections.Read]
POST   /api/v1/collections/cases/{id}/letter              -> SendCollectionLetter        [Admin, Collections.Update]

# Strategies
GET    /api/v1/collections/strategies                     -> ListCollectionStrategies    [Admin, Collections.Admin]
POST   /api/v1/collections/strategies                     -> CreateCollectionStrategy    [Admin, Collections.Admin]
PUT    /api/v1/collections/strategies/{id}                -> UpdateCollectionStrategy    [Admin, Collections.Admin]
DELETE /api/v1/collections/strategies/{id}                -> DeleteCollectionStrategy    [Admin, Collections.Admin]

# Reports
GET    /api/v1/collections/reports/stats                  -> GetCollectionStats          [Admin, Collections.Read]
GET    /api/v1/collections/reports/aging                  -> GetAgingReport              [Admin, Collections.Read]
GET    /api/v1/collections/reports/agent-performance      -> GetAgentPerformance         [Admin, Collections.Read]
GET    /api/v1/collections/reports/forecast               -> GetCollectionForecast       [Admin, Collections.Read]

# Customer
GET    /api/v1/collections/customers/{customerId}         -> GetCustomerCollectionCases  [Authenticated, Collections.Read]
```

#### Request/Response Models

```
OpenCollectionCaseRequest      { CustomerId, InvoiceIds[], Notes }
AssignAgentRequest             { AgentId }
RecordActionRequest            { Type, Description, Amount, ContactMethod, Duration, Outcome }
PromiseRequest                 { Amount, PromiseDate, ContactMethod }
SettlementRequest              { Amount, PaymentDate }
WriteOffRequest                { DebtId, Reason, ApprovedBy }
EscalateRequest                { Reason }
CloseCaseRequest               { Reason, Result }
CreateStrategyRequest          { Name, Description, AgingBucket, ActionSequence, EscalationAfterDays, WriteOffAfterDays }
CollectionFilterRequest        { Status, Priority, AgentId, CustomerId, AgingBucket, Page, Size }
```

#### Authorization Requirements

```
Collections.Read: View cases, debts, actions
Collections.Create: Open new collection cases
Collections.Update: Record actions, promises, case management
Collections.Admin: Write-off, settle, escalate, close, manage strategies
```

#### Rate Limiting

```
Case operations: 30 requests/minute
Action recording: 60 requests/minute
Bulk case creation: 5 requests/minute
Report generation: 10 requests/minute
```

### 10.5 Events

#### Published Events

| Event | Topic | Schema |
|-------|-------|--------|
| CollectionCaseOpened | `collections.case.opened` | `{ CaseId, CaseNumber, CustomerId, TenantId, TotalOutstanding, Currency, DebtCount, OccurredAt }` |
| CollectionCaseAssigned | `collections.case.assigned` | `{ CaseId, CaseNumber, AgentId, AgentName, OccurredAt }` |
| CollectionCaseEscalated | `collections.case.escalated` | `{ CaseId, CaseNumber, Reason, OccurredAt }` |
| CollectionCaseClosed | `collections.case.closed` | `{ CaseId, CaseNumber, Result, Reason, TotalRecovered, OccurredAt }` |
| DebtWrittenOff | `collections.debt.written_off` | `{ DebtId, CaseId, InvoiceId, Amount, Reason, OccurredAt }` |
| DebtStatusChanged | `collections.debt.status_changed` | `{ DebtId, CaseId, InvoiceId, OldStatus, NewStatus, OccurredAt }` |
| PromiseToPayReceived | `collections.promise.received` | `{ PromiseId, CaseId, CustomerId, Amount, PromiseDate, OccurredAt }` |
| SettlementAgreed | `collections.settlement.agreed` | `{ SettlementId, CaseId, CustomerId, SettlementAmount, OriginalAmount, Percentage, OccurredAt }` |

#### Subscribed Events

| Event | Source | Action |
|-------|--------|--------|
| `invoices.invoice.overdue` | Invoices | Auto-open collection case |
| `payments.payment.received` | Payments | Update debt status if allocated to overdue invoice |
| `crm.customer.status_changed` | CRM | Update case status based on customer |

### 10.6 Data Ownership

#### Owned Tables

| Table | Type | Purpose |
|-------|------|---------|
| `collections_cases` | Primary | Collection case records |
| `collections_debts` | Primary | Debts within cases |
| `collections_actions` | Secondary | Collection actions taken |
| `collections_strategies` | Primary | Collection strategy definitions |
| `collections_payment_plans` | Secondary | Payment plan agreements |

#### Foreign Key Relationships

```
collections_cases.tenant_id -> iam_tenants.id (RESTRICT)
collections_cases.customer_id -> crm_customers.id (RESTRICT)
collections_debts.collection_case_id -> collections_cases.id (CASCADE)
collections_debts.invoice_id -> invoices.id (RESTRICT)
collections_actions.collection_case_id -> collections_cases.id (CASCADE)
collections_strategies.tenant_id -> iam_tenants.id (RESTRICT)
collections_payment_plans.collection_case_id -> collections_cases.id (CASCADE)
```

### 10.7 Dependencies

#### Depends On

| Module | Reason |
|--------|--------|
| IAM | Tenant context |
| CRM | Customer reference |
| Invoices | Overdue invoice reference on debts |

#### Depended On By

| Module | Reason |
|--------|--------|
| Notifications | Collection communications |
| Reporting | Collections analytics |

### 10.8 Security Requirements

#### RBAC Roles & Permissions

```
Module-level:
  Collections.Read     -- View collection cases, debts, actions
  Collections.Create   -- Open new collection cases
  Collections.Update   -- Record actions, promises, manage cases
  Collections.Admin    -- Write-off, settle, escalate, manage strategies

Role defaults:
  Admin: All permissions
  CollectionsAgent: Collections.Read, Collections.Create, Collections.Update
  BillingAdmin: Collections.Read
  ReadOnly: Collections.Read
```

#### Data Isolation Rules

- Collection cases are tenant-scoped
- Agents can only see assigned cases (configurable)
- Write-off amounts require dual approval (agent + manager)
- Settlement agreements are legally binding -- require documented approval
- Communication with debtors must comply with fair debt collection practices
- External agency data sharing requires customer consent

#### Audit Requirements

- All collection actions recorded (type, timestamp, agent, outcome)
- Case assignment changes
- Write-off approvals (who approved, amount, reason)
- Settlement agreements (terms, amounts, approver)
- Promise to pay records and verification results
- Escalation events
- External agency communications
- Payment plan creation and modifications

---

## 11. ServiceInventory — Service Inventory

### 11.1 Domain Layer

#### Aggregate: Service

```
Id: Guid
TenantId: Guid
CustomerId: Guid
SubscriptionId: Guid?
SubscriptionProductId: Guid?
ServiceType: ServiceType           (Internet, Voice, TV, VPN, IoT, Cloud, Hosting, Email)
Name: string
Status: ServiceStatus              (Pending, Active, Suspended, Disconnected, Terminated)
ActivationDate: DateTime?
SuspensionDate: DateTime?
TerminationDate: DateTime?
Characteristics: Dictionary<string, object>
Endpoints: List<ServiceEndpoint>
AssignedResources: List<ServiceResource>
ParentServiceId: Guid?
RelatedServices: List<ServiceRelation>
Location: ServiceLocation?
ProvisioningRequestId: Guid?
OrderItemId: Guid?
Notes: string?
ExternalReference: string?

Methods:
  Activate(DateTime activationDate)
  Suspend(string reason)
  Unsuspend()
  Disconnect(string reason)
  Terminate(DateTime terminationDate, string reason)
  AssignResource(ServiceResource resource)
  RemoveResource(Guid resourceId)
  AddEndpoint(ServiceEndpoint endpoint)
  GetEndpointByType(string endpointType): ServiceEndpoint
```

#### Entity: ServiceEndpoint

```
Id: Guid
ServiceId: Guid
Type: EndpointType                (IPAddress, PhoneNumber, Username, URL, Port, VLAN, MACAddress)
Value: string
Label: string?
IsPrimary: bool
Status: EndpointStatus            (Active, Inactive, Reserved)
AssignedAt: DateTime
ReleasedAt: DateTime?
```

#### Entity: ServiceResource

```
Id: Guid
ServiceId: Guid
ResourceType: ResourceType       (Bandwidth, Storage, Port, IPRange, DevicePort, VLAN, License)
ResourceId: Guid                 (reference to NetworkInventory resource)
Quantity: int
Unit: string
AllocatedAt: DateTime
ReleasedAt: DateTime?
```

#### Value Object: ServiceLocation

```
Address: AddressSnapshot
Latitude: double?
Longitude: double?
ServiceTerminalId: string?
CabinetId: string?
PortId: string?
```

#### Entity: ServiceRelation

```
Id: Guid
ServiceId: Guid
RelatedServiceId: Guid
RelationType: RelationType        (Parent, Child, Peer, Bundle, Backup, Redundant)
```

#### Value Objects

```
ServiceType: enum { Internet, Voice, TV, VPN, IoT, Cloud, Hosting, Email, SMS, MMS, Data, LeasedLine, Colocation }
ServiceStatus: enum { Pending, Active, Suspended, Disconnected, Terminated, Provisioning }
EndpointType: enum { IPAddress, PhoneNumber, Username, URL, Port, VLAN, MACAddress, IMSI, IMEI, ICCID, SIP }
EndpointStatus: enum { Active, Inactive, Reserved }
ResourceType: enum { Bandwidth, Storage, Port, IPRange, DevicePort, VLAN, License, Capacity, Channel }
RelationType: enum { Parent, Child, Peer, Bundle, Backup, Redundant, Component }
```

#### Domain Events

```
ServiceCreatedEvent           { ServiceId, CustomerId, SubscriptionId, ServiceType, OccurredAt }
ServiceActivatedEvent         { ServiceId, ActivationDate, OccurredAt }
ServiceSuspendedEvent         { ServiceId, Reason, OccurredAt }
ServiceUnsuspendedEvent       { ServiceId, OccurredAt }
ServiceDisconnectedEvent      { ServiceId, Reason, OccurredAt }
ServiceTerminatedEvent        { ServiceId, Reason, OccurredAt }
ServiceEndpointAssignedEvent  { ServiceId, EndpointId, EndpointType, Value, OccurredAt }
ServiceEndpointReleasedEvent  { ServiceId, EndpointId, EndpointType, OccurredAt }
ServiceResourceAllocatedEvent { ServiceId, ResourceType, ResourceId, Quantity, OccurredAt }
ServiceResourceReleasedEvent  { ServiceId, ResourceType, ResourceId, OccurredAt }
```

#### Domain Services

```
IServiceLifecycleService:
  ActivateService(Service service): Result
  SuspendService(Service service, string reason): Result
  TerminateService(Service service, string reason): Result

IServiceValidationService:
  ValidateServiceConfiguration(ServiceType type, Dictionary<string, object> characteristics): ValidationResult
  CheckEndpointAvailability(EndpointType type, string value): bool
  ValidateServiceDependencies(Service service): ValidationResult
```

#### Repository Interfaces

```
IServiceRepository:
  GetByIdAsync(Guid id): Service? (with endpoints, resources, relations)
  GetByCustomerIdAsync(Guid customerId): List<Service>
  GetBySubscriptionIdAsync(Guid subscriptionId): List<Service>
  GetByTypeAsync(ServiceType type, Guid tenantId): List<Service>
  GetByStatusAsync(ServiceStatus status, Guid tenantId): List<Service>
  GetByEndpointAsync(EndpointType type, string value): Service?
  GetPagedAsync(Guid tenantId, ServiceFilter filter): PagedResult<Service>
  AddAsync(Service service)
  UpdateAsync(Service service)
  CountByStatusAsync(Guid tenantId): Dictionary<ServiceStatus, int>

IServiceEndpointRepository:
  GetByServiceIdAsync(Guid serviceId): List<ServiceEndpoint>
  GetByValueAsync(string value): ServiceEndpoint?
  AddAsync(ServiceEndpoint endpoint)
  UpdateAsync(ServiceEndpoint endpoint)
  DeleteAsync(ServiceEndpoint endpoint)
  IsEndpointAvailable(EndpointType type, string value): bool
```

#### Domain Rules / Invariants

- Service endpoints must be unique across tenant (e.g., IP address, phone number)
- A service cannot be activated without required characteristics configured
- Cannot suspend an already suspended service
- Resource allocation must have sufficient available inventory
- Services with active child services cannot be terminated
- Endpoint types are service-type specific (e.g., phone number for Voice)
- Service location is required for physical services (internet, leased line)

### 11.2 Application Layer

#### Commands

```
CreateServiceCommand            { TenantId, CustomerId, SubscriptionId, SubscriptionProductId, ServiceType, Name, Characteristics, Location, OrderItemId }
ActivateServiceCommand          { ServiceId, ActivationDate }
SuspendServiceCommand           { ServiceId, Reason }
UnsuspendServiceCommand         { ServiceId }
DisconnectServiceCommand        { ServiceId, Reason }
TerminateServiceCommand         { ServiceId, Reason, TerminationDate }
AssignEndpointCommand           { ServiceId, Type, Value, Label, IsPrimary }
ReleaseEndpointCommand          { ServiceId, EndpointId }
AssignResourceCommand           { ServiceId, ResourceType, ResourceId, Quantity }
ReleaseResourceCommand          { ServiceId, ResourceId }
UpdateServiceCharacteristicsCommand { ServiceId, Characteristics }
BulkUpdateServiceStatusCommand  { ServiceIds[], NewStatus }
```

#### Queries

```
GetServiceQuery                 { ServiceId }
GetServiceByEndpointQuery       { EndpointType, Value }
ListServicesQuery               { TenantId, CustomerId, SubscriptionId, ServiceType, Status, Page, Size }
GetCustomerServicesQuery        { CustomerId }
GetSubscriptionServicesQuery    { SubscriptionId }
SearchServicesQuery             { TenantId, SearchTerm, Page, Size }
GetServiceEndpointsQuery        { ServiceId }
GetServiceResourcesQuery        { ServiceId }
GetServiceRelationsQuery        { ServiceId }
GetServiceStatsQuery            { TenantId }
GetServicesByTypeQuery          { TenantId, ServiceType }
```

#### Command/Query Handlers

```
CreateServiceHandler            -> Validates, creates service, publishes ServiceCreatedEvent
ActivateServiceHandler          -> Validates preconditions, activates, assigns endpoints, publishes ServiceActivatedEvent
SuspendServiceHandler           -> Suspends, publishes ServiceSuspendedEvent
TerminateServiceHandler         -> Validates no child services, releases resources, terminates, publishes ServiceTerminatedEvent
AssignEndpointHandler           -> Validates availability, assigns endpoint, publishes ServiceEndpointAssignedEvent
ReleaseEndpointHandler          -> Releases endpoint, publishes ServiceEndpointReleasedEvent
```

#### DTOs

```
ServiceSummaryDto     { Id, CustomerId, CustomerName, SubscriptionId, ServiceType, Name, Status, ActivationDate, EndpointCount, ResourceCount, CreatedAt }
ServiceDetailDto      { Id, CustomerId, CustomerName, SubscriptionId, ServiceType, Name, Status, Characteristics, Endpoints, Resources, Relations, Location, ActivationDate, SuspensionDate, TerminationDate, Timeline }
ServiceEndpointDto    { Id, Type, Value, Label, IsPrimary, Status, AssignedAt }
ServiceResourceDto    { Id, ResourceType, ResourceId, ResourceName, Quantity, Unit, AllocatedAt }
```

### 11.3 Infrastructure Layer

#### EF Core Entity Configurations

```
ServiceConfiguration:
  Table: "service_inventory"
  HasIndex(s => s.TenantId)
  HasIndex(s => s.CustomerId)
  HasIndex(s => s.SubscriptionId)
  HasIndex(s => new { s.TenantId, s.ServiceType })
  HasIndex(s => new { s.TenantId, s.Status })
  Property(s => s.Characteristics).HasColumnType("jsonb")
  Property(s => s.Notes).HasColumnType("text")
  OwnsOne(s => s.Location)
  HasMany(s => s.Endpoints).WithOne().HasForeignKey(se => se.ServiceId).OnDelete(DeleteBehavior.Cascade)
  HasMany(s => s.AssignedResources).WithOne().HasForeignKey(sr => sr.ServiceId).OnDelete(DeleteBehavior.Cascade)
  HasMany(s => s.RelatedServices).WithOne().HasForeignKey(sr => sr.ServiceId).OnDelete(DeleteBehavior.Cascade)

ServiceEndpointConfiguration:
  Table: "service_endpoints"
  HasIndex(se => new { se.Type, se.Value }).IsUnique()
  HasIndex(se => se.ServiceId)

ServiceResourceConfiguration:
  Table: "service_resources"
  HasIndex(sr => new { sr.ServiceId, sr.ResourceId })
  HasIndex(sr => sr.ResourceId)
```

#### Integration Events

**Publishes:**
```
ServiceCreatedEvent           -> Topic: "service_inventory.service.created"
ServiceActivatedEvent         -> Topic: "service_inventory.service.activated"
ServiceSuspendedEvent         -> Topic: "service_inventory.service.suspended"
ServiceDisconnectedEvent      -> Topic: "service_inventory.service.disconnected"
ServiceTerminatedEvent        -> Topic: "service_inventory.service.terminated"
```

**Subscribes to:**
```
provisioning.request.completed    -> Create service from provisioned resources
subscription.product_added        -> Create pending service
subscription.activated            -> Activate services
subscription.suspended            -> Suspend services
subscription.terminated           -> Terminate services
```

#### Caching Strategy

```
Cache: Service by ID (Redis, 15 minutes)
Cache: Services by customer (Redis, 30 minutes)
Cache: Endpoint lookup (Redis, 1 hour)
Cache: Service counts by status (Redis, 1 hour)
```

### 11.4 API Layer

#### Endpoints

```
GET    /api/v1/service-inventory/services                           -> ListServices       [Authenticated, ServiceInventory.Read]
POST   /api/v1/service-inventory/services                           -> CreateService       [Admin, ServiceInventory.Create]
GET    /api/v1/service-inventory/services/{id}                      -> GetService          [Authenticated, ServiceInventory.Read]
POST   /api/v1/service-inventory/services/{id}/activate             -> ActivateService     [Admin, ServiceInventory.Update]
POST   /api/v1/service-inventory/services/{id}/suspend              -> SuspendService      [Admin, ServiceInventory.Update]
POST   /api/v1/service-inventory/services/{id}/unsuspend            -> UnsuspendService    [Admin, ServiceInventory.Update]
POST   /api/v1/service-inventory/services/{id}/disconnect           -> DisconnectService   [Admin, ServiceInventory.Update]
POST   /api/v1/service-inventory/services/{id}/terminate            -> TerminateService    [Admin, ServiceInventory.Admin]
GET    /api/v1/service-inventory/services/{id}/endpoints            -> GetServiceEndpoints [Authenticated, ServiceInventory.Read]
POST   /api/v1/service-inventory/services/{id}/endpoints            -> AssignEndpoint      [Admin, ServiceInventory.Update]
DELETE /api/v1/service-inventory/services/{id}/endpoints/{epId}     -> ReleaseEndpoint     [Admin, ServiceInventory.Update]
GET    /api/v1/service-inventory/services/{id}/resources            -> GetServiceResources [Authenticated, ServiceInventory.Read]
POST   /api/v1/service-inventory/services/{id}/resources            -> AssignResource      [Admin, ServiceInventory.Update]
DELETE /api/v1/service-inventory/services/{id}/resources/{resId}    -> ReleaseResource     [Admin, ServiceInventory.Update]
GET    /api/v1/service-inventory/services/by-endpoint               -> GetServiceByEndpoint [Admin, ServiceInventory.Read]
GET    /api/v1/service-inventory/services/stats                     -> GetServiceStats     [Admin, ServiceInventory.Read]
```

### 11.5 Dependencies

| Module | Reason |
|--------|--------|
| IAM | Tenant context |
| CRM | Customer reference |
| Subscriptions | Subscription reference |
| NetworkInventory | Resource references |

**Depended on by:** Provisioning, NetworkInventory, Ticketing

## 12. NetworkInventory — Network Inventory

### 12.1 Domain Layer

#### Aggregate: NetworkDevice

```
Id: Guid
TenantId: Guid
Name: string
Hostname: string
DeviceType: DeviceType          (Router, Switch, Firewall, AccessPoint, BaseStation, Server, Storage)
Vendor: string
Model: string
SerialNumber: string
SoftwareVersion: string
OsType: string?
ManagementIp: string
Status: DeviceStatus            (Online, Offline, Degraded, Maintenance, Provisioning)
Location: DeviceLocation?
RackPosition: string?
Ports: List<DevicePort>
Interfaces: List<NetworkInterface>
Tags: List<string>
MonitoringEnabled: bool
LastSeenAt: DateTime?
WarrantyExpiry: DateTime?
PurchaseDate: DateTime?
CreatedAt: DateTime
UpdatedAt: DateTime

Methods:
  Online()
  Offline(string reason)
  Maintenance(string reason)
  AddPort(DevicePort port)
  RemovePort(Guid portId)
  UpdateSoftware(string version)
```

#### Entity: DevicePort

```
Id: Guid
DeviceId: Guid
PortNumber: int
PortName: string
PortType: PortType              (Ethernet, Fiber, SFP, QSFP, Console, Management)
Speed: string?
MediaType: string?
Status: PortStatus              (Up, Down, Disabled, Shutdown)
IsUsed: bool
ConnectedToDeviceId: Guid?
ConnectedToPortId: Guid?
VlanId: int?
VlanMode: VlanMode              (Access, Trunk, Hybrid)
```

#### Entity: NetworkInterface

```
Id: Guid
DeviceId: Guid
Name: string
IpAddress: string
SubnetMask: string?
Gateway: string?
MacAddress: string?
Type: InterfaceType             (Loopback, Physical, VLAN, Tunnel, LAG)
VlanId: int?
Mtu: int?
Status: InterfaceStatus         (Up, Down, AdminDown)
```

#### Entity: IPResource

```
Id: Guid
TenantId: Guid
IpAddress: string
Subnet: string
Type: IpType                   (IPv4, IPv6)
Status: IpStatus               (Available, Reserved, Allocated, Deprecated)
PoolId: Guid?
DeviceId: Guid?
ServiceId: Guid?
AssignedAt: DateTime?
ReleasedAt: DateTime?
```

#### Entity: IPPool

```
Id: Guid
TenantId: Guid
Name: string
Network: string
Cidr: string
Gateway: string?
StartRange: string
EndRange: string
Type: IpType
Status: PoolStatus              (Active, Full, Deprecated)
TotalIps: int
UsedIps: int
AvailableIps: int
```

#### Value Objects

```
DeviceType: enum { Router, Switch, Firewall, AccessPoint, BaseStation, Server, Storage, LoadBalancer, DPI, BRAS, OLT, ONT }
DeviceStatus: enum { Online, Offline, Degraded, Maintenance, Provisioning, Decommissioned }
PortType: enum { Ethernet, Fiber, SFP, QSFP, Console, Management, PoE, USB }
PortStatus: enum { Up, Down, Disabled, Shutdown }
InterfaceType: enum { Loopback, Physical, VLAN, Tunnel, LAG, Virtual }
InterfaceStatus: enum { Up, Down, AdminDown }
IpType: enum { IPv4, IPv6, IPv4_Private }
IpStatus: enum { Available, Reserved, Allocated, Deprecated }
PoolStatus: enum { Active, Full, Deprecated }
VlanMode: enum { Access, Trunk, Hybrid }
```

#### Domain Events

```
NetworkDeviceCreatedEvent     { DeviceId, Name, DeviceType, Vendor, Model, OccurredAt }
NetworkDeviceStatusChanged   { DeviceId, OldStatus, NewStatus, OccurredAt }
DevicePortConnectedEvent      { DeviceId, PortId, ConnectedDeviceId, ConnectedPortId, OccurredAt }
DevicePortDisconnectedEvent   { DeviceId, PortId, OccurredAt }
IPAddressAllocatedEvent       { IpAddress, DeviceId, ServiceId, OccurredAt }
IPAddressReleasedEvent        { IpAddress, OccurredAt }
IPPoolExhaustedEvent          { PoolId, Name, OccurredAt }
```

#### Repository Interfaces

```
INetworkDeviceRepository:
  GetByIdAsync(Guid id): NetworkDevice?
  GetByHostnameAsync(string hostname): NetworkDevice?
  GetByTypeAsync(DeviceType type, Guid tenantId): List<NetworkDevice>
  GetPagedAsync(Guid tenantId, DeviceFilter filter): PagedResult<NetworkDevice>
  AddAsync(NetworkDevice device)
  UpdateAsync(NetworkDevice device)

IIPResourceRepository:
  GetByAddressAsync(string ipAddress): IPResource?
  GetAvailableByPoolAsync(Guid poolId, int count): List<IPResource>
  GetAllocateByServiceAsync(Guid serviceId): List<IPResource>
  AddAsync(IPResource resource)
  AddRangeAsync(List<IPResource> resources)
  UpdateAsync(IPResource resource)

IIPPoolRepository:
  GetByIdAsync(Guid id): IPPool?
  GetByTenantAsync(Guid tenantId): List<IPPool>
  AddAsync(IPPool pool)
  UpdateAsync(IPPool pool)
```

### 12.2 Application Layer

#### Commands

```
CreateNetworkDeviceCommand    { TenantId, Name, Hostname, DeviceType, Vendor, Model, SerialNumber, ManagementIp, Location, Ports, Interfaces }
UpdateNetworkDeviceCommand    { DeviceId, Name, Hostname, SoftwareVersion, Location }
ChangeDeviceStatusCommand     { DeviceId, NewStatus, Reason }
AddDevicePortCommand          { DeviceId, PortNumber, PortName, PortType, Speed }
ConnectPortsCommand           { DeviceId, PortId, TargetDeviceId, TargetPortId }
DisconnectPortCommand         { DeviceId, PortId }
AllocateIPCommand             { PoolId, ServiceId, Count }
ReleaseIPCommand              { IpAddress }
CreateIPPoolCommand           { TenantId, Name, Network, Cidr, Gateway, StartRange, EndRange, Type }
UpdateIPPoolCommand           { PoolId, Name, Gateway }
DiscoverDevicesCommand        { TenantId, Subnet }
SyncDeviceStatusCommand       { DeviceId }
```

#### Queries

```
GetNetworkDeviceQuery         { DeviceId }
ListNetworkDevicesQuery       { TenantId, DeviceType, Status, Vendor, Page, Size }
GetDevicePortsQuery           { DeviceId }
GetDeviceInterfacesQuery      { DeviceId }
GetIPPoolQuery                { PoolId }
ListIPPoolsQuery              { TenantId }
GetPoolUsageQuery             { PoolId }
GetAvailableIPsQuery          { PoolId, Count }
SearchIPAddressesQuery        { TenantId, SearchTerm, Status, Page, Size }
GetNetworkMapQuery            { TenantId }
GetDeviceStatsQuery           { TenantId }
```

### 12.3 Infrastructure Layer

#### EF Core Configurations

```
NetworkDeviceConfiguration:
  Table: "network_devices"
  HasIndex(nd => new { nd.TenantId, nd.Hostname }).IsUnique()
  HasIndex(nd => nd.SerialNumber).IsUnique()
  HasIndex(nd => new { nd.TenantId, nd.DeviceType })
  HasIndex(nd => new { nd.TenantId, nd.Status })
  OwnsOne(nd => nd.Location)
  HasMany(nd => nd.Ports).WithOne().HasForeignKey(dp => dp.DeviceId).OnDelete(DeleteBehavior.Cascade)
  HasMany(nd => nd.Interfaces).WithOne().HasForeignKey(ni => ni.DeviceId).OnDelete(DeleteBehavior.Cascade)

IPResourceConfiguration:
  Table: "network_ip_resources"
  HasIndex(ip => ip.IpAddress).IsUnique()
  HasIndex(ip => ip.PoolId)
  HasIndex(ip => new { ip.Status, ip.PoolId })

IPPoolConfiguration:
  Table: "network_ip_pools"
  HasIndex(p => new { p.TenantId, p.Name }).IsUnique()
```

### 12.4 API Layer

```
GET    /api/v1/network/devices                          -> ListNetworkDevices    [Admin, Network.Read]
POST   /api/v1/network/devices                          -> CreateNetworkDevice   [Admin, Network.Admin]
GET    /api/v1/network/devices/{id}                     -> GetNetworkDevice      [Admin, Network.Read]
PUT    /api/v1/network/devices/{id}                     -> UpdateNetworkDevice   [Admin, Network.Admin]
PATCH  /api/v1/network/devices/{id}/status              -> ChangeDeviceStatus    [Admin, Network.Admin]
GET    /api/v1/network/devices/{id}/ports               -> GetDevicePorts        [Admin, Network.Read]
POST   /api/v1/network/devices/{id}/ports               -> AddDevicePort         [Admin, Network.Admin]
POST   /api/v1/network/devices/{id}/ports/connect       -> ConnectPorts          [Admin, Network.Admin]
POST   /api/v1/network/devices/{id}/ports/disconnect    -> DisconnectPort        [Admin, Network.Admin]
GET    /api/v1/network/devices/{id}/interfaces          -> GetDeviceInterfaces   [Admin, Network.Read]
POST   /api/v1/network/devices/discover                 -> DiscoverDevices       [Admin, Network.Admin]
GET    /api/v1/network/devices/stats                    -> GetDeviceStats        [Admin, Network.Read]

GET    /api/v1/network/ip-pools                         -> ListIPPools           [Admin, Network.Read]
POST   /api/v1/network/ip-pools                         -> CreateIPPool          [Admin, Network.Admin]
GET    /api/v1/network/ip-pools/{id}                    -> GetIPPool             [Admin, Network.Read]
PUT    /api/v1/network/ip-pools/{id}                    -> UpdateIPPool          [Admin, Network.Admin]
GET    /api/v1/network/ip-pools/{id}/usage              -> GetPoolUsage          [Admin, Network.Read]
POST   /api/v1/network/ip-pools/{id}/allocate           -> AllocateIP            [Admin, Network.Admin]

GET    /api/v1/network/ip-addresses                     -> SearchIPAddresses     [Admin, Network.Read]
POST   /api/v1/network/ip-addresses/release             -> ReleaseIP             [Admin, Network.Admin]
```

### 12.5 Dependencies

| Module | Reason |
|--------|--------|
| IAM | Tenant context |
| ServiceInventory | Service reference on IP allocations |

**Depended on by:** Provisioning, ServiceInventory

## 13. Provisioning — Service Provisioning

### 13.1 Domain Layer

#### Aggregate: ProvisioningRequest

```
Id: Guid
TenantId: Guid
RequestNumber: string
OrderId: Guid?
OrderItemId: Guid?
SubscriptionId: Guid?
SubscriptionProductId: Guid?
ServiceId: Guid?
CustomerId: Guid
Type: ProvisioningType          (Create, Modify, Suspend, Unsuspend, Terminate)
Status: ProvisioningStatus      (Pending, InProgress, Completed, Failed, RolledBack)
Priority: ProvisioningPriority  (Low, Normal, High, Critical)
Steps: List<ProvisioningStep>
TriggeredBy: string
TriggeredAt: DateTime
CompletedAt: DateTime?
RequestPayload: string?
ResponsePayload: string?
ErrorMessage: string?
RetryCount: int
MaxRetries: int

Methods:
  Start()
  Complete()
  Fail(string errorMessage)
  Retry()
  AddStep(ProvisioningStep step)
  ExecuteNextStep(): ProvisioningStepResult
  CanRollback(): bool
  Rollback()
```

#### Entity: ProvisioningStep

```
Id: Guid
ProvisioningRequestId: Guid
Sequence: int
StepType: StepType              (CreateAAA, ConfigureRadius, AssignIP, CreateVlan, ConfigureCPE, ActivatePort, CreateSIP)
Name: string
TargetSystem: string            (AAA, DNS, DHCP, BRAS, OLT, NMS, EMS)
Parameters: Dictionary<string, object>
Status: StepStatus              (Pending, InProgress, Completed, Failed, Skipped, RolledBack)
StartedAt: DateTime?
CompletedAt: DateTime?
ErrorMessage: string?
RetryCount: int
```

#### Entity: ProvisioningTemplate

```
Id: Guid
TenantId: Guid
Name: string
ServiceType: ServiceType
Steps: List<TemplateStep>
Parameters: Dictionary<string, string>       (parameter definitions)
CreatedAt: DateTime
UpdatedAt: DateTime
```

#### Value Objects

```
ProvisioningType: enum { Create, Modify, Suspend, Unsuspend, Terminate }
ProvisioningStatus: enum { Pending, InProgress, Completed, Failed, RolledBack, WaitingForApproval }
ProvisioningPriority: enum { Low, Normal, High, Critical }
StepType: string (extensible, system-specific)
StepStatus: enum { Pending, InProgress, Completed, Failed, Skipped, RolledBack }
```

#### Domain Events

```
ProvisioningRequestCreatedEvent  { RequestId, RequestNumber, OrderId, ServiceType, Type, OccurredAt }
ProvisioningStartedEvent         { RequestId, OccurredAt }
ProvisioningStepCompletedEvent   { RequestId, StepId, StepType, OccurredAt }
ProvisioningCompletedEvent       { RequestId, ServiceId, OccurredAt }
ProvisioningFailedEvent          { RequestId, ErrorMessage, RetryCount, OccurredAt }
ProvisioningRolledBackEvent      { RequestId, OccurredAt }
```

#### Domain Services

```
IProvisioningEngine:
  ExecuteRequest(ProvisioningRequest request): RequestResult
  ExecuteStep(ProvisioningStep step, ProvisioningRequest request): StepResult
  ValidateRequest(ProvisioningRequest request): ValidationResult
  RollbackRequest(ProvisioningRequest request): RollbackResult

IActivationService:
  ActivateServiceOnNetwork(Service service, Dictionary<string, object> config): ActivationResult
  DeactivateServiceOnNetwork(Service service): DeactivationResult
  UpdateServiceOnNetwork(Service service, Dictionary<string, object> changes): UpdateResult
```

### 13.2 Infrastructure Layer

#### Integration Events

**Publishes:**
```
ProvisioningRequestCreatedEvent  -> Topic: "provisioning.request.created"
ProvisioningCompletedEvent       -> Topic: "provisioning.request.completed"
ProvisioningFailedEvent          -> Topic: "provisioning.request.failed"
ProvisioningRolledBackEvent      -> Topic: "provisioning.request.rolled_back"
```

**Subscribes to:**
```
orders.order.completed              -> Create provisioning request
subscription.activated              -> Create provisioning request
subscription.suspended              -> Create suspend provisioning
subscription.terminated             -> Create deprovisioning request
service_inventory.service.created   -> Provision service
```

### 13.3 Dependencies

| Module | Reason |
|--------|--------|
| IAM | Tenant context |
| Orders | Order reference |
| Subscriptions | Subscription product reference |
| ServiceInventory | Service to provision |
| NetworkInventory | Network resources to allocate |
| Workflow | Complex provisioning workflows |

**Depended on by:** ServiceInventory

---

## 14. Workflow — Workflow Engine

### 14.1 Domain Layer

#### Aggregate: WorkflowDefinition

```
Id: Guid
TenantId: Guid
Name: string
Description: string?
Category: string               (Order, Billing, Provisioning, Collection, Support)
Version: int
IsActive: bool
IsPublished: bool
Steps: List<WorkflowStep>
Transitions: List<WorkflowTransition>
Triggers: List<WorkflowTrigger>
Variables: Dictionary<string, object>?
TimeoutMinutes: int?
MaxRetries: int

Methods:
  Publish()
  CreateNewVersion(): WorkflowDefinition
  Activate()
  Deactivate()
  AddStep(WorkflowStep step)
  CanTransition(string fromStatus, string toStatus): bool
  GetNextSteps(string currentStatus): List<WorkflowStep>
```

#### Entity: WorkflowStep

```
Id: Guid
WorkflowDefinitionId: Guid
Name: string
Type: StepType                 (Approval, Automation, Notification, Condition, Wait, UserTask, SubWorkflow, Webhook)
Configuration: Dictionary<string, object>
AssignedRole: string?
AssignedUser: Guid?
TimeoutMinutes: int?
RetryOnFailure: bool
IsAutoComplete: bool
```

#### Entity: WorkflowTransition

```
Id: Guid
WorkflowDefinitionId: Guid
FromStepId: Guid
ToStepId: Guid
Condition: string?             (expression)
Label: string
```

#### Entity: WorkflowTrigger

```
Id: Guid
WorkflowDefinitionId: Guid
EventType: string              (e.g., "orders.order.placed", "subscription.activated")
Condition: string?             (expression to evaluate)
```

#### Aggregate: WorkflowInstance

```
Id: Guid
TenantId: Guid
WorkflowDefinitionId: Guid
WorkflowDefinitionVersion: int
CorrelationId: string          (e.g., order ID, subscription ID)
CorrelationEntityType: string  (Order, Subscription, Ticket, etc.)
Status: WorkflowInstanceStatus (Running, Completed, Failed, Suspended, Terminated)
CurrentStepId: Guid?
CurrentStepName: string?
Variables: Dictionary<string, object>
History: List<WorkflowExecutionLog>
StartedAt: DateTime
CompletedAt: DateTime?
DueDate: DateTime?

Methods:
  Start()
  AdvanceToNextStep(string stepId)
  Complete()
  Fail(string error)
  Suspend(string reason)
  Resume()
  Terminate(string reason)
  SetVariable(string key, object value)
  GetVariable<T>(string key): T?
```

#### Entity: WorkflowExecutionLog

```
Id: Guid
WorkflowInstanceId: Guid
StepId: Guid?
StepName: string?
EventType: string              (Started, Completed, Failed, Approved, Rejected, Error)
Actor: string?
Data: string?
Timestamp: DateTime
```

#### Value Objects

```
StepType: enum { Approval, Automation, Notification, Condition, Wait, UserTask, SubWorkflow, Webhook, Script }
WorkflowInstanceStatus: enum { Pending, Running, Completed, Failed, Suspended, Terminated, WaitingForInput }
```

#### Domain Events

```
WorkflowStartedEvent           { InstanceId, WorkflowDefinitionId, CorrelationId, CorrelationEntityType, OccurredAt }
WorkflowStepCompletedEvent     { InstanceId, StepId, StepName, OccurredAt }
WorkflowCompletedEvent         { InstanceId, OccurredAt }
WorkflowFailedEvent            { InstanceId, ErrorMessage, StepId, OccurredAt }
WorkflowSuspendedEvent         { InstanceId, Reason, OccurredAt }
WorkflowResumedEvent           { InstanceId, OccurredAt }
WorkflowTaskAssignedEvent      { InstanceId, StepId, AssignedTo, OccurredAt }
WorkflowTaskCompletedEvent     { InstanceId, StepId, Action, OccurredAt }
```

#### Repository Interfaces

```
IWorkflowDefinitionRepository:
  GetByIdAsync(Guid id): WorkflowDefinition? (with steps, transitions, triggers)
  GetByEventTypeAsync(string eventType, Guid tenantId): List<WorkflowDefinition>
  GetActiveByCategoryAsync(string category, Guid tenantId): List<WorkflowDefinition>
  AddAsync(WorkflowDefinition definition)
  UpdateAsync(WorkflowDefinition definition)

IWorkflowInstanceRepository:
  GetByIdAsync(Guid id): WorkflowInstance? (with history, variables)
  GetByCorrelationIdAsync(string correlationId): List<WorkflowInstance>
  GetByStatusAsync(WorkflowInstanceStatus status): List<WorkflowInstance>
  GetDueActionsAsync(): List<WorkflowInstance>
  AddAsync(WorkflowInstance instance)
  UpdateAsync(WorkflowInstance instance)
  GetRunningByDefinitionAsync(Guid definitionId): List<WorkflowInstance>
```

### 14.2 Application Layer

#### Commands

```
CreateWorkflowDefinitionCommand   { TenantId, Name, Description, Category, Steps, Transitions, Triggers, TimeoutMinutes }
PublishWorkflowDefinitionCommand { DefinitionId }
ActivateWorkflowDefinitionCommand { DefinitionId }
DeactivateWorkflowDefinitionCommand { DefinitionId }
StartWorkflowInstanceCommand     { DefinitionId, CorrelationId, CorrelationEntityType, Variables }
AdvanceWorkflowStepCommand       { InstanceId, StepId, Action, Data }
CompleteWorkflowStepCommand      { InstanceId, StepId, Data }
FailWorkflowStepCommand          { InstanceId, StepId, ErrorMessage }
SuspendWorkflowInstanceCommand   { InstanceId, Reason }
ResumeWorkflowInstanceCommand    { InstanceId }
TerminateWorkflowInstanceCommand { InstanceId, Reason }
TriggerWorkflowFromEventCommand  { EventType, CorrelationId, EventData }
EvaluatePendingWorkflowsCommand  { } (background)
```

#### Queries

```
GetWorkflowDefinitionQuery      { DefinitionId }
ListWorkflowDefinitionsQuery    { TenantId, Category, IsActive, Page, Size }
GetWorkflowInstanceQuery        { InstanceId }
ListWorkflowInstancesQuery      { TenantId, DefinitionId, Status, CorrelationEntityType, CorrelationId, Page, Size }
GetWorkflowInstanceHistoryQuery { InstanceId }
GetWorkflowStatsQuery           { TenantId }
GetPendingTasksQuery            { UserId, Role }
GetWorkflowTimelineQuery        { InstanceId }
```

### 14.3 Dependencies

| Module | Reason |
|--------|--------|
| IAM | Tenant context, role/user assignments |
| Notifications | Task assignment notifications |

**Depended on by:** Orders, Billing, Provisioning, Collections, Ticketing (all modules needing workflow automation)

## 15. Ticketing — Ticketing/Helpdesk

### 15.1 Domain Layer

#### Aggregate: Ticket

```
Id: Guid
TenantId: Guid
TicketNumber: string
CustomerId: Guid?
Subject: string
Description: string
Status: TicketStatus            (New, Open, InProgress, PendingCustomer, Resolved, Closed, Reopened)
Priority: TicketPriority        (Low, Medium, High, Critical)
Type: TicketType                (Complaint, Inquiry, Technical, Billing, ServiceRequest, ChangeRequest)
Source: TicketSource            (Portal, Email, Phone, Chat, Social, System)
Category: string?
SubCategory: string?
AssignedAgentId: Guid?
AssignedGroupId: Guid?
SlaPolicyId: Guid?
SlaDeadline: DateTime?
FirstResponseAt: DateTime?
ResolvedAt: DateTime?
ClosedAt: DateTime?
ReopenedCount: int
Comments: List<TicketComment>
Attachments: List<TicketAttachment>
RelatedEntityType: string?      (Order, Subscription, Service, Invoice)
RelatedEntityId: Guid?
Tags: List<string>
SatisfactionRating: int?
SatisfactionComment: string?

Methods:
  Assign(Guid agentId)
  AssignToGroup(Guid groupId)
  ChangeStatus(TicketStatus status)
  UpdatePriority(TicketPriority priority)
  AddComment(TicketComment comment)
  AddAttachment(TicketAttachment attachment)
  Resolve(string resolution)
  Close()
  Reopen()
  CalculateSlaStatus(): SlaStatus
  IsSlaBreached(): bool
```

#### Entity: TicketComment

```
Id: Guid
TicketId: Guid
AuthorId: Guid
AuthorName: string
AuthorType: CommentAuthorType   (Agent, Customer, System)
Content: string
IsInternal: bool                (internal notes not visible to customer)
Attachments: List<TicketAttachment>
CreatedAt: DateTime
```

#### Entity: TicketAttachment

```
Id: Guid
TicketId: Guid
CommentId: Guid?
FileName: string
FileType: string
FileSize: long
StoragePath: string
UploadedBy: Guid
UploadedAt: DateTime
```

#### Entity: SupportGroup

```
Id: Guid
TenantId: Guid
Name: string
Description: string?
Members: List<Guid>             (user IDs)
SlaPolicyId: Guid?
IsActive: bool
```

#### Entity: SlaPolicy

```
Id: Guid
TenantId: Guid
Name: string
Priority: TicketPriority
FirstResponseMinutes: int
ResolutionMinutes: int
EscalationMinutes: int?
BusinessHoursOnly: bool
AutoEscalate: bool
```

#### Value Objects

```
TicketStatus: enum { New, Open, InProgress, PendingCustomer, PendingThirdParty, Resolved, Closed, Reopened }
TicketPriority: enum { Low, Medium, High, Critical }
TicketType: enum { Complaint, Inquiry, Technical, Billing, ServiceRequest, ChangeRequest, Outage }
TicketSource: enum { Portal, Email, Phone, Chat, Social, System, Api }
CommentAuthorType: enum { Agent, Customer, System }
SlaStatus: enum { WithinSla, ApproachingBreach, Breached, NotApplicable }
```

#### Domain Events

```
TicketCreatedEvent           { TicketId, TicketNumber, CustomerId, Subject, Priority, Type, Source, OccurredAt }
TicketAssignedEvent          { TicketId, AssignedAgentId, AssignedGroupId, OccurredAt }
TicketStatusChangedEvent     { TicketId, OldStatus, NewStatus, OccurredAt }
TicketPriorityChangedEvent   { TicketId, OldPriority, NewPriority, OccurredAt }
TicketCommentAddedEvent      { TicketId, CommentId, AuthorType, Content, OccurredAt }
TicketResolvedEvent          { TicketId, Resolution, OccurredAt }
TicketClosedEvent            { TicketId, SatisfactionRating, OccurredAt }
TicketReopenedEvent          { TicketId, Reason, OccurredAt }
TicketSlaBreachedEvent       { TicketId, SlaDeadline, OccurredAt }
TicketEscalatedEvent         { TicketId, EscalationLevel, OccurredAt }
```

### 15.2 Application Layer

#### Commands

```
CreateTicketCommand          { TenantId, CustomerId, Subject, Description, Priority, Type, Source, Category, RelatedEntityType, RelatedEntityId, Tags }
AssignTicketCommand          { TicketId, AgentId }
AssignTicketToGroupCommand   { TicketId, GroupId }
UpdateTicketStatusCommand    { TicketId, Status }
UpdateTicketPriorityCommand  { TicketId, Priority }
AddCommentCommand            { TicketId, Content, IsInternal, AuthorId, AuthorType }
ResolveTicketCommand         { TicketId, Resolution }
CloseTicketCommand           { TicketId }
ReopenTicketCommand          { TicketId, Reason }
AddAttachmentCommand         { TicketId, FileName, FileType, FileSize, FileContent }
RateSatisfactionCommand      { TicketId, Rating, Comment }
CreateSlaPolicyCommand       { TenantId, Name, Priority, FirstResponseMinutes, ResolutionMinutes }
CreateSupportGroupCommand    { TenantId, Name, Description, Members }
BulkAssignTicketsCommand     { TicketIds[], AgentId }
TransferTicketCommand        { TicketId, TargetGroupId, Reason }
```

### 15.3 Dependencies

| Module | Reason |
|--------|--------|
| IAM | Tenant context, agent/user references |
| CRM | Customer reference |
| Notifications | Ticket assignment/update notifications |
| ServiceInventory | Service reference for technical tickets |

**Depended on by:** Notifications, Reporting

## 16. Notifications — Notification Engine

### 16.1 Domain Layer

#### Aggregate: Notification

```
Id: Guid
TenantId: Guid
Type: NotificationType           (Email, SMS, Push, InApp, Webhook, Voice)
Priority: NotificationPriority   (Low, Normal, High, Urgent)
Status: NotificationStatus       (Pending, Queued, Sent, Delivered, Failed, Bounced, Read)
From: string?
To: string[]
Cc: string[]?
Bcc: string[]?
Subject: string
Body: string
BodyHtml: string?
TemplateName: string?
TemplateData: Dictionary<string, object>
Channel: string
CorrelationId: string?           (e.g., invoice ID, ticket ID)
CorrelationEntityType: string?
ScheduledAt: DateTime?
SentAt: DateTime?
DeliveredAt: DateTime?
ReadAt: DateTime?
FailedAt: DateTime?
FailureReason: string?
ProviderMessageId: string?       (e.g., SES message ID, Twilio SID)
Attachments: List<NotificationAttachment>
RetryCount: int
MaxRetries: int
```

#### Entity: NotificationTemplate

```
Id: Guid
TenantId: Guid
Name: string
Description: string?
Type: NotificationType
Subject: string
BodyTemplate: string            (with placeholders like {{variable}})
BodyHtmlTemplate: string?
Variables: List<string>         (expected variables)
IsSystem: bool                  (cannot be deleted)
CreatedAt: DateTime
UpdatedAt: DateTime
```

#### Entity: NotificationPreference

```
Id: Guid
TenantId: Guid
CustomerId: Guid?
UserId: Guid?
Channel: NotificationType       (Email, SMS, Push, InApp)
Category: string                (Billing, Support, Marketing, System, Promotions)
Enabled: bool
Address: string?                (email address, phone number, push token)
Verified: bool
```

#### Entity: NotificationAttachment

```
Id: Guid
NotificationId: Guid
FileName: string
ContentType: string
StoragePath: string
```

#### Value Objects

```
NotificationType: enum { Email, SMS, Push, InApp, Webhook, Voice }
NotificationPriority: enum { Low, Normal, High, Urgent }
NotificationStatus: enum { Pending, Queued, Sent, Delivered, Failed, Bounced, Read, Clicked }
```

#### Domain Events

```
NotificationSentEvent        { NotificationId, Type, To, Subject, CorrelationId, OccurredAt }
NotificationDeliveredEvent   { NotificationId, DeliveredAt, OccurredAt }
NotificationFailedEvent      { NotificationId, FailureReason, OccurredAt }
NotificationBouncedEvent     { NotificationId, BounceType, OccurredAt }
NotificationOpenedEvent      { NotificationId, ReadAt, OccurredAt }
NotificationClickedEvent     { NotificationId, LinkClicked, OccurredAt }
```

### 16.2 Application Layer

#### Commands

```
SendNotificationCommand           { TenantId, Type, To[], Subject, Body, TemplateName, TemplateData, CorrelationId, CorrelationEntityType, Priority, ScheduledAt }
SendTemplateNotificationCommand   { TenantId, Type, To[], TemplateName, TemplateData, CorrelationId, Priority }
SendBulkNotificationCommand       { TenantId, Type, To[], Subject, Body, CorrelationEntityType }
CreateNotificationTemplateCommand { TenantId, Name, Type, Subject, BodyTemplate, Variables }
UpdateNotificationTemplateCommand { TemplateId, Subject, BodyTemplate }
DeleteNotificationTemplateCommand { TemplateId }
UpdatePreferenceCommand           { TenantId, UserId, Channel, Category, Enabled, Address }
VerifyPreferenceCommand           { TenantId, UserId, Channel }
MarkAsReadCommand                 { NotificationId }
RetryFailedNotificationCommand    { NotificationId }
CancelNotificationCommand          { NotificationId }
```

#### Queries

```
GetNotificationQuery           { NotificationId }
ListNotificationsQuery         { TenantId, UserId, CustomerId, Type, Status, CorrelationId, Page, Size }
GetUnreadCountQuery            { UserId }
ListNotificationTemplatesQuery { TenantId, Type }
GetNotificationTemplateQuery   { TemplateId }
GetNotificationPreferencesQuery { UserId }
GetNotificationStatsQuery      { TenantId }
```

### 16.3 Dependencies

| Module | Reason |
|--------|--------|
| IAM | Tenant context, user references |
| CRM | Customer contact info for notifications |

**Depended on by:** All modules (cross-cutting notification concerns)

## 17. Reporting — Reporting & BI

### 17.1 Domain Layer

#### Value Objects

```
ReportType: enum { Tabular, Chart, Pivot, Dashboard, Scheduled, AdHoc }
ReportFormat: enum { Pdf, Excel, Csv, Html, Json, Image }
ChartType: enum { Bar, Line, Pie, Area, Donut, Scatter, Heatmap, Table }
ReportStatus: enum { Draft, Published, Archived }
ScheduleFrequency: enum { OneTime, Hourly, Daily, Weekly, Monthly, Quarterly, Yearly }
DataSource: enum { OperationalDb, DataWarehouse, Elasticsearch, Cube }
```

#### Domain Events

```
ReportGeneratedEvent         { ReportId, ReportName, Format, Size, OccurredAt }
ReportScheduledEvent         { ReportId, Schedule, OccurredAt }
ReportDeliveredEvent         { ReportId, Recipients, OccurredAt }
DashboardCreatedEvent        { DashboardId, Name, OccurredAt }
```

### 17.2 Application Layer

#### Commands

```
GenerateReportCommand        { ReportId, Parameters, Format }
ScheduleReportCommand        { ReportId, Schedule, Recipients[], Format }
CreateDashboardCommand       { Name, Description, Layout }
CreateReportDefinitionCommand { Name, Category, DataSource, Query, Parameters[], ChartType }
```

### 17.3 Dependencies

| Module | Reason |
|--------|--------|
| IAM | Tenant context, user/role permissions |
| All modules | Data sources for reports |

## 18. Audit — Audit Trail

### 18.1 Domain Layer

#### Aggregate: AuditEvent

```
Id: Guid
TenantId: Guid
EventType: string              (e.g., "user.created", "order.placed", "invoice.paid")
Category: AuditCategory        (Security, Data, Configuration, Financial, Access)
CorrelationId: string?
CorrelationEntityType: string? (User, Order, Invoice, Subscription)
Action: string                 (Create, Read, Update, Delete, Login, Export)
ActorId: Guid?
ActorType: string              (User, System, ApiKey)
ActorName: string?
TargetId: string?
TargetType: string?
TargetDisplayName: string?
OriginalValues: string?        (JSON before change)
NewValues: string?             (JSON after change)
Changes: List<AuditChange>
IpAddress: string?
UserAgent: string?
SessionId: string?
RequestId: string?
Timestamp: DateTime
Metadata: Dictionary<string, string>
IsImmutable: true              (enforced at DB level)
RetentionDays: int
```

#### Entity: AuditChange

```
Id: Guid
AuditEventId: Guid
PropertyName: string
OriginalValue: string?
NewValue: string?
ChangeType: string             (Modified, Added, Removed)
```

#### Value Objects

```
AuditCategory: enum { Security, Data, Configuration, Financial, Access, System, Compliance }
```

#### Repository Interfaces

```
IAuditEventRepository:
  AddAsync(AuditEvent auditEvent)
  GetPagedAsync(Guid tenantId, AuditFilter filter): PagedResult<AuditEvent>
  GetByCorrelationIdAsync(string correlationId): List<AuditEvent>
  GetByDateRangeAsync(Guid tenantId, DateTime from, DateTime to): List<AuditEvent>
  GetByEventTypeAsync(string eventType, Guid tenantId): List<AuditEvent>
  GetByActorAsync(Guid actorId): List<AuditEvent>
  DeleteOlderThanAsync(DateTime cutoff)
```

### 18.2 Infrastructure Layer

#### Audit Interceptor

```
EF Core SaveChangesInterceptor:
  - Intercepts all SaveChanges calls
  - Detects entity changes (Added, Modified, Deleted)
  - Captures OriginalValues and NewValues
  - Creates AuditEvent for each change
  - Writes to audit_events table (append-only)
  - Runs in background to avoid transaction impact
```

#### Integration Events

**Publishes:**
```
AuditEventCreatedEvent       -> Topic: "audit.event.created"

Subscribes to: All domain events from all modules for centralized audit capture
```

### 18.3 Dependencies

| Module | Reason |
|--------|--------|
| IAM | Tenant context, actor identification |

**Depended on by:** All modules (cross-cutting)

## 19. APIGateway — API Gateway

### 19.1 Domain Layer

The API Gateway is the single entry point for all external and internal API traffic. It handles cross-cutting concerns centrally.

#### Configuration Entities

```
RouteConfig:
  Id: Guid
  Path: string                (e.g., "/api/v1/orders/**")
  Module: string
  Methods: List<string>
  UpstreamUrl: string?
  RequiresAuthentication: bool
  RequiredRoles: List<string>
  RequiredPermissions: List<string>
  RateLimitPolicy: RateLimitPolicy?
  CorsPolicy: string?
  TimeoutMs: int
  IsEnabled: bool

RateLimitPolicy:
  Name: string
  PermitLimit: int
  WindowMinutes: int
  QueueLimit: int
  BurstLimit: int?

CorsPolicy:
  Name: string
  AllowedOrigins: List<string>
  AllowedMethods: List<string>
  AllowedHeaders: List<string>
  AllowCredentials: bool

ApiKey:
  Id: Guid
  TenantId: Guid
  Name: string
  KeyHash: string
  KeyPrefix: string
  Scopes: List<string>
  IsActive: bool
  ExpiresAt: DateTime?
  AllowedIps: List<string>?
  CreatedAt: DateTime
  LastUsedAt: DateTime?
```

### 19.2 Cross-Cutting Concerns

#### Authentication & Authorization

```
- Validates JWT tokens from IAM module
- Validates API keys for machine-to-machine communication
- Extracts tenant context from JWT claims
- Enforces RBAC permissions per route
- Returns 401/403 for unauthorized requests
```

#### Rate Limiting

```
- Token bucket algorithm per client/IP/API key
- Configurable limits per endpoint group
- Returns 429 with Retry-After header
- Redis-backed distributed rate limiting
```

#### Request/Response Transformation

```
- Request validation (JSON schema, content type)
- Response formatting (standard envelope)
- Error normalization
- Header propagation for correlationId, tenantId
- CORS enforcement
```

#### Logging & Monitoring

```
- Request/response logging (excluding sensitive data)
- Latency tracking
- Error tracking and alerting
- Request ID propagation
- Structured logging
```

### 19.3 API Layer

#### Endpoints

```
GET    /api/v1/gateway/routes                -> ListRoutes               [Admin]
POST   /api/v1/gateway/routes                -> CreateRoute              [Admin]
PUT    /api/v1/gateway/routes/{id}           -> UpdateRoute              [Admin]
DELETE /api/v1/gateway/routes/{id}           -> DeleteRoute              [Admin]

GET    /api/v1/gateway/rate-limit-policies   -> ListRateLimitPolicies    [Admin]
POST   /api/v1/gateway/rate-limit-policies   -> CreateRateLimitPolicy    [Admin]
PUT    /api/v1/gateway/rate-limit-policies/{id} -> UpdateRateLimitPolicy  [Admin]

GET    /api/v1/gateway/cors-policies         -> ListCorsPolicies         [Admin]
POST   /api/v1/gateway/cors-policies         -> CreateCorsPolicy         [Admin]

POST   /api/v1/gateway/api-keys              -> CreateApiKey             [Admin]
GET    /api/v1/gateway/api-keys              -> ListApiKeys              [Admin]
DELETE /api/v1/gateway/api-keys/{id}         -> RevokeApiKey             [Admin]

GET    /api/v1/gateway/health                -> HealthCheck              [Public]
GET    /api/v1/gateway/health/ready          -> ReadinessCheck           [Public]
GET    /api/v1/gateway/health/live           -> LivenessCheck            [Public]
GET    /api/v1/gateway/metrics               -> PrometheusMetrics        [Admin]
```

### 19.4 API Versioning Strategy

```
URL-based versioning: /api/v1/, /api/v2/
Version header: X-API-Version
Breaking changes -> new major version
Non-breaking changes (additive) -> backwards compatible
Deprecation policy: minimum 6 months with Deprecation header
```

### 19.5 Standard API Response Envelope

```
{
  "data": { ... },           // Actual response payload
  "meta": {
    "requestId": "uuid",
    "timestamp": "ISO8601",
    "version": "1.0",
    "page": 1,               // for paginated responses
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  },
  "errors": [                 // present only on error
    {
      "code": "VALIDATION_ERROR",
      "message": "Human-readable message",
      "details": { ... }
    }
  ]
}
```

### 19.6 Dependencies

| Module | Reason |
|--------|--------|
| IAM | Token validation, user/role/permission resolution |
| Rate Limiting | Redis for distributed rate limiting |

**Depended on by:** All external clients

### 19.7 Security Requirements

```
- TLS 1.3 required for all connections
- JWT token validation on every request
- API key validation for m2m communication
- SQL injection prevention (input sanitization)
- XSS protection headers
- CORS policies per origin
- Rate limiting per client
- Request size limits
- Sensitive data masking in logs
- IP allow/deny lists for admin endpoints
```

---
