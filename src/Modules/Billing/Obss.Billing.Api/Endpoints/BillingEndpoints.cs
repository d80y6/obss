using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Obss.Billing.Application.Commands.AddAdjustment;
using Obss.Billing.Application.Commands.CreateBillingAccount;
using Obss.Billing.Application.Commands.UpdateBillingAccount;
using Obss.Billing.Application.Commands.PatchBillingAccount;
using Obss.Billing.Application.Commands.DeleteBillingAccount;
using Obss.Billing.Application.Commands.AddBillingAccountRelatedParty;
using Obss.Billing.Application.Commands.RemoveBillingAccountRelatedParty;
using Obss.Billing.Application.Commands.CreateBillPresentationMedia;
using Obss.Billing.Application.Commands.UpdateBillPresentationMedia;
using Obss.Billing.Application.Commands.RemoveBillPresentationMedia;
using Obss.Billing.Application.Commands.RecordBalanceTransaction;
using Obss.Billing.Application.Queries.GetBillingAccountById;
using Obss.Billing.Application.Queries.SearchBillingAccounts;
using Obss.Billing.Application.Queries.GetBillingAccountBalance;
using Obss.Billing.Application.Queries.GetBillingAccountRelatedParties;
using Obss.Billing.Application.Queries.GetBillingAccountBillPresentationMedia;
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
using Obss.SharedKernel.Application.Authorization;

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
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.BillWrite));

        group.MapGet("/bills/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBillByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.BillRead));

        group.MapGet("/bills", async ([AsParameters] GetBillsByCustomerQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.BillRead));

        group.MapGet("/bills/open", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOpenBillsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.BillRead));

        group.MapPost("/bills/{id:guid}/finalize", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new FinalizeBillCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.BillFinalize));

        group.MapPost("/bills/{id:guid}/adjustments", async (Guid id, AddAdjustmentCommand command, IMediator mediator) =>
        {
            if (id != command.BillId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.BillAdjust));

        group.MapPost("/cycles", async (GenerateBillingCycleCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/billing/cycles/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.CycleManage));

        group.MapPost("/tax-rules", async (CreateTaxRuleCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/billing/tax-rules/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.TaxManage));

        group.MapGet("/tax-rules", async ([AsParameters] GetTaxRulesQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.TaxManage));

        group.MapPost("/tax-exemptions", async (ApplyTaxExemptionCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/billing/tax-exemptions/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.TaxManage));

        group.MapGet("/tax-exemptions/customer/{customerId:guid}", async (Guid customerId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetCustomerTaxExemptionsQuery(customerId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.TaxManage));

        group.MapPost("/bills/{id:guid}/calculate-taxes", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CalculateBillTaxesCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.TaxManage));

        group.MapGet("/jobs", async (BillingDbContext dbContext) =>
        {
            var jobs = await dbContext.BillingJobs.OrderByDescending(j => j.CreatedAt).ToListAsync();
            return (IResult)TypedResults.Ok(jobs);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.BillRead));

        group.MapGet("/jobs/{id:guid}", async (Guid id, BillingDbContext dbContext) =>
        {
            var job = await dbContext.BillingJobs.FindAsync(id);
            return job is not null ? (IResult)TypedResults.Ok(job) : (IResult)TypedResults.NotFound();
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.BillRead));

        group.MapPost("/jobs", async (string jobType, BillingDbContext dbContext, IUnitOfWork unitOfWork) =>
        {
            var job = new BillingJob(Guid.NewGuid(), jobType);
            dbContext.BillingJobs.Add(job);
            await unitOfWork.SaveChangesAsync();
            return (IResult)TypedResults.Created($"/api/v1/billing/jobs/{job.Id}", job);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.BillWrite));

        group.MapPost("/billing-accounts", async (CreateBillingAccountCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/billing/billing-accounts/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountWrite));

        group.MapGet("/billing-accounts/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBillingAccountByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountRead));

        group.MapPut("/billing-accounts/{id:guid}", async (Guid id, UpdateBillingAccountCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountWrite));

        group.MapGet("/billing-accounts", async ([AsParameters] SearchBillingAccountsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountRead));

        // TMF666 BillingAccount enhancements
        group.MapPatch("/billing-accounts/{id:guid}", async (Guid id, PatchBillingAccountCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountWrite));

        group.MapDelete("/billing-accounts/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteBillingAccountCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountWrite));

        group.MapGet("/billing-accounts/{id:guid}/balance", async (Guid id, DateTime? asOfDate, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBillingAccountBalanceQuery(id, asOfDate));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountRead));

        group.MapPost("/billing-accounts/{id:guid}/adjustments", async (Guid id, RecordBalanceTransactionCommand command, IMediator mediator) =>
        {
            if (id != command.BillingAccountId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountWrite));

        group.MapGet("/billing-accounts/{id:guid}/related-parties", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBillingAccountRelatedPartiesQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountRead));

        group.MapPost("/billing-accounts/{id:guid}/related-parties", async (Guid id, AddBillingAccountRelatedPartyCommand command, IMediator mediator) =>
        {
            if (id != command.BillingAccountId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/billing/billing-accounts/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountWrite));

        group.MapDelete("/billing-accounts/{id:guid}/related-parties/{partyId}", async (Guid id, string partyId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveBillingAccountRelatedPartyCommand(id, partyId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountWrite));

        group.MapGet("/billing-accounts/{id:guid}/presentation-media", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetBillingAccountBillPresentationMediaQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountRead));

        group.MapPost("/billing-accounts/{id:guid}/presentation-media", async (Guid id, CreateBillPresentationMediaCommand command, IMediator mediator) =>
        {
            if (id != command.BillingAccountId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/billing/billing-accounts/{id}/presentation-media/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountWrite));

        group.MapPut("/billing-accounts/{id:guid}/presentation-media/{mediaId:guid}", async (Guid id, Guid mediaId, UpdateBillPresentationMediaCommand command, IMediator mediator) =>
        {
            if (id != command.BillingAccountId || mediaId != command.MediaId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountWrite));

        group.MapDelete("/billing-accounts/{id:guid}/presentation-media/{mediaId:guid}", async (Guid id, Guid mediaId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveBillPresentationMediaCommand(id, mediaId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Billing.AccountWrite));
    }
}
