using Obss.SharedKernel.Domain.Common;

namespace Obss.IAM.Domain.Entities;

public class Permission : Entity<Guid>
{
    private Permission() { }

    public Permission(
        Guid id,
        string code,
        string name,
        string? description,
        string module,
        string resource,
        string action)
        : base(id)
    {
        Code = code;
        Name = name;
        Description = description;
        Module = module;
        Resource = resource;
        Action = action;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Module { get; private set; } = string.Empty;
    public string Resource { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
}
