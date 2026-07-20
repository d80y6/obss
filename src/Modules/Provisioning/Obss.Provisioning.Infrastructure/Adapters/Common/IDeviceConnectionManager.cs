namespace Obss.Provisioning.Infrastructure.Adapters.Common;

public interface IDeviceConnectionManager
{
    Task<bool> TestConnectionAsync(string endpoint, int timeoutSeconds, CancellationToken cancellationToken = default);

    Task<bool> TestCredentialsAsync(string endpoint, string username, string password, CancellationToken cancellationToken = default);
}
