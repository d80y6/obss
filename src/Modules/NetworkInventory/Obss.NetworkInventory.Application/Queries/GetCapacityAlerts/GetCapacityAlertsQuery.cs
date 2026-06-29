using MediatR;
using Obss.NetworkInventory.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NetworkInventory.Application.Queries.GetCapacityAlerts;

public sealed record GetCapacityAlertsQuery() : IRequest<Result<IReadOnlyList<CapacityRecordDto>>>;
