using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Api.Endpoints;

public static class LookupEndpoints
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/lookups/customer-types", () =>
        {
            var values = Enum.GetNames<CustomerType>();
            return TypedResults.Ok(values.Select(v => new { value = v, label = v }));
        }).AllowAnonymous();

        group.MapGet("/lookups/currencies", () =>
        {
            var currencies = Currency.GetAll().Select(c => new
            {
                value = c.Code,
                label = c.Name,
                labelAr = c.NameAr
            });
            return TypedResults.Ok(currencies);
        }).AllowAnonymous();
    }
}
