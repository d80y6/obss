using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.ProductCatalog.Domain.Domain.Exceptions;

[Serializable]
public sealed class ProductNotFoundException : DomainException
{
    public ProductNotFoundException() { }
    public ProductNotFoundException(string message) : base(message) { }
    public ProductNotFoundException(Guid productId)
        : base($"Product with identifier '{productId}' was not found.") { }
    public ProductNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private ProductNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class OfferNotFoundException : DomainException
{
    public OfferNotFoundException() { }
    public OfferNotFoundException(string message) : base(message) { }
    public OfferNotFoundException(Guid offerId)
        : base($"Offer with identifier '{offerId}' was not found.") { }
    public OfferNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private OfferNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class DuplicateSkuException : DomainException
{
    public DuplicateSkuException() { }
    public DuplicateSkuException(string sku)
        : base($"A product with SKU '{sku}' already exists.") { }
    public DuplicateSkuException(string message, Exception innerException)
        : base(message, innerException) { }
    private DuplicateSkuException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidPricingException : DomainException
{
    public InvalidPricingException() { }
    public InvalidPricingException(string message)
        : base(message) { }
    public InvalidPricingException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidPricingException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
