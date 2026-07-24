using Obss.AAA.Domain.Entities;
using Obss.AAA.Domain.ValueObjects;

namespace Obss.AAA.Application.Abstractions;

public interface IAaaAdapterSelector
{
    IAaaAdapter GetAdapter(AaaProtocolType protocolType);
    IAaaAdapter GetAdapter(NetworkAccessServer nas);
    IReadOnlyDictionary<AaaProtocolType, IAaaAdapter> AllAdapters { get; }
}
