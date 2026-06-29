using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Obss.Billing.Application.Commands.AddAdjustment;
using Obss.Billing.Application.Commands.ApplyTaxExemption;
using Obss.Billing.Application.Commands.CalculateBillTaxes;
using Obss.Billing.Application.Commands.CreateTaxRule;
using Obss.Billing.Application.Commands.FinalizeBill;
using Obss.Billing.Application.Commands.GenerateBill;
using Obss.Billing.Application.Commands.GenerateBillingCycle;
using Obss.Billing.Application.Queries.GetBillById;
using Obss.Billing.Application.Queries.GetBillsByCustomer;
using Obss.Billing.Application.Queries.GetCustomerTaxExemptions;
using Obss.Billing.Application.Queries.GetOpenBills;
using Obss.Billing.Application.Queries.GetTaxRules;
using Obss.Billing.Domain.Entities;
using Obss.Billing.Infrastructure.Persistence;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Billing.Api.Endpoints;

public static class BillingEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/bills/generate", async (GenerateBillCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/billing/bills/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/bills/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBillByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/bills", async ([AsParameters] GetBillsByCustomerQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/bills/open", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOpenBillsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/bills/{id:guid}/finalize", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new FinalizeBillCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/bills/{id:guid}/adjustments", async (Guid id, AddAdjustmentCommand command, IMediator mediator) =>
        {
            if (id != command.BillId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/cycles", async (GenerateBillingCycleCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/billing/cycles/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/tax-rules", async (CreateTaxRuleCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/billing/tax-rules/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/tax-rules", async ([AsParameters] GetTaxRulesQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/tax-exemptions", async (ApplyTaxExemptionCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/billing/tax-exemptions/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/tax-exemptions/customer/{customerId:guid}", async (Guid customerId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCustomerTaxExemptionsQuery(customerId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/bills/{id:guid}/calculate-taxes", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CalculateBillTaxesCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/jobs", async (BillingDbContext dbContext) =>
        {
            var jobs = await dbContext.BillingJobs.OrderByDescending(j => j.CreatedAt).ToListAsync();
            return (IResult)TypedResults.Ok(jobs);
        });

        group.MapGet("/jobs/{id:guid}", async (Guid id, BillingDbContext dbContext) =>
        {
            var job = await dbContext.BillingJobs.FindAsync(id);
            return job is not null ? (IResult)TypedResults.Ok(job) : (IResult)TypedResults.NotFound();
        });

        group.MapPost("/jobs", async (string jobType, BillingDbContext dbContext, IUnitOfWork unitOfWork) =>
        {
            var job = new BillingJob(Guid.NewGuid(), jobType);
            dbContext.BillingJobs.Add(job);
            await unitOfWork.SaveChangesAsync();
            return (IResult)TypedResults.Created($"/api/v1/billing/jobs/{job.Id}", job);
        });
    }
}
