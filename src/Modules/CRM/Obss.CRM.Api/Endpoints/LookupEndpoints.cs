using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Obss.CRM.Domain.ValueObjects;

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
            var currencies = new[]
            {
                new { value = "USD", label = "US Dollar" },
                new { value = "YER", label = "Yemeni Rial" },
                new { value = "SAR", label = "Saudi Riyal" },
                new { value = "EUR", label = "Euro" },
            };
            return TypedResults.Ok(currencies);
        }).AllowAnonymous();
    }
}
