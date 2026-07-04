using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ProductCatalog.Application.Commands.RemoveProductOfferingTerm;

public sealed record RemoveProductOfferingTermCommand(
    Guid OfferId,
    Guid TermId) : IRequest<Result<Unit>>;
