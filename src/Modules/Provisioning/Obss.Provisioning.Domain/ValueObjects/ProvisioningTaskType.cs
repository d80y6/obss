namespace Obss.Provisioning.Domain.ValueObjects;

public enum ProvisioningTaskType
{
    NetworkConfig,
    ResourceAllocation,
    DNSSetup,
    AccountSetup,
    EmailNotification,
    PhysicalInstall,
    Custom
}
