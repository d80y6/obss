using Obss.SharedKernel.Domain.Common;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Domain.Entities;

public class Contact : Entity<Guid>
{
    private Contact() { }

    public Contact(
        Guid id,
        Guid customerId,
        string firstName,
        string lastName,
        Email email,
        PhoneNumber? phoneNumber,
        PhoneNumber? mobileNumber,
        string? position,
        bool isPrimary,
        bool isBilling,
        bool isTechnical)
        : base(id)
    {
        CustomerId = customerId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        MobileNumber = mobileNumber;
        Position = position;
        IsPrimary = isPrimary;
        IsBilling = isBilling;
        IsTechnical = isTechnical;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid CustomerId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public Email Email { get; private set; } = default!;
    public PhoneNumber? PhoneNumber { get; private set; }
    public PhoneNumber? MobileNumber { get; private set; }
    public string? Position { get; private set; }
    public bool IsPrimary { get; private set; }
    public bool IsBilling { get; private set; }
    public bool IsTechnical { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Customer Customer { get; private set; } = null!;

    public void UpdateDetails(
        string firstName,
        string lastName,
        Email email,
        PhoneNumber? phoneNumber,
        PhoneNumber? mobileNumber,
        string? position,
        bool isPrimary,
        bool isBilling,
        bool isTechnical)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        MobileNumber = mobileNumber;
        Position = position;
        IsPrimary = isPrimary;
        IsBilling = isBilling;
        IsTechnical = isTechnical;
    }
}
