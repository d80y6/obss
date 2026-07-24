using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.Orders.Application.Commands.AcknowledgeProductOrderItem;
using Obss.Orders.Application.Commands.AddItemRelationship;
using Obss.Orders.Application.Commands.AddProductOrderItem;
using Obss.Orders.Application.Commands.ApproveProductOrder;
using Obss.Orders.Application.Commands.AssessProductOrderItem;
using Obss.Orders.Application.Commands.CancelProductOrder;
using Obss.Orders.Application.Commands.CancelProductOrderItem;
using Obss.Orders.Application.Commands.CompleteOrderFulfillment;
using Obss.Orders.Application.Commands.CompleteProductOrderItem;
using Obss.Orders.Application.Commands.CreateMilestone;
using Obss.Orders.Application.Commands.CreateProductOrder;
using Obss.Orders.Application.Commands.DeleteProductOrder;
using Obss.Orders.Application.Commands.FailProductOrderItem;
using Obss.Orders.Application.Commands.HoldProductOrderItem;
using Obss.Orders.Application.Commands.PatchProductOrder;
using Obss.Orders.Application.Commands.RejectProductOrderItem;
using Obss.Orders.Application.Commands.RemoveItemRelationship;
using Obss.Orders.Application.Commands.RemoveMilestone;
using Obss.Orders.Application.Commands.RemoveProductOrderItem;
using Obss.Orders.Application.Commands.ResumeProductOrderItem;
using Obss.Orders.Application.Commands.StartProductOrderItem;
using Obss.Orders.Application.Commands.SubmitProductOrder;
using Obss.Orders.Application.Commands.UpdateMilestone;
using Obss.Orders.Application.Commands.ValidateProductOrder;
using Obss.Orders.Application.Queries.GetOrderFulfillmentStatus;
using Obss.Orders.Application.Queries.GetProductOrderById;
using Obss.Orders.Application.Queries.GetProductOrderMilestones;
using Obss.Orders.Application.Queries.GetProductOrderItemRelationships;
using Obss.Orders.Application.Queries.GetProductOrders;
using Obss.Orders.Application.Queries.GetProductOrdersByCustomer;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Infrastructure;
using Obss.SharedKernel.Application.Authorization;

namespace Obss.Orders.Api.Endpoints;

