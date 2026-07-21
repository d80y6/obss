using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace Obss.Provisioning.Infrastructure.Transports.Ssh;

public sealed class SshTransport : ISshTransport
{
    private readonly SshTransportConfig _config;
    private readonly ILogger<SshTransport> _logger;

    public SshTransport(SshTransportConfig config, ILogger<SshTransport> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public TransportProtocol Protocol => TransportProtocol.Ssh;
    public ITransportConfig Config => _config;

    public async Task<TransportConnectionResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var client = CreateSshClient();
            await Task.Run(() => client.Connect(), cancellationToken);

            sw.Stop();
            var info = $"SSH connected to {_config.Host}:{_config.Port} ({(client.IsConnected ? "connected" : "disconnected")})";
            client.Disconnect();

            return TransportConnectionResult.Ok(info, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "SSH connection test failed for {Host}:{Port}", _config.Host, _config.Port);
            return TransportConnectionResult.Fail(ex.Message, sw.Elapsed);
        }
    }

    public async Task<TransportResult> ExecuteCommandAsync(string command, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var client = CreateSshClient();
            await Task.Run(() => client.Connect(), cancellationToken);

            using var sshCommand = client.RunCommand(command);
            var output = sshCommand.Result;
            var error = sshCommand.Error;

            client.Disconnect();
            sw.Stop();

            if (!string.IsNullOrEmpty(error) && sshCommand.ExitStatus != 0)
            {
                return TransportResult.Fail(
                    $"SSH command failed (exit: {sshCommand.ExitStatus}): {error}",
                    sw.Elapsed, Protocol);
            }

            var result = new StringBuilder();
            if (!string.IsNullOrEmpty(output))
                result.Append(output);
            if (!string.IsNullOrEmpty(error))
                result.AppendLine().Append("[STDERR] ").Append(error);

            return TransportResult.Ok(result.ToString(), sw.Elapsed, Protocol);
        }
        catch (SshConnectionException ex)
        {
            sw.Stop();
            return TransportResult.Fail($"SSH connection error: {ex.Message}", sw.Elapsed, Protocol);
        }
        catch (SshAuthenticationException ex)
        {
            sw.Stop();
            return TransportResult.Fail($"SSH authentication error: {ex.Message}", sw.Elapsed, Protocol);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return TransportResult.Fail($"SSH command execution failed: {ex.Message}", sw.Elapsed, Protocol);
        }
    }

    public async Task UploadFileAsync(string localPath, string remotePath, CancellationToken cancellationToken = default)
    {
        using var client = CreateSshClient();
        await Task.Run(() => client.Connect(), cancellationToken);

        if (_config.UseSftp)
        {
            using var sftp = new SftpClient(client.ConnectionInfo);
            await Task.Run(() => sftp.Connect(), cancellationToken);

            await using var fileStream = File.OpenRead(localPath);
            sftp.UploadFile(fileStream, remotePath);
            sftp.Disconnect();
        }
        else
        {
            using var scp = new ScpClient(client.ConnectionInfo);
            await Task.Run(() => scp.Connect(), cancellationToken);
            scp.Upload(new FileInfo(localPath), remotePath);
            scp.Disconnect();
        }

        client.Disconnect();
    }

    public async Task<TransportResult> DownloadFileAsync(string remotePath, CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var client = CreateSshClient();
            await Task.Run(() => client.Connect(), cancellationToken);

            using var sftp = new SftpClient(client.ConnectionInfo);
            await Task.Run(() => sftp.Connect(), cancellationToken);

            await using var stream = new MemoryStream();
            sftp.DownloadFile(remotePath, stream);
            sftp.Disconnect();
            client.Disconnect();

            sw.Stop();
            var content = Encoding.UTF8.GetString(stream.ToArray());
            return TransportResult.Ok(content, sw.Elapsed, Protocol);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return TransportResult.Fail($"SFTP download failed: {ex.Message}", sw.Elapsed, Protocol);
        }
    }

    private SshClient CreateSshClient()
    {
        var connectionInfo = BuildConnectionInfo();
        var client = new SshClient(connectionInfo);
        client.ConnectionInfo.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
        return client;
    }

    private ConnectionInfo BuildConnectionInfo()
    {
        var host = _config.Host;
        var port = _config.Port > 0 ? _config.Port : 22;
        var username = _config.Username ?? "root";

        if (!string.IsNullOrEmpty(_config.PrivateKeyPath))
        {
            var keyFiles = new[] { new PrivateKeyFile(_config.PrivateKeyPath, _config.PrivateKeyPassphrase) };
            return new ConnectionInfo(host, port, username, new PrivateKeyAuthenticationMethod(username, keyFiles));
        }

        if (!string.IsNullOrEmpty(_config.Password))
        {
            return new ConnectionInfo(host, port, username,
                new PasswordAuthenticationMethod(username, _config.Password));
        }

        return new ConnectionInfo(host, port, username,
            new NoneAuthenticationMethod(username));
    }
}
