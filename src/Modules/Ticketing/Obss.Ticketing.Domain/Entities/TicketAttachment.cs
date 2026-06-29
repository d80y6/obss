using Obss.SharedKernel.Domain.Common;

namespace Obss.Ticketing.Domain.Entities;

public class TicketAttachment : Entity<Guid>
{
    private TicketAttachment() { }

    public TicketAttachment(Guid id, Guid ticketId, string fileName, string contentType, long fileSize, string storagePath, string uploadedById)
        : base(id)
    {
        TicketId = ticketId;
        FileName = fileName;
        ContentType = contentType;
        FileSize = fileSize;
        StoragePath = storagePath;
        UploadedById = uploadedById;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid TicketId { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }
    public string StoragePath { get; private set; } = string.Empty;
    public string UploadedById { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public Ticket Ticket { get; private set; } = null!;
}
