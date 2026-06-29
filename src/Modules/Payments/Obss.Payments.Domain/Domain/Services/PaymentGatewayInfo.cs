using Obss.Payments.Domain.ValueObjects;

namespace Obss.Payments.Domain.Services;

public sealed record PaymentGatewayInfo(
    PaymentProvider Provider,
    string Name,
    IEnumerable<string> SupportedCurrencies);
