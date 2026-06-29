using MediatR;
using Obss.Payments.Domain.Services;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Queries.GetSupportedGateways;

public sealed class GetSupportedGatewaysQueryHandler : IRequestHandler<GetSupportedGatewaysQuery, Result<IEnumerable<PaymentGatewayInfo>>>
{
    private readonly IPaymentGatewayService _gatewayService;

    public GetSupportedGatewaysQueryHandler(IPaymentGatewayService gatewayService)
    {
        _gatewayService = gatewayService;
    }

    public async Task<Result<IEnumerable<PaymentGatewayInfo>>> Handle(GetSupportedGatewaysQuery request, CancellationToken cancellationToken)
    {
        var gateways = await _gatewayService.GetSupportedGateways(cancellationToken);
        return Result.Success(gateways);
    }
}
