using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Obss.Provisioning.Infrastructure.Transports.Abstractions;
using Renci.SshNet;

namespace Obss.Provisioning.Infrastructure.Transports.Netconf;

public sealed class NetconfTransport : INetconfTransport
{
    private static readonly XNamespace NetconfNs = "urn:ietf:params:xml:ns:netconf:base:1.0";
    private readonly NetconfTransportConfig _config;

    public NetconfTransport(NetconfTransportConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public TransportProtocol Protocol => TransportProtocol.Netconf;
    public ITransportConfig Config => _config;

    public async Task<TransportConnectionResult> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var session = await OpenSessionAsync(cancellationToken);
            var capabilities = session.ServerCapabilities;
            sw.Stop();

            var info = $"NETCONF connected to {_config.Host}:{_config.Port} with {capabilities.Count} capabilities";
            return TransportConnectionResult.Ok(info, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return TransportConnectionResult.Fail($"NETCONF connection failed: {ex.Message}", sw.Elapsed);
        }
    }

    public async Task<TransportResult> GetConfigAsync(string source = "running", CancellationToken cancellationToken = default)
    {
        return await ExecuteRpcInternalAsync(
            new XElement(NetconfNs + "get-config",
                new XElement(NetconfNs + "source",
                    new XElement(NetconfNs + source))),
            cancellationToken);
    }

    public async Task<TransportResult> EditConfigAsync(string xmlConfig, string target = "running", CancellationToken cancellationToken = default)
    {
        var configElement = XElement.Parse(xmlConfig);
        return await ExecuteRpcInternalAsync(
            new XElement(NetconfNs + "edit-config",
                new XElement(NetconfNs + "target",
                    new XElement(NetconfNs + target)),
                new XElement(NetconfNs + "config", configElement)),
            cancellationToken);
    }

    public async Task<TransportResult> CopyConfigAsync(string source, string target, CancellationToken cancellationToken = default)
    {
        return await ExecuteRpcInternalAsync(
            new XElement(NetconfNs + "copy-config",
                new XElement(NetconfNs + "source",
                    new XElement(NetconfNs + source)),
                new XElement(NetconfNs + "target",
                    new XElement(NetconfNs + target))),
            cancellationToken);
    }

    public async Task<TransportResult> ExecuteRpcAsync(string xmlRpc, CancellationToken cancellationToken = default)
    {
        var rpcElement = XElement.Parse(xmlRpc);
        return await ExecuteRpcInternalAsync(rpcElement, cancellationToken);
    }

    public async Task<TransportResult> LockAsync(string target = "running", CancellationToken cancellationToken = default)
    {
        return await ExecuteRpcInternalAsync(
            new XElement(NetconfNs + "lock",
                new XElement(NetconfNs + "target",
                    new XElement(NetconfNs + target))),
            cancellationToken);
    }

    public async Task<TransportResult> UnlockAsync(string target = "running", CancellationToken cancellationToken = default)
    {
        return await ExecuteRpcInternalAsync(
            new XElement(NetconfNs + "unlock",
                new XElement(NetconfNs + "target",
                    new XElement(NetconfNs + target))),
            cancellationToken);
    }

    public async Task<TransportResult> GetSchemaAsync(string moduleName, CancellationToken cancellationToken = default)
    {
        return await ExecuteRpcInternalAsync(
            new XElement(NetconfNs + "get-schema",
                new XElement("identifier", moduleName),
                new XElement("version", "1.0")),
            cancellationToken);
    }

