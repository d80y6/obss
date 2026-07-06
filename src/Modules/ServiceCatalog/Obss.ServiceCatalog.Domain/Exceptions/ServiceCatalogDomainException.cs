using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.ServiceCatalog.Domain.Exceptions;

[Serializable]
public class ServiceCatalogDomainException : DomainException
{
    public ServiceCatalogDomainException() { }
    public ServiceCatalogDomainException(string message) : base(message) { }
    public ServiceCatalogDomainException(string message, Exception inner) : base(message, inner) { }
    protected ServiceCatalogDomainException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}
