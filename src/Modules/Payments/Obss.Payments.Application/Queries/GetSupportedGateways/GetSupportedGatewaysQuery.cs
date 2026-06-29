using MediatR;
using Obss.Payments.Domain.Services;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetSupportedGateways;

public sealed record GetSupportedGatewaysQuery : IRequest<Result<IEnumerable<PaymentGatewayInfo>>>;
