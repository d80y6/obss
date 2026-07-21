using Obss.Provisioning.Infrastructure.Transports.Abstractions;

namespace Obss.Provisioning.Infrastructure.Transports.Ssh;

public interface ISshTransport : INetworkTransport
{
    Task<TransportResult> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default);

    Task UploadFileAsync(string localPath, string remotePath, CancellationToken cancellationToken = default);

    Task<TransportResult> DownloadFileAsync(string remotePath, CancellationToken cancellationToken = default);
}
