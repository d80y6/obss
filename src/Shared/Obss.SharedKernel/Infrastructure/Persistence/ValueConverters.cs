using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.SharedKernel.Infrastructure.Persistence;

public class EmailValueConverter : ValueConverter<Email, string>
{
    public EmailValueConverter() : base(
        v => v.Value,
        v => Email.Create(v))
    { }
}

public class TenantIdValueConverter : ValueConverter<TenantId, string>
{
    public TenantIdValueConverter() : base(
        v => v.Value,
        v => TenantId.Create(v))
    { }
}

public class PhoneNumberValueConverter : ValueConverter<PhoneNumber, string>
{
    public PhoneNumberValueConverter() : base(
        v => v.FullNumber,
        v => PhoneNumber.Create(v, "+967"))
    { }
}