    private async Task<TransportResult> ExecuteRpcInternalAsync(XElement rpcContent, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            using var session = await OpenSessionAsync(cancellationToken);

            var messageId = Guid.NewGuid().ToString("N");
            var rpc = new XElement(NetconfNs + "rpc",
                new XAttribute("message-id", messageId),
                rpcContent);

            var rpcXml = rpc.ToString(SaveOptions.DisableFormatting);
            var rpcBytes = Encoding.UTF8.GetBytes(rpcXml);
            var rpcFrame = FrameMessage(rpcBytes);

            session.SendData(rpcFrame);

            var responseData = new byte[_config.MaxMessageSize];
            var received = session.ReadData(responseData);
            session.Disconnect();

            sw.Stop();
            var response = Encoding.UTF8.GetString(responseData, 0, received);
            var cleaned = CleanNetconfResponse(response);

            if (IsErrorResponse(cleaned))
            {
                return TransportResult.Fail(
                    $"NETCONF RPC error: {ExtractError(cleaned)}",
                    sw.Elapsed, Protocol);
            }

            return TransportResult.Ok(cleaned, sw.Elapsed, Protocol);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return TransportResult.Fail($"NETCONF operation failed: {ex.Message}", sw.Elapsed, Protocol);
        }
    }

    private async Task<NetconfSession> OpenSessionAsync(CancellationToken cancellationToken)
    {
            var session = new NetconfSession(_config);
            await session.ConnectAsync(cancellationToken);
        return session;
    }

    private static byte[] FrameMessage(byte[] message)
    {
        var framing = $"</encoding=\"base64\"/>\n{Convert.ToBase64String(message)}||>";
        var header = $"#4\n{framing.Length}\n";
        return Encoding.UTF8.GetBytes(header + framing);
    }

    private static string CleanNetconfResponse(string raw)
    {
        var lines = raw.Split('\n')
            .SkipWhile(l => l.StartsWith('#'))
            .SkipWhile(l => l.StartsWith('<'))
            .Where(l => l.StartsWith('<'));
        return string.Join(Environment.NewLine, lines);
    }

    private static bool IsErrorResponse(string xml)
    {
        return xml.Contains("<rpc-error>");
    }

    private static string ExtractError(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            var error = doc.Descendants(NetconfNs + "rpc-error")
                .FirstOrDefault()?
                .Descendants(NetconfNs + "error-message")
                .FirstOrDefault()?
                .Value;
            return error ?? "Unknown NETCONF error";
        }
        catch
        {
            return "Failed to parse NETCONF error response";
        }
    }

    private sealed class NetconfSession : IDisposable
    {
        private readonly SshClient _sshClient;

        public NetconfSession(NetconfTransportConfig config)
        {
            var connectionInfo = new ConnectionInfo(
                config.Host,
                config.Port > 0 ? config.Port : 830,
                config.Username ?? "root",
                !string.IsNullOrEmpty(config.Password)
                    ? new PasswordAuthenticationMethod(config.Username ?? "root", config.Password)
                    : new NoneAuthenticationMethod(config.Username ?? "root"));

            connectionInfo.Timeout = TimeSpan.FromSeconds(config.TimeoutSeconds);
            _sshClient = new SshClient(connectionInfo);
        }

        public IList<string> ServerCapabilities { get; private set; } = new List<string>();

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() => _sshClient.Connect(), cancellationToken);

            var shellStream = _sshClient.CreateShellStream("netconf", 80, 24, 800, 600, 65536);

            var hello = await ReceiveHelloAsync(shellStream, cancellationToken);
            ServerCapabilities = ParseCapabilities(hello);

            await SendHelloAsync(shellStream, cancellationToken);
        }

        public void SendData(byte[] data)
        {
            var shellStream = _sshClient.CreateShellStream("netconf", 80, 24, 800, 600, 65536);
            shellStream.Write(data);
        }

        public int ReadData(byte[] buffer)
        {
            var shellStream = _sshClient.CreateShellStream("netconf", 80, 24, 800, 600, 65536);
            return shellStream.Read(buffer, 0, buffer.Length);
        }

        public void Disconnect()
        {
            if (_sshClient.IsConnected)
                _sshClient.Disconnect();
        }

        public void Dispose()
        {
            _sshClient?.Dispose();
        }

        private static async Task<string> ReceiveHelloAsync(ShellStream stream, CancellationToken cancellationToken)
        {
            var buffer = new byte[65535];
            var received = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
            return Encoding.UTF8.GetString(buffer, 0, received);
        }

        private static async Task SendHelloAsync(ShellStream stream, CancellationToken cancellationToken)
        {
            var hello = new XElement(NetconfNs + "hello",
                new XElement(NetconfNs + "capabilities",
                    new XElement(NetconfNs + "capability", "urn:ietf:params:netconf:base:1.0"),
                    new XElement(NetconfNs + "capability", "urn:ietf:params:netconf:base:1.1"),
                    new XElement(NetconfNs + "capability", "urn:ietf:params:netconf:capability:writable-running:1.0"),
                    new XElement(NetconfNs + "capability", "urn:ietf:params:netconf:capability:candidate:1.0"),
                    new XElement(NetconfNs + "capability", "urn:ietf:params:netconf:capability:rollback-on-error:1.0")));

            var helloXml = hello.ToString(SaveOptions.DisableFormatting);
            var helloBytes = Encoding.UTF8.GetBytes(helloXml);
            var frame = FrameMessage(helloBytes);

            await stream.WriteAsync(frame, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        private static List<string> ParseCapabilities(string helloXml)
        {
            try
            {
                var doc = XDocument.Parse(helloXml);
                return doc.Descendants(NetconfNs + "capability")
                    .Select(e => e.Value)
                    .ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
