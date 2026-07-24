using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.AAA.Application.Queries.GetAaaLogs;

namespace Obss.AAA.Api.Extensions;

public static class AuditLogEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/logs", async (
            int page,
            int pageSize,
            string? eventType,
            string? username,
            Guid? nasId,
            DateTime? dateFrom,
            DateTime? dateTo,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new GetAaaLogsQuery(
                Page: page > 0 ? page : 1,
                PageSize: pageSize > 0 ? pageSize : 20,
                EventType: eventType,
                Username: username,
                NasId: nasId,
                DateFrom: dateFrom,
                DateTo: dateTo);

            var result = await mediator.Send(query, ct);
            return result.IsSuccess
                ? (IResult)TypedResults.Ok(result.Value)
                : (IResult)TypedResults.NotFound();
        });
    }
}
