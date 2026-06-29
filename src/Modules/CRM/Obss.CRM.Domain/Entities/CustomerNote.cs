using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Entities;

public class CustomerNote : Entity<Guid>
{
    private CustomerNote() { }

    public CustomerNote(
        Guid id,
        Guid customerId,
        string content,
        NoteCategory category,
        string createdById)
        : base(id)
    {
        CustomerId = customerId;
        Content = content;
        Category = category;
        CreatedById = createdById;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid CustomerId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public NoteCategory Category { get; private set; }
    public string CreatedById { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    public Customer Customer { get; private set; } = null!;
}
