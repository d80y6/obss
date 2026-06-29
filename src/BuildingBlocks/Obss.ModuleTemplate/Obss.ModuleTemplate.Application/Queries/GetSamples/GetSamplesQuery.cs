using MediatR;
using Obss.ModuleTemplate.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ModuleTemplate.Application.Queries.GetSamples;

public sealed record GetSamplesQuery(
    string? TenantId,
    bool? IsActive,
    string? SearchTerm,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<SampleDto>>>;
