using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetDegradedLinks;

public sealed record GetDegradedLinksQuery() : IRequest<Result<IReadOnlyList<ConnectivityLinkDto>>>;
