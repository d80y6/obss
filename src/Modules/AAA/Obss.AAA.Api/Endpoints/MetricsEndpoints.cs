using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.AAA.Application.Queries.GetAaaMetrics;

namespace Obss.AAA.Api.Extensions;

public static class MetricsEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/metrics", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAaaMetricsQuery(), ct);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound();
        });
    }
}
