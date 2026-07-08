using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ServiceOrderCharacteristic : Entity<Guid>
{
    public ServiceOrderCharacteristic(Guid id, string key, string value)
        : base(id)
    {
        Key = key;
        Value = value;
    }

    private ServiceOrderCharacteristic() { }

    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
}
