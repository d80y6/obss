using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.ResolveCollectionCase;

public sealed record ResolveCollectionCaseCommand(Guid CaseId) : IRequest<Result<CollectionCaseDto>>;
