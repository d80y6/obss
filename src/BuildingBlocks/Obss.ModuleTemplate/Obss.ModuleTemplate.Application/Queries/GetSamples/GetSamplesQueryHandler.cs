using Mapster;
using MediatR;
using Obss.ModuleTemplate.Application.Abstractions;
using Obss.ModuleTemplate.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ModuleTemplate.Application.Queries.GetSamples;

public sealed class GetSamplesQueryHandler : IRequestHandler<GetSamplesQuery, Result<IReadOnlyList<SampleDto>>>
{
    private readonly ISampleRepository _sampleRepository;

    public GetSamplesQueryHandler(ISampleRepository sampleRepository)
    {
        _sampleRepository = sampleRepository;
    }

    public async Task<Result<IReadOnlyList<SampleDto>>> Handle(GetSamplesQuery request, CancellationToken cancellationToken)
    {
        var samples = await _sampleRepository.GetFilteredAsync(
            request.TenantId,
            request.IsActive,
            request.SearchTerm,
            request.Page,
            request.PageSize,
            cancellationToken);

        var result = samples.Adapt<List<SampleDto>>();
        return Result.Success<IReadOnlyList<SampleDto>>(result);
    }
}
