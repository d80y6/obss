using MediatR;
using Obss.ServiceQualification.Application.Commands.CheckServiceQualification;
using Obss.ServiceQualification.Application.Queries.GetServiceQualificationById;
using Obss.ServiceQualification.Application.Queries.GetServiceQualifications;

namespace Obss.ServiceQualification.Api.Endpoints;

public static class ServiceQualificationEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/", async (CheckServiceQualificationCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? (IResult)TypedResults.Created($"/api/v1/service-qualification/{result.Value.Id}", result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetServiceQualificationByIdQuery(id));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound(result.Error);
        });

        group.MapGet("/", async (
            Guid? customerId,
            DateTime? from,
            DateTime? to,
            IMediator mediator) =>
        {
            var result = await mediator.Send(new GetServiceQualificationsQuery(customerId, from, to, null));
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.BadRequest(result.Error);
        });
    }
}
