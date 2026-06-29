using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Queries.GetCollectionCases;

public sealed record GetCollectionCasesQuery(
    string? Status,
    Guid? CustomerId,
    int? DunningLevel,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<IReadOnlyList<CollectionCaseDto>>>;
