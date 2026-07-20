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
    public static readonly Currency Usd = new("USD", "US Dollar", "دولار أمريكي", 2, "840", "en-US");
    public static readonly Currency Yer = new("YER", "Yemeni Rial", "ريال يمني", 2, "886", "ar-YE");
    public static readonly Currency Sar = new("SAR", "Saudi Riyal", "ريال سعودي", 2, "682", "ar-SA");

    private static readonly Dictionary<string, Currency> _all = new()
    {
        [Usd.Code] = Usd,
        [Yer.Code] = Yer,
        [Sar.Code] = Sar,
    };

    private Currency(string code, string name, string nameAr, int decimalPlaces, string numericCode, string defaultCulture)
    {
        Code = code;
        Name = name;
        NameAr = nameAr;
        DecimalPlaces = decimalPlaces;
        NumericCode = numericCode;
        DefaultCulture = defaultCulture;
    }

    public string Code { get; }
    public string Name { get; }
    public string NameAr { get; }
    public int DecimalPlaces { get; }
    public string NumericCode { get; }
    public string DefaultCulture { get; }

    public static Currency FromCode(string code)
        => _all.TryGetValue(code.ToUpperInvariant(), out var currency) ? currency : Yer;

    public static IReadOnlyCollection<Currency> GetAll() => [.. _all.Values];

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
    }
}
