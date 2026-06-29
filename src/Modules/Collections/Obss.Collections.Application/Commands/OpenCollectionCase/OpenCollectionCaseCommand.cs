using MediatR;
using Obss.Collections.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.OpenCollectionCase;

public sealed record OpenCollectionCaseCommand(
    Guid CustomerId,
    string CustomerName,
    decimal TotalOverdueAmount,
    string Currency) : IRequest<Result<CollectionCaseDto>>;
