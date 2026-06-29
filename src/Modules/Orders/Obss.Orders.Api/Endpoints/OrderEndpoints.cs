using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Orders.Application.Commands.AddOrderItem;
using Obss.Orders.Application.Commands.ApproveOrder;
using Obss.Orders.Application.Commands.CancelOrder;
using Obss.Orders.Application.Commands.CompleteOrderFulfillment;
using Obss.Orders.Application.Commands.CreateOrder;
using Obss.Orders.Application.Commands.SubmitOrder;
using Obss.Orders.Application.Commands.ValidateOrder;
using Obss.Orders.Application.Queries.GetOrderById;
using Obss.Orders.Application.Queries.GetOrderFulfillmentStatus;
using Obss.Orders.Application.Queries.GetOrders;
using Obss.Orders.Application.Queries.GetOrdersByCustomer;

namespace Obss.Orders.Api.Endpoints;

public static class OrderEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/orders", async (CreateOrderCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/orders/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/orders/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOrderByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/orders", async ([AsParameters] GetOrdersQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/customers/{customerId:guid}/orders", async (Guid customerId, [AsParameters] GetOrdersByCustomerQuery query, IMediator mediator) =>
        {
            if (customerId != query.CustomerId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(query);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/orders/{id:guid}/submit", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SubmitOrderCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/orders/{id:guid}/approve", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ApproveOrderCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/orders/{id:guid}/cancel", async (Guid id, CancelOrderCommand command, IMediator mediator) =>
        {
            if (id != command.OrderId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/orders/{id:guid}/items", async (Guid id, AddOrderItemCommand command, IMediator mediator) =>
        {
            if (id != command.OrderId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/orders/{id:guid}/fulfillment", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOrderFulfillmentStatusQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapPost("/orders/{id:guid}/fulfillment/complete", async (Guid id, CompleteOrderFulfillmentCommand command, IMediator mediator) =>
        {
            if (id != command.OrderId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapPost("/orders/{id:guid}/validate", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ValidateOrderCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
