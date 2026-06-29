using Obss.SharedKernel.Domain.Common;

namespace Obss.IAM.Domain.Entities;

public class UserRole : Entity<Guid>
{
    private UserRole() { }

    public UserRole(Guid id, Guid userId, Guid roleId, Guid assignedBy)
        : base(id)
    {
        UserId = userId;
        RoleId = roleId;
        AssignedBy = assignedBy;
        AssignedAt = DateTime.UtcNow;
    }

    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public Guid AssignedBy { get; private set; }

    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;
}