public static class ProductOrderEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/orders", async (CreateProductOrderCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/productOrder/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite));

        group.MapGet("/orders/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProductOrderByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderRead));

        group.MapGet("/orders", async ([AsParameters] GetProductOrdersQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(query);
            if (!result.IsSuccess)
                return (IResult)TypedResults.BadRequest(result.Error);

            var paginationRequest = new TmfPaginationRequest { Offset = query.Offset, Limit = query.Limit };
            httpContext.Response.SetPaginationHeaders(paginationRequest, result.Value.TotalCount);
            return (IResult)TypedResults.Ok(result.Value.Items);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderRead));

        group.MapGet("/customers/{customerId:guid}/orders", async (Guid customerId, [AsParameters] GetProductOrdersByCustomerQuery query, IMediator mediator, HttpContext httpContext) =>
        {
            if (customerId != query.CustomerId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(query);
            if (!result.IsSuccess)
                return (IResult)TypedResults.BadRequest(result.Error);

            var paginationRequest = new TmfPaginationRequest { Offset = query.Offset, Limit = query.Limit };
            httpContext.Response.SetPaginationHeaders(paginationRequest, result.Value.TotalCount);
            return (IResult)TypedResults.Ok(result.Value.Items);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderRead));

        group.MapPost("/orders/{id:guid}/submit", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new SubmitProductOrderCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite));

        group.MapPost("/orders/{id:guid}/approve", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ApproveProductOrderCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderApprove));

        group.MapPost("/orders/{id:guid}/cancel", async (Guid id, CancelProductOrderCommand command, IMediator mediator) =>
        {
            if (id != command.OrderId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderCancel));

        group.MapPost("/orders/{id:guid}/items", async (Guid id, AddProductOrderItemCommand command, IMediator mediator) =>
        {
            if (id != command.OrderId)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite));

        group.MapDelete("/orders/{orderId:guid}/items/{itemId:guid}", async (Guid orderId, Guid itemId, IMediator mediator) =>
        {
            var result = await mediator.Send(new RemoveProductOrderItemCommand(orderId, itemId));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite));

        group.MapGet("/orders/{id:guid}/fulfillment", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetOrderFulfillmentStatusQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderFulfill));

        group.MapPost("/orders/{id:guid}/fulfillment/complete", async (Guid id, CompleteOrderFulfillmentCommand command, IMediator mediator) =>
        {
            if (id != command.OrderId)
                return (IResult)TypedResults.BadRequest();
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderFulfill));

        group.MapPost("/orders/{id:guid}/validate", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new ValidateProductOrderCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite));

        group.MapPatch("/orders/{id:guid}", async (Guid id, PatchProductOrderCommand command, IMediator mediator) =>
        {
            if (id != command.Id)
                return (IResult)TypedResults.BadRequest();

            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite));

        group.MapDelete("/orders/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new DeleteProductOrderCommand(id));
            return result.IsSuccess
                ? (IResult)TypedResults.NoContent()
                : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite));

        // Item state transitions
        group.MapPost("/{id:guid}/items/{itemId:guid}/acknowledge", async (Guid id, Guid itemId, ISender sender) =>
        {
            var result = await sender.Send(new AcknowledgeProductOrderItemCommand(id, itemId));
            return result.IsSuccess ? (IResult)TypedResults.Ok() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("AcknowledgeProductOrderItem");

        group.MapPost("/{id:guid}/items/{itemId:guid}/start", async (Guid id, Guid itemId, ISender sender) =>
        {
            var result = await sender.Send(new StartProductOrderItemCommand(id, itemId));
            return result.IsSuccess ? (IResult)TypedResults.Ok() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("StartProductOrderItem");

        group.MapPost("/{id:guid}/items/{itemId:guid}/hold", async (Guid id, Guid itemId, ISender sender) =>
        {
            var result = await sender.Send(new HoldProductOrderItemCommand(id, itemId));
            return result.IsSuccess ? (IResult)TypedResults.Ok() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("HoldProductOrderItem");

        group.MapPost("/{id:guid}/items/{itemId:guid}/resume", async (Guid id, Guid itemId, ISender sender) =>
        {
            var result = await sender.Send(new ResumeProductOrderItemCommand(id, itemId));
            return result.IsSuccess ? (IResult)TypedResults.Ok() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("ResumeProductOrderItem");

        group.MapPost("/{id:guid}/items/{itemId:guid}/assess", async (Guid id, Guid itemId, ISender sender) =>
        {
            var result = await sender.Send(new AssessProductOrderItemCommand(id, itemId));
            return result.IsSuccess ? (IResult)TypedResults.Ok() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("AssessProductOrderItem");

        group.MapPost("/{id:guid}/items/{itemId:guid}/reject", async (Guid id, Guid itemId, RejectProductOrderItemCommand command, ISender sender) =>
        {
            var result = await sender.Send(new RejectProductOrderItemCommand(id, itemId, command.Reason));
            return result.IsSuccess ? (IResult)TypedResults.Ok() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("RejectProductOrderItem");

        group.MapPost("/{id:guid}/items/{itemId:guid}/complete", async (Guid id, Guid itemId, ISender sender) =>
        {
            var result = await sender.Send(new CompleteProductOrderItemCommand(id, itemId));
            return result.IsSuccess ? (IResult)TypedResults.Ok() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("CompleteProductOrderItem");

        group.MapPost("/{id:guid}/items/{itemId:guid}/fail", async (Guid id, Guid itemId, FailProductOrderItemCommand command, ISender sender) =>
        {
            var result = await sender.Send(new FailProductOrderItemCommand(id, itemId, command.Error));
            return result.IsSuccess ? (IResult)TypedResults.Ok() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("FailProductOrderItem");

        group.MapPost("/{id:guid}/items/{itemId:guid}/cancel", async (Guid id, Guid itemId, ISender sender) =>
        {
            var result = await sender.Send(new CancelProductOrderItemCommand(id, itemId));
            return result.IsSuccess ? (IResult)TypedResults.Ok() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("CancelProductOrderItem");

        // Relationships
        group.MapGet("/{id:guid}/relationships", async (Guid id, Guid? itemId, ISender sender) =>
        {
            var result = await sender.Send(new GetProductOrderItemRelationshipsQuery(id, itemId));
            return result.IsSuccess ? (IResult)TypedResults.Ok(result.Value) : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderRead)).WithName("GetProductOrderRelationships");

        group.MapPost("/{id:guid}/relationships", async (Guid id, AddItemRelationshipCommand command, ISender sender) =>
        {
            var result = await sender.Send(new AddItemRelationshipCommand(id, command.ItemId, command.TargetItemId, command.Type));
            return result.IsSuccess ? Results.Created($"/api/v1/productOrder/{id}/relationships", null) : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("AddProductOrderRelationship");

        group.MapDelete("/{id:guid}/relationships/{relationshipId:guid}", async (Guid id, Guid relationshipId, ISender sender) =>
        {
            var result = await sender.Send(new RemoveItemRelationshipCommand(id, relationshipId));
            return result.IsSuccess ? (IResult)TypedResults.NoContent() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("RemoveProductOrderRelationship");

        // Milestones
        group.MapGet("/{id:guid}/milestones", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetProductOrderMilestonesQuery(id));
            return result.IsSuccess ? (IResult)TypedResults.Ok(result.Value) : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderRead)).WithName("GetProductOrderMilestones");

        group.MapPost("/{id:guid}/milestones", async (Guid id, CreateMilestoneCommand command, ISender sender) =>
        {
            var result = await sender.Send(new CreateMilestoneCommand(id, command.Name, command.Description, command.MilestoneDate));
            return result.IsSuccess ? Results.Created($"/api/v1/productOrder/{id}/milestones", null) : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("CreateProductOrderMilestone");

        group.MapPatch("/{id:guid}/milestones/{milestoneId:guid}", async (Guid id, Guid milestoneId, UpdateMilestoneCommand command, ISender sender) =>
        {
            var result = await sender.Send(new UpdateMilestoneCommand(id, milestoneId, command.Status, command.MilestoneDate));
            return result.IsSuccess ? (IResult)TypedResults.Ok() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("UpdateProductOrderMilestone");

        group.MapDelete("/{id:guid}/milestones/{milestoneId:guid}", async (Guid id, Guid milestoneId, ISender sender) =>
        {
            var result = await sender.Send(new RemoveMilestoneCommand(id, milestoneId));
            return result.IsSuccess ? (IResult)TypedResults.NoContent() : (IResult)TypedResults.BadRequest(result.Error);
        }).RequireAuthorization(Permissions.PolicyName(Permissions.Orders.OrderWrite)).WithName("RemoveProductOrderMilestone");
    }
}
