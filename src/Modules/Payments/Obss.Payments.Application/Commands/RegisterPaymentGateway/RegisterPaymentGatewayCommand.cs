using MediatR;
using Obss.Payments.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Payments.Application.Commands.RegisterPaymentGateway;

public sealed record RegisterPaymentGatewayCommand(
    string Name,
    string Provider,
    string Configuration,
    List<string> SupportedCurrencies,
    decimal? MinAmount,
    decimal? MaxAmount,
    decimal TransactionFee,
    string FeeType) : IRequest<Result<PaymentGatewayDto>>;
