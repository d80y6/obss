using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.CRM.Domain.Entities;

public class QuoteItem : Entity<Guid>
{
    private readonly List<QuotePrice> _prices = [];
    private readonly List<QuoteItemRelationship> _itemRelationships = [];
    private readonly List<Note> _notes = [];

    private QuoteItem() { }

    public QuoteItem(
        Guid id,
        QuoteItemAction action,
        int quantity,
        Guid? productOfferingId,
        string? productOfferingName,
        Guid? productId)
        : base(id)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        Action = action;
        State = QuoteItemState.InProgress;
        Quantity = quantity;
        ProductOfferingId = productOfferingId;
        ProductOfferingName = productOfferingName;
        ProductId = productId;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public QuoteItemAction Action { get; private set; }
    public QuoteItemState State { get; private set; }
    public int Quantity { get; private set; }
    public Guid? ProductOfferingId { get; private set; }
    public string? ProductOfferingName { get; private set; }
    public Guid? ProductId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public IReadOnlyCollection<QuotePrice> Prices => _prices.AsReadOnly();
    public IReadOnlyCollection<QuoteItemRelationship> ItemRelationships => _itemRelationships.AsReadOnly();
    public IReadOnlyCollection<Note> Notes => _notes.AsReadOnly();

    public void UpdateDetails(QuoteItemAction action, int quantity, Guid? productOfferingId, string? productOfferingName, Guid? productId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        Action = action;
        Quantity = quantity;
        ProductOfferingId = productOfferingId;
        ProductOfferingName = productOfferingName;
        ProductId = productId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetState(QuoteItemState state)
    {
        State = state;
        UpdatedAt = DateTime.UtcNow;
    }
    public void AddPrice(QuotePrice price) { _prices.Add(price); UpdatedAt = DateTime.UtcNow; }
    public void AddItemRelationship(QuoteItemRelationship relationship) { _itemRelationships.Add(relationship); UpdatedAt = DateTime.UtcNow; }
    public void AddNote(Note note) { _notes.Add(note); UpdatedAt = DateTime.UtcNow; }
}
