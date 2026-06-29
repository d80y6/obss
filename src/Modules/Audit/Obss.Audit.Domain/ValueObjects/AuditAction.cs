namespace Obss.Audit.Domain.ValueObjects;

public enum AuditAction
{
    Created,
    Updated,
    Deleted,
    Viewed,
    Exported,
    Restored,
    Login,
    Logout,
    FailedLogin
}
