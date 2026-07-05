using Mapster;
using MediatR;
using Obss.Collections.Application.Abstractions;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Queries.GetAgingReport;

public sealed class GetAgingReportQueryHandler : IRequestHandler<GetAgingReportQuery, Result<AgingReportDto>>
{
    private readonly ICollectionCaseRepository _caseRepository;

    public GetAgingReportQueryHandler(ICollectionCaseRepository caseRepository)
    {
        _caseRepository = caseRepository;
    }

    public async Task<Result<AgingReportDto>> Handle(GetAgingReportQuery request, CancellationToken cancellationToken)
    {
        var currency = request.Currency ?? "USD";
        var buckets = await _caseRepository.GetAgingBucketsAsync(currency, cancellationToken);

        var bucketConfigs = new[]
        {
            new { Name = "1-30 Days", Min = 1, Max = 30 },
            new { Name = "31-60 Days", Min = 31, Max = 60 },
            new { Name = "61-90 Days", Min = 61, Max = 90 },
            new { Name = "91-120 Days", Min = 91, Max = 120 },
            new { Name = "120+ Days", Min = 121, Max = int.MaxValue }
        };

        var bucketDtos = new List<AgingBucketDto>();
        var totalCases = 0;
        var totalCustomers = 0;
        var grandTotal = 0m;

        foreach (var config in bucketConfigs)
        {
            var data = buckets
                .Where(kvp => kvp.Key >= config.Min && kvp.Key <= config.Max)
                .ToList();

            var bucketCaseCount = data.Sum(d => d.Value.CaseCount);
            var bucketCustomerCount = data.Sum(d => d.Value.CustomerCount);
            var bucketAmount = data.Sum(d => d.Value.TotalAmount);

            bucketDtos.Add(new AgingBucketDto(
                config.Name,
                config.Min,
                config.Max == int.MaxValue ? 9999 : config.Max,
                bucketCustomerCount,
                bucketCaseCount,
                bucketAmount,
                currency));

            totalCases += bucketCaseCount;
            totalCustomers += bucketCustomerCount;
            grandTotal += bucketAmount;
        }

        return Result.Success(new AgingReportDto(
            DateTime.UtcNow,
            bucketDtos,
            totalCustomers,
            totalCases,
            grandTotal,
            currency));
    }
}
