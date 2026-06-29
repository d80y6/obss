using MediatR;
using Obss.ApiGateway.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Queries.GetPartners;

public sealed record GetPartnersQuery : IRequest<Result<IReadOnlyList<PartnerDto>>>;
