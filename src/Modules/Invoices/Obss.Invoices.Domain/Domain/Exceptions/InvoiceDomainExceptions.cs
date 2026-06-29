using System.Runtime.Serialization;
using Obss.SharedKernel.Domain.Exceptions;

namespace Obss.Invoices.Domain.Exceptions;

[Serializable]
public sealed class InvoiceNotFoundException : DomainException
{
    public InvoiceNotFoundException() { }
    public InvoiceNotFoundException(string message) : base(message) { }
    public InvoiceNotFoundException(Guid invoiceId)
        : base($"Invoice with id '{invoiceId}' was not found.") { }
    public InvoiceNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvoiceNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class InvalidInvoiceStateException : DomainException
{
    public InvalidInvoiceStateException() { }
    public InvalidInvoiceStateException(string message)
        : base(message) { }
    public InvalidInvoiceStateException(string message, Exception innerException)
        : base(message, innerException) { }
    private InvalidInvoiceStateException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

[Serializable]
public sealed class CreditNoteNotFoundException : DomainException
{
    public CreditNoteNotFoundException() { }
    public CreditNoteNotFoundException(string message) : base(message) { }
    public CreditNoteNotFoundException(Guid creditNoteId)
        : base($"Credit note with id '{creditNoteId}' was not found.") { }
    public CreditNoteNotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
    private CreditNoteNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}
