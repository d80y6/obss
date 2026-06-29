using Obss.SharedKernel.Domain.Common;

namespace Obss.Ticketing.Domain.Entities;

public class TicketComment : Entity<Guid>
{
    private TicketComment() { }

    public TicketComment(Guid id, Guid ticketId, string content, bool isInternal, string authorId, string authorName)
        : base(id)
    {
        TicketId = ticketId;
        Content = content;
        IsInternal = isInternal;
        AuthorId = authorId;
        AuthorName = authorName;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid TicketId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public bool IsInternal { get; private set; }
    public string AuthorId { get; private set; } = string.Empty;
    public string AuthorName { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public Ticket Ticket { get; private set; } = null!;
}
