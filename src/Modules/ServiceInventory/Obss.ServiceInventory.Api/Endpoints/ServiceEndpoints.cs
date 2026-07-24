using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ServiceInventory.Application.Abstractions;
using Obss.ServiceInventory.Domain.ValueObjects;
using Obss.ServiceInventory.Application.Commands.ActivateService;
using Obss.ServiceInventory.Application.Commands.AddTopologyLink;
using Obss.ServiceInventory.Application.Commands.AllocateResource;
using Obss.ServiceInventory.Application.Commands.CompleteDiscoveryJob;
using Obss.ServiceInventory.Application.Commands.CreateService;
using Obss.ServiceInventory.Application.Commands.CreateTopology;
using Obss.ServiceInventory.Application.Commands.DecommissionService;
using Obss.ServiceInventory.Application.Commands.ReleaseResource;
using Obss.ServiceInventory.Application.Commands.RemoveTopologyLink;
using Obss.ServiceInventory.Application.Commands.ResumeService;
using Obss.ServiceInventory.Application.Commands.StartDiscoveryJob;
using Obss.ServiceInventory.Application.Commands.UpdateService;
using Obss.ServiceInventory.Application.Commands.SuspendService;
using Obss.ServiceInventory.Application.Queries.GetDiscoveryJobs;
using Obss.ServiceInventory.Application.Queries.GetDownstreamServices;
using Obss.ServiceInventory.Application.Queries.GetServiceById;
using Obss.ServiceInventory.Application.Queries.GetServiceResources;
using Obss.ServiceInventory.Application.Queries.GetServiceTopology;
using Obss.ServiceInventory.Application.Queries.GetServices;
using Obss.ServiceInventory.Application.Queries.GetServicesBySubscription;
using Obss.ServiceInventory.Application.Queries.GetUnmatchedResources;
using Obss.ServiceInventory.Application.Queries.GetUpstreamServices;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.ServiceInventory.Api.Endpoints;

public static class ServiceEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/services", async (CreateServiceCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/service-inventory/services/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));

        group.MapGet("/services/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetServiceByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapGet("/services", async (HttpContext httpContext, [AsParameters] GetServicesQuery query, IMediator mediator, IServiceRepository serviceRepository) =>
        {
            var result = await mediator.Send(query);
            if (!result.IsSuccess)
                return (IResult)TypedResults.BadRequest(result.Error);

            ServiceType? serviceType = null;
            if (!string.IsNullOrWhiteSpace(query.ServiceType) && Enum.TryParse<ServiceType>(query.ServiceType, out var parsedType))
                serviceType = parsedType;

            ServiceStatus? status = null;
            if (!string.IsNullOrWhiteSpace(query.Status) && Enum.TryParse<ServiceStatus>(query.Status, out var parsedStatus))
                status = parsedStatus;

            var totalCount = await serviceRepository.CountFilteredAsync(query.CustomerId, serviceType, status);
            httpContext.Response.Headers["x-total-count"] = totalCount.ToString();
            return (IResult)TypedResults.Ok(result.Value);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapGet("/subscriptions/{subscriptionId:guid}/services", async (Guid subscriptionId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetServicesBySubscriptionQuery(subscriptionId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapPost("/services/{id:guid}/activate", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ActivateServiceCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceActivate));

        group.MapPost("/services/{id:guid}/suspend", async (Guid id, SuspendServiceCommand command, IMediator mediator) =>
        {
            if (id != command.ServiceId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceSuspend));

        group.MapPost("/services/{id:guid}/decommission", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DecommissionServiceCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceTerminate));

        group.MapPost("/services/{id:guid}/resume", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ResumeServiceCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceResume));

        group.MapPatch("/services/{id:guid}", async (Guid id, UpdateServiceCommand command, IMediator mediator) =>
        {
            if (id != command.ServiceId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceChange));

        group.MapDelete("/services/topology/{topologyId:guid}/links/{linkId:guid}", async (Guid topologyId, Guid linkId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveTopologyLinkCommand(topologyId, linkId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));

        group.MapPost("/services/{id:guid}/topology", async (Guid id, CreateTopologyCommand command, IMediator mediator) =>
        {
            if (id != command.ServiceId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/service-inventory/services/{id}/topology", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));

        group.MapGet("/services/{id:guid}/topology", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetServiceTopologyQuery(id));
            if (result.IsSuccess)
                return (IResult)TypedResults.Ok(result.Value);
            return (IResult)TypedResults.Ok<object?>(null);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapGet("/services/{id:guid}/topology/upstream", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUpstreamServicesQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapGet("/services/{id:guid}/topology/downstream", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetDownstreamServicesQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapGet("/services/{id:guid}/resources", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetServiceResourcesQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapPost("/services/{id:guid}/resources", async (Guid id, AllocateResourceCommand command, IMediator mediator) =>
        {
            if (id != command.ServiceId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));

        group.MapDelete("/services/{serviceId:guid}/resources/{resourceId:guid}", async (Guid serviceId, Guid resourceId, IMediator mediator) =>
        {
            var result = await mediator.Send(new ReleaseResourceCommand(serviceId, resourceId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));

        group.MapPost("/services/topology/{topologyId:guid}/links", async (Guid topologyId, AddTopologyLinkCommand command, IMediator mediator) =>
        {
            if (topologyId != command.ServiceTopologyId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/service-inventory/services/topology/{topologyId}/links", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));

        group.MapPost("/services/discovery", async (StartDiscoveryJobCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/service-inventory/services/discovery/jobs/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));

        group.MapGet("/services/discovery/jobs", async ([AsParameters] GetDiscoveryJobsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapGet("/services/discovery/unmatched", async ([AsParameters] GetUnmatchedResourcesQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceRead));

        group.MapPut("/services/discovery/jobs/{jobId:guid}/complete", async (Guid jobId, CompleteDiscoveryJobCommand command, IMediator mediator) =>
        {
            if (jobId != command.JobId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Telecom.ServiceWrite));
    }
}
