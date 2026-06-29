using Obss.SharedKernel.Domain.Common;

namespace Obss.Invoices.Domain.Entities;

public class DisputeAttachment : Entity<Guid>
{
    private DisputeAttachment() { }

    public DisputeAttachment(Guid id, Guid invoiceDisputeId, string fileName, string contentType, long fileSize, string storagePath)
        : base(id)
    {
        InvoiceDisputeId = invoiceDisputeId;
        FileName = fileName;
        ContentType = contentType;
        FileSize = fileSize;
        StoragePath = storagePath;
        UploadedAt = DateTime.UtcNow;
    }

    public Guid InvoiceDisputeId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }
}
