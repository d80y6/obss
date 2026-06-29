using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.ModuleTemplate.Application.Commands.CreateSample;
using Obss.ModuleTemplate.Application.Queries.GetSamples;

namespace Obss.ModuleTemplate.Api.Endpoints;

public static class SampleEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("/samples", async (CreateSampleCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess
                ? Results.Created($"/api/v1/samples/{result.Value.Id}", result.Value)
                : Results.BadRequest(result.Error);
        });

        group.MapGet("/samples", async ([AsParameters] GetSamplesQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Error);
        });
    }
}
