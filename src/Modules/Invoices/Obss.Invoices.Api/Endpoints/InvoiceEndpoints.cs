using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.Commands.CancelInvoice;
using Obss.Invoices.Application.Commands.CreateInvoiceFromBill;
using Obss.Invoices.Application.Commands.FinalizeInvoice;
using Obss.Invoices.Application.Commands.IssueCreditNote;
using Obss.Invoices.Application.Commands.MarkInvoiceAsSent;
using Obss.Invoices.Application.Commands.OpenDispute;
using Obss.Invoices.Application.Commands.RecordInvoicePayment;
using Obss.Invoices.Application.Commands.RejectDispute;
using Obss.Invoices.Application.Commands.ResolveDispute;
using Obss.Invoices.Application.Queries.GetDisputeById;
using Obss.Invoices.Application.Queries.GetInvoiceById;
using Obss.Invoices.Application.Queries.GetInvoiceDisputes;
using Obss.Invoices.Application.Queries.GetInvoiceSummary;
using Obss.Invoices.Application.Queries.GetInvoiceView;
using Obss.Invoices.Application.Queries.GetAllInvoices;
using Obss.Invoices.Application.Queries.GetInvoicesByCustomer;
using Obss.Invoices.Application.Queries.GetOverdueInvoices;
using Obss.Invoices.Application.Services;
using Obss.Invoices.Infrastructure.Persistence;

namespace Obss.Invoices.Api.Endpoints;

public static class InvoiceEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/invoices", async (CreateInvoiceFromBillCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v{{version}}/invoices/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/invoices/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetInvoiceByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/invoices", async (
            Guid? customerId,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            IMediator mediator) =>
        {
            if (customerId.HasValue)
            {
                var result = await mediator.Send(new GetInvoicesByCustomerQuery(customerId.Value, status, fromDate, toDate));
                return result.IsSuccess
                    ? (IResult)TypedResults.Ok(result.Value)
                    : (IResult)TypedResults.BadRequest(result.Error);
            }

            var allResult = await mediator.Send(new GetAllInvoicesQuery());
            return allResult.IsSuccess
                ? (IResult)TypedResults.Ok(allResult.Value)
                : (IResult)TypedResults.BadRequest(allResult.Error);
        });

        group.MapPost("/invoices/{id:guid}/finalize", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new FinalizeInvoiceCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/invoices/{id:guid}/send", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new MarkInvoiceAsSentCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/invoices/{id:guid}/pay", async (Guid id, RecordInvoicePaymentCommand command, IMediator mediator) =>
        {
            if (id != command.InvoiceId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/invoices/{id:guid}/cancel", async (Guid id, CancelInvoiceCommand command, IMediator mediator) =>
        {
            if (id != command.InvoiceId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/invoices/{id:guid}/credit-note", async (Guid id, IssueCreditNoteCommand command, IMediator mediator) =>
        {
            if (id != command.InvoiceId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v{{version}}/invoices/{id}/credit-notes/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/invoices/overdue", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOverdueInvoicesQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/invoices/summary", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetInvoiceSummaryQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/invoices/{id:guid}/view", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetInvoiceViewQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/invoices/{id:guid}/pdf", async (Guid id, IInvoicePresenter presenter, IInvoiceRepository repository) =>
        {
            var invoice = await repository.GetByIdWithDetailsAsync(id);
            if (invoice is null)
                return (IResult)TypedResults.NotFound();

            var pdf = await presenter.GeneratePdfAsync(invoice);
            return (IResult)TypedResults.File(pdf, "application/pdf", $"invoice-{id}.pdf");
        });

        group.MapGet("/invoices/{id:guid}/html", async (Guid id, IInvoicePresenter presenter, IInvoiceRepository repository) =>
        {
            var invoice = await repository.GetByIdWithDetailsAsync(id);
            if (invoice is null)
                return (IResult)TypedResults.NotFound();

            var html = await presenter.GenerateHtmlAsync(invoice);
            return (IResult)TypedResults.Content(html, "text/html");
        });

        group.MapPost("/invoices/{id:guid}/disputes", async (Guid id, OpenDisputeCommand command, IMediator mediator) =>
        {
            if (id != command.InvoiceId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v{{version}}/invoices/disputes/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/invoices/{id:guid}/disputes", async (Guid id, string? status, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetInvoiceDisputesQuery(id, status));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/invoices/disputes", async (string? status, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetInvoiceDisputesQuery(null, status));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/invoices/{id:guid}/credit-notes", async (Guid id, InvoiceDbContext dbContext) =>
        {
            var creditNotes = await dbContext.CreditNotes
                .Where(c => c.InvoiceId == id)
                .OrderByDescending(c => c.IssuedAt)
                .ToListAsync();
            return (IResult)TypedResults.Ok(creditNotes);
        });

        group.MapGet("/invoices/credit-notes", async (Guid? customerId, string? status, InvoiceDbContext dbContext) =>
        {
            var query = dbContext.CreditNotes.AsQueryable();
            if (customerId.HasValue)
                query = query.Where(c => c.CustomerId == customerId.Value);
            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status.ToString() == status);
            var creditNotes = await query.OrderByDescending(c => c.IssuedAt).ToListAsync();
            return (IResult)TypedResults.Ok(creditNotes);
        });

        group.MapGet("/invoices/disputes/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetDisputeByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/invoices/disputes/{id:guid}/resolve", async (Guid id, ResolveDisputeCommand command, IMediator mediator) =>
        {
            if (id != command.DisputeId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/invoices/disputes/{id:guid}/reject", async (Guid id, RejectDisputeCommand command, IMediator mediator) =>
        {
            if (id != command.DisputeId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
