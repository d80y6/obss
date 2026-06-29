using System.Text.Json;
using Obss.Notifications.Domain.Exceptions;
using Obss.Notifications.Domain.ValueObjects;
using Obss.SharedKernel.Domain.Common;

namespace Obss.Notifications.Domain.Entities;

public class NotificationTemplate : AggregateRoot<Guid>
{
    private const string PlaceholderPattern = @"\{\{(.+?)\}\}";
    private static readonly System.Text.RegularExpressions.Regex PlaceholderRegex =
        new(PlaceholderPattern, System.Text.RegularExpressions.RegexOptions.Compiled);

    private NotificationTemplate() { }

    private NotificationTemplate(
        Guid id,
        string tenantId,
        string name,
        string? description,
        NotificationType notificationType,
        string subject,
        string body,
        List<string> variables)
        : base(id)
    {
        TenantId = tenantId;
        Name = name;
        Description = description;
        NotificationType = notificationType;
        Subject = subject;
        Body = body;
        Variables = JsonSerializer.Serialize(variables);
        IsActive = true;
        Version = 1;
        CreatedAt = DateTime.UtcNow;
    }

    public string TenantId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public string Variables { get; private set; } = "[]";
    public bool IsActive { get; private set; }
    public int Version { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public static NotificationTemplate Create(
        string tenantId,
        string name,
        string? description,
        NotificationType notificationType,
        string subject,
        string body,
        List<string>? variables = null)
    {
        return new NotificationTemplate(
            Guid.NewGuid(),
            tenantId,
            name,
            description,
            notificationType,
            subject,
            body,
            variables ?? []);
    }

    public void Update(
        string? name,
        string? description,
        string? subject,
        string? body,
        List<string>? variables)
    {
        if (name is not null)
            Name = name;

        if (description is not null)
            Description = description;

        if (subject is not null)
            Subject = subject;

        if (body is not null)
            Body = body;

        if (variables is not null)
            Variables = JsonSerializer.Serialize(variables);

        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (IsActive)
            return;

        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public NotificationTemplate CreateNewVersion()
    {
        var newVersion = new NotificationTemplate(
            Guid.NewGuid(),
            TenantId,
            Name,
            Description,
            NotificationType,
            Subject,
            Body,
            GetVariableList())
        {
            Version = Version + 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        return newVersion;
    }

    public (string subject, string body) Render(Dictionary<string, string> variables)
    {
        var variableNames = GetVariableList();
        var missing = variableNames.Where(v => !variables.ContainsKey(v)).ToList();

        if (missing.Count > 0)
        {
            throw new TemplateRenderException(
                $"Missing required variables: {string.Join(", ", missing)}");
        }

        var renderedSubject = ReplacePlaceholders(Subject, variables);
        var renderedBody = ReplacePlaceholders(Body, variables);

        return (renderedSubject, renderedBody);
    }

    public List<string> GetVariableList()
    {
        return JsonSerializer.Deserialize<List<string>>(Variables) ?? [];
    }

    private static string ReplacePlaceholders(string template, Dictionary<string, string> variables)
    {
        return PlaceholderRegex.Replace(template, match =>
        {
            var key = match.Groups[1].Value.Trim();
            return variables.TryGetValue(key, out var value) ? value : match.Value;
        });
    }
}
