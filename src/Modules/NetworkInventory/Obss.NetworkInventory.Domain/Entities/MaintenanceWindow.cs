using Obss.SharedKernel.Domain.Common;

namespace Obss.NetworkInventory.Domain.Entities;

public class MaintenanceWindow : Entity<Guid>
{
    private readonly List<string> _affectedDeviceIds = [];

    private MaintenanceWindow() { }

    private MaintenanceWindow(
        Guid id,
        string title,
        string? titleAr,
        string? description,
        string affectedTechnology,
        DateTime startTime,
        DateTime endTime,
        string approvedBy,
        bool suppressAlarms)
        : base(id)
    {
        Title = title;
        TitleAr = titleAr;
        Description = description;
        AffectedTechnology = affectedTechnology;
        StartTime = startTime;
        EndTime = endTime;
        Status = "SCHEDULED";
        ApprovedBy = approvedBy;
        SuppressAlarms = suppressAlarms;
    }

    public string Title { get; private set; } = string.Empty;
    public string? TitleAr { get; private set; }
    public string? Description { get; private set; }
    public string AffectedTechnology { get; private set; } = string.Empty;
    public IReadOnlyList<string> AffectedDeviceIds => _affectedDeviceIds.AsReadOnly();
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public string ApprovedBy { get; private set; } = string.Empty;
    public bool SuppressAlarms { get; private set; }

    public static MaintenanceWindow Create(
        string title,
        string? titleAr,
        string? description,
        string affectedTechnology,
        DateTime startTime,
        DateTime endTime,
        string approvedBy,
        bool suppressAlarms)
    {
        return new MaintenanceWindow(
            Guid.NewGuid(),
            title,
            titleAr,
            description,
            affectedTechnology,
            startTime,
            endTime,
            approvedBy,
            suppressAlarms);
    }

    public void AddAffectedDevice(string deviceId)
    {
        if (!_affectedDeviceIds.Contains(deviceId))
        {
            _affectedDeviceIds.Add(deviceId);
        }
    }

    public void Activate()
    {
        if (Status != "SCHEDULED")
        {
            return;
        }

        Status = "ACTIVE";
    }

    public void Complete()
    {
        if (Status != "ACTIVE")
        {
            return;
        }

        Status = "COMPLETED";
    }

    public void Cancel()
    {
        if (Status == "COMPLETED")
        {
            return;
        }

        Status = "CANCELLED";
    }

    public bool IsActive()
    {
        var now = DateTime.UtcNow;
        return Status == "ACTIVE" || (Status == "SCHEDULED" && now >= StartTime && now <= EndTime);
    }

    public bool AffectsDevice(string deviceId)
    {
        return _affectedDeviceIds.Contains(deviceId) || AffectedTechnology == "ALL";
    }
}
