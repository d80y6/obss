using Obss.SharedKernel.Domain.Common;

namespace Obss.SharedKernel.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    private Address(string street, string city, string? state, string? postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public string Street { get; }
    public string City { get; }
    public string? State { get; }
    public string? PostalCode { get; }
    public string Country { get; }

    public static Address Create(string street, string city, string? state, string? postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be empty", nameof(street));
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        return new Address(street, city, state, postalCode, country);
    }

    public override string ToString() => $"{Street}, {City}{(State is not null ? $", {State}" : "")}, {Country}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State ?? string.Empty;
        yield return PostalCode ?? string.Empty;
        yield return Country;
    }
}