using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetLinkPath;

public sealed record GetLinkPathQuery(Guid SourceElementId, Guid TargetElementId) : IRequest<Result<IReadOnlyList<ConnectivityLinkDto>>>;
