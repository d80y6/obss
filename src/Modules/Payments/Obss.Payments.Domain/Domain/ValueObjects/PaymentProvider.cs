namespace Obss.Payments.Domain.ValueObjects;

public enum PaymentProvider
{
    Stripe,
    PayPal,
    LocalBank,
    MobileMoney,
    Cash
}
