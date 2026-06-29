using Obss.SharedKernel.Domain.Common;

namespace Obss.SharedKernel.Domain.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    private PhoneNumber(string number, string countryCode)
    {
        Number = number;
        CountryCode = countryCode;
    }

    public string Number { get; }
    public string CountryCode { get; }

    public static PhoneNumber Create(string number, string countryCode = "+967")
    {
        if (string.IsNullOrWhiteSpace(number))
            throw new ArgumentException("Phone number cannot be empty", nameof(number));

        var cleaned = new string(number.Where(char.IsDigit).ToArray());

        if (cleaned.Length < 7 || cleaned.Length > 15)
            throw new ArgumentException($"Invalid phone number length: {cleaned.Length}", nameof(number));

        return new PhoneNumber(cleaned, countryCode);
    }

    public string FullNumber => $"{CountryCode}{Number}";

    public override string ToString() => FullNumber;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Number;
        yield return CountryCode;
    }
}