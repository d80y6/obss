namespace Obss.ServiceCatalog.Domain.ValueObjects;

public sealed record FtthServiceSpec
{
    public string Technology { get; init; } = "FTTH";
    public string Segment { get; init; } = string.Empty;
    public int DownloadSpeedMbps { get; init; }
    public int UploadSpeedMbps { get; init; }
    public string? OntModel { get; init; }
    public bool IncludeVoice { get; init; }
    public bool IncludeTv { get; init; }
    public bool IncludeStaticIp { get; init; }
    public int PublicIpCount { get; init; }
    public string? ServiceProfile { get; init; }
    public string? LineProfile { get; init; }
    public string? VlanId { get; init; }
    public bool RequiresInstallation { get; init; } = true;
    public string? InstallationType { get; init; }
    public string? SlaLevel { get; init; }
}
