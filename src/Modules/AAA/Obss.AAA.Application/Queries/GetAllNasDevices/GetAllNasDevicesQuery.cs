using MediatR;
using Obss.AAA.Application.Contracts;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAllNasDevices;

public sealed record GetAllNasDevicesQuery(
    int Page = 1,
    int PageSize = 20,
    string? NasType = null) : IRequest<Result<PaginatedResult<NasDto>>>;
