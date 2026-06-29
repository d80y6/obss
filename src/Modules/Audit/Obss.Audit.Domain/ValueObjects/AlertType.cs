namespace Obss.Audit.Domain.ValueObjects;

public enum AlertType
{
    FailedLogin,
    MassExport,
    PermissionChange,
    SensitiveDataAccess,
    DataDeletion,
    AnomalousActivity,
    RetentionBreach
}
