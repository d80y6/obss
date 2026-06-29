using Obss.SharedKernel.Domain.Common;

namespace Obss.SharedKernel.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public static readonly Money Zero = new(0m, Currency.Usd);

    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }
    public Currency Currency { get; }

    public static Money Create(decimal amount, string currencyCode)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));

        var currency = Currency.FromCode(currencyCode);
        return new Money(amount, currency);
    }

    public static Money FromUsd(decimal amount) => new(amount, Currency.Usd);

    public static Money FromYer(decimal amount) => new(amount, Currency.Yer);

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot add {other.Currency.Code} to {Currency.Code}");

        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Cannot subtract {other.Currency.Code} from {Currency.Code}");

        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal multiplier) => new(Amount * multiplier, Currency);

    public Money Negate() => new(-Amount, Currency);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

public sealed class Currency : ValueObject
{
    public static readonly Currency Usd = new("USD", "US Dollar", 2, "840");
    public static readonly Currency Yer = new("YER", "Yemeni Rial", 2, "886");

    private Currency(string code, string name, int decimalPlaces, string numericCode)
    {
        Code = code;
        Name = name;
        DecimalPlaces = decimalPlaces;
        NumericCode = numericCode;
    }

    public string Code { get; }
    public string Name { get; }
    public int DecimalPlaces { get; }
    public string NumericCode { get; }

    public static Currency FromCode(string code)
    {
        return code.ToUpperInvariant() switch
        {
            "USD" => Usd,
            "YER" => Yer,
            _ => throw new ArgumentException($"Unsupported currency code: {code}", nameof(code))
        };
    }

    public static IReadOnlyCollection<Currency> GetAll() => [Usd, Yer];

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}