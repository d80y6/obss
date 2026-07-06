using Microsoft.AspNetCore.Http;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.SharedKernel.Infrastructure;

public static class PaginationExtensions
{
    public static void SetPaginationHeaders(this HttpResponse response, TmfPaginationRequest request, int totalCount)
    {
        var result = new TmfPaginationResponse();
        result.SetPaginationHeaders(request.Offset, request.Limit, totalCount);

        response.Headers["X-Total-Count"] = result.TotalCount.ToString();
        response.Headers["X-Result-Count"] = result.ResultCount.ToString();
        response.Headers["X-Offset"] = result.Offset.ToString();
        response.Headers["X-Limit"] = result.Limit.ToString();
    }
}
