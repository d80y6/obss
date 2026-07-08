using Obss.SharedKernel.Domain.Common;

namespace Obss.Provisioning.Domain.Entities;

public class ServiceOrderNote : Entity<Guid>
{
    public ServiceOrderNote(Guid id, string text, string? author)
        : base(id)
    {
        Text = text;
        Author = author;
        CreatedAt = DateTime.UtcNow;
    }

    private ServiceOrderNote() { }

    public string Text { get; private set; } = string.Empty;
    public string? Author { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
