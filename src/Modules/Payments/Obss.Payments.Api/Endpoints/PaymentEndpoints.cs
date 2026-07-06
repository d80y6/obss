using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Payments.Application.Commands.AllocatePayment;
using Obss.Payments.Application.Commands.AutoReconcile;
using Obss.Payments.Application.Commands.CompletePayment;
using Obss.Payments.Application.Commands.ImportBankStatement;
using Obss.Payments.Application.Commands.ProcessGatewayPayment;
using Obss.Payments.Application.Commands.ReconcilePayment;
using Obss.Payments.Application.Commands.RecordPayment;
using Obss.Payments.Application.Commands.RefundPayment;
using Obss.Payments.Application.Commands.RegisterPaymentGateway;
using Obss.Payments.Application.Queries.GetPaymentById;
using Obss.Payments.Application.Queries.GetPaymentSummary;
using Obss.Payments.Application.Queries.GetPayments;
using Obss.Payments.Application.Queries.GetPaymentsByInvoice;
using Obss.Payments.Application.Queries.GetReconciliationStatus;
using Obss.Payments.Application.Queries.GetReconciliations;
using Obss.Payments.Application.Queries.GetRefunds;
using Obss.Payments.Application.Queries.GetSupportedGateways;
using Obss.Payments.Application.Queries.GetUnmatchedTransactions;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Infrastructure;

namespace Obss.Payments.Api.Endpoints;

public static class PaymentEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/payments", async (RecordPaymentCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v{{version}}/payments/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/payments/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPaymentByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/payments", async (
            Guid? customerId,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            IMediator mediator,
            HttpContext httpContext,
            [AsParameters] TmfPaginationRequest pagination) =>
        {
            var query = new GetPaymentsQuery(customerId, status, fromDate, toDate, pagination.Offset, pagination.Limit);
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/payments/by-invoice/{invoiceId:guid}", async (Guid invoiceId, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPaymentsByInvoiceQuery(invoiceId));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/payments/{id:guid}/complete", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new CompletePaymentCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/payments/{id:guid}/refund", async (Guid id, RefundPaymentCommand command, IMediator mediator) =>
        {
            if (id != command.PaymentId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/payments/refunds", async (
            Guid? paymentId,
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            IMediator mediator,
            HttpContext httpContext,
            [AsParameters] TmfPaginationRequest pagination) =>
        {
            var query = new GetRefundsQuery(paymentId, status, fromDate, toDate, pagination.Offset, pagination.Limit);
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/payments/{id:guid}/allocate", async (Guid id, AllocatePaymentCommand command, IMediator mediator) =>
        {
            if (id != command.PaymentId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/payments/summary", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPaymentSummaryQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/payments/gateways", async (RegisterPaymentGatewayCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v{{version}}/payments/gateways/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).WithName("RegisterPaymentGateway");

        group.MapGet("/payments/gateways", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetSupportedGatewaysQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/payments/process", async (ProcessGatewayPaymentCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).WithName("ProcessGatewayPayment");

        group.MapGet("/payments/reconciliation", async (
            string? status,
            DateTime? fromDate,
            DateTime? toDate,
            IMediator mediator,
            HttpContext httpContext,
            [AsParameters] TmfPaginationRequest pagination) =>
        {
            var query = new GetReconciliationsQuery(status, fromDate, toDate, pagination.Offset, pagination.Limit);
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).WithName("GetReconciliations");

        group.MapPost("/payments/reconciliation/import", async (ImportBankStatementCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v{{version}}/payments/reconciliation/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).WithName("ImportBankStatement");

        group.MapGet("/payments/reconciliation/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetReconciliationStatusQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).WithName("GetReconciliationStatus");

        group.MapGet("/payments/reconciliation/unmatched", async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetUnmatchedTransactionsQuery());
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).WithName("GetUnmatchedTransactions");

        group.MapPost("/payments/reconciliation/auto", async (AutoReconcileCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).WithName("AutoReconcile");

        group.MapPost("/payments/reconciliation/{reconciliationId:guid}/match", async (
            Guid reconciliationId,
            ReconcilePaymentCommand command,
            IMediator mediator) =>
        {
            if (reconciliationId != command.ReconciliationId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).WithName("ReconcilePayment");
    }
}
