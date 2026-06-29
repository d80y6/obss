using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Queries.GetCollectionCaseById;

public sealed record GetCollectionCaseByIdQuery(Guid CaseId) : IRequest<Result<CollectionCaseDto>>;
