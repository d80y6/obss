# Service Inventory - Topology & Discovery Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development or superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add F-066 Resource Topology and F-068 Resource Discovery to the ServiceInventory module.

**Architecture:** Follows existing CQRS + Domain-Driven patterns: Domain entities/value objects → Application commands/queries/handlers/DTOs/Mapster mappings → Infrastructure EF Core configs/repositories → API endpoints/module registration.

**Tech Stack:** .NET 9, MediatR, Mapster, EF Core, Npgsql

---

### Task 1: Domain Value Objects (Enums)

**Files:**
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Domain/ValueObjects/TopologyType.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Domain/ValueObjects/LinkType.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Domain/ValueObjects/Direction.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Domain/ValueObjects/DiscoveryType.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Domain/ValueObjects/DiscoveryStatus.cs`

TopologyType: Tree, Mesh, Linear, Ring
LinkType: DependsOn, ConnectedTo, Contains, BundledWith
Direction: Unidirectional, Bidirectional
DiscoveryType: NetworkScan, IPRange, API, SNMP, Manual
DiscoveryStatus: Pending, Running, Completed, Failed

### Task 2: Domain Entities (TopologyLink, ServiceTopology, ResourceDiscoveryJob)

**Files:**
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Domain/Entities/TopologyLink.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Domain/Entities/ServiceTopology.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Domain/Entities/ResourceDiscoveryJob.cs`

TopologyLink: Entity<Guid>, properties ServiceTopologyId, SourceServiceId, TargetServiceId, LinkType, Direction, Attributes
ServiceTopology: AggregateRoot<Guid>, properties ServiceId, TopologyType, CreatedAt, UpdatedAt, collection of TopologyLinks
ResourceDiscoveryJob: AggregateRoot<Guid>, properties TenantId, DiscoveryType, Configuration, Status, StartedAt, CompletedAt, ResourcesFound, ResourcesMatched, ErrorMessage, CreatedBy, CreatedAt

### Task 3: Application DTOs

**Files:**
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/DTOs/ServiceTopologyDto.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/DTOs/TopologyLinkDto.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/DTOs/DiscoveryJobDto.cs`

Follow existing record pattern with string enums.

### Task 4: Application Abstractions (Repositories)

**Files:**
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Abstractions/IServiceTopologyRepository.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Abstractions/IResourceDiscoveryJobRepository.cs`

Follow existing IServiceRepository pattern.

### Task 5: Topology Commands

**Files:**
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Commands/CreateTopology/CreateTopologyCommand.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Commands/CreateTopology/CreateTopologyCommandHandler.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Commands/AddTopologyLink/AddTopologyLinkCommand.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Commands/AddTopologyLink/AddTopologyLinkCommandHandler.cs`

### Task 6: Topology Queries

**Files:**
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Queries/GetServiceTopology/GetServiceTopologyQuery.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Queries/GetServiceTopology/GetServiceTopologyQueryHandler.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Queries/GetUpstreamServices/GetUpstreamServicesQuery.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Queries/GetUpstreamServices/GetUpstreamServicesQueryHandler.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Queries/GetDownstreamServices/GetDownstreamServicesQuery.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Queries/GetDownstreamServices/GetDownstreamServicesQueryHandler.cs`

### Task 7: Discovery Commands

**Files:**
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Commands/StartDiscoveryJob/StartDiscoveryJobCommand.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Commands/StartDiscoveryJob/StartDiscoveryJobCommandHandler.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Commands/CompleteDiscoveryJob/CompleteDiscoveryJobCommand.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Commands/CompleteDiscoveryJob/CompleteDiscoveryJobCommandHandler.cs`

### Task 8: Discovery Queries

**Files:**
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Queries/GetDiscoveryJobs/GetDiscoveryJobsQuery.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Queries/GetDiscoveryJobs/GetDiscoveryJobsQueryHandler.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Queries/GetUnmatchedResources/GetUnmatchedResourcesQuery.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Queries/GetUnmatchedResources/GetUnmatchedResourcesQueryHandler.cs`

### Task 9: Update Mapping Config

**Files:**
- Modify: `src/Modules/ServiceInventory/Obss.ServiceInventory.Application/Mappings/ServiceMappingConfig.cs`

Add Mapster config for ServiceTopology→ServiceTopologyDto, TopologyLink→TopologyLinkDto, ResourceDiscoveryJob→DiscoveryJobDto.

### Task 10: Infrastructure EF Configurations

**Files:**
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Infrastructure/Persistence/Configurations/ServiceTopologyConfiguration.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Infrastructure/Persistence/Configurations/TopologyLinkConfiguration.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Infrastructure/Persistence/Configurations/ResourceDiscoveryJobConfiguration.cs`

Follow existing snake_case + jsonb pattern.

### Task 11: Infrastructure Repositories

**Files:**
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Infrastructure/Persistence/Repositories/ServiceTopologyRepository.cs`
- Create: `src/Modules/ServiceInventory/Obss.ServiceInventory.Infrastructure/Persistence/Repositories/ResourceDiscoveryJobRepository.cs`

### Task 12: Update DbContext

**Files:**
- Modify: `src/Modules/ServiceInventory/Obss.ServiceInventory.Infrastructure/Persistence/ServiceDbContext.cs`

Add DbSet properties for ServiceTopologies, TopologyLinks, ResourceDiscoveryJobs.

### Task 13: API Endpoints

**Files:**
- Modify: `src/Modules/ServiceInventory/Obss.ServiceInventory.Api/Endpoints/ServiceEndpoints.cs`

Add: POST /services/{id}/topology, GET /services/{id}/topology, GET /services/{id}/topology/upstream, GET /services/{id}/topology/downstream, POST /services/discovery, GET /services/discovery/jobs

### Task 14: Update Module Registration

**Files:**
- Modify: `src/Modules/ServiceInventory/Obss.ServiceInventory.Api/Extensions/ServiceModuleRegistration.cs`

Register repositories and DbSets.
