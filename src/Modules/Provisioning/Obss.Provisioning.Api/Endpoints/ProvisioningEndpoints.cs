using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Obss.Provisioning.Application.Abstractions;
using Obss.Provisioning.Application.Commands.CreateProvisioningJob;
using Obss.Provisioning.Application.Commands.CreateProvisioningTemplate;
using Obss.Provisioning.Application.Commands.FailProvisioningJob;
using Obss.Provisioning.Application.Commands.RetryProvisioningJob;
using Obss.Provisioning.Application.Commands.StartProvisioningJob;
using Obss.Provisioning.Application.DTOs;
using Obss.Provisioning.Application.Queries.GetProvisioningJobById;
using Obss.Provisioning.Application.Queries.GetProvisioningJobs;
using Obss.Provisioning.Application.Queries.GetProvisioningTemplates;
using Obss.Provisioning.Infrastructure.Persistence;

namespace Obss.Provisioning.Api.Endpoints;

public static class ProvisioningEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/jobs", async (CreateProvisioningJobCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/provisioning/jobs/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/jobs/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProvisioningJobByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/jobs", async ([AsParameters] GetProvisioningJobsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/jobs/{id:guid}/start", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new StartProvisioningJobCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/jobs/{id:guid}/fail", async (Guid id, FailProvisioningJobCommand command, IMediator mediator) =>
        {
            if (id != command.JobId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/jobs/{id:guid}/retry", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new RetryProvisioningJobCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/jobs/{id:guid}/logs", async (Guid id, ProvisioningDbContext dbContext) =>
        {
            var tasks = await dbContext.ProvisioningTasks
                .Where(t => t.ProvisioningJobId == id)
                .OrderBy(t => t.StepNumber)
                .Select(t => new
                {
                    t.Id,
                    StepNumber = t.StepNumber,
                    TaskType = t.TaskType.ToString(),
                    t.Status,
                    t.ErrorMessage,
                    t.Result,
                    t.StartedAt,
                    t.CompletedAt,
                    t.RetryCount
                })
                .ToListAsync();
            return (IResult)TypedResults.Ok(tasks);
        });

        group.MapGet("/templates", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProvisioningTemplatesQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/templates", async (CreateProvisioningTemplateCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/provisioning/templates/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/templates/{id:guid}", async (Guid id, IProvisioningTemplateRepository repository) =>
        {
            var template = await repository.GetByIdAsync(id);
            return template is not null
                ? (IResult)TypedResults.Ok(template.Adapt<ProvisioningTemplateDto>())
                : (IResult)TypedResults.NotFound();
        });
    }
}
