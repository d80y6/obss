using MediatR;
using Obss.AAA.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Queries.GetAllNasDevices;

public sealed record GetAllNasDevicesQuery : IRequest<Result<IReadOnlyList<NasDto>>>;
