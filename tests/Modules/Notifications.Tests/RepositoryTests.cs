using Xunit;
using FluentAssertions;
using Obss.Notifications.Domain.Entities;
using Obss.Notifications.Domain.ValueObjects;
using Obss.Notifications.Infrastructure.Persistence.Repositories;

namespace Obss.Notifications.Tests;

public class RepositoryTests : NotificationsIntegrationTests
{
    [Fact]
    public async Task CanAddAndRetrieveNotification()
    {
        using var context = CreateDbContext();
        var repository = new NotificationRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var notification = Notification.Create(
            tenantId,
            NotificationType.Email,
            "email",
            "recipient@example.com",
            "Test Subject",
            "Test Body",
            NotificationPriority.High,
            "Order",
            Guid.NewGuid());

        await repository.AddAsync(notification);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(notification.Id);

        retrieved.Should().NotBeNull();
        retrieved!.TenantId.Should().Be(tenantId);
        retrieved.Recipient.Should().Be("recipient@example.com");
        retrieved.Subject.Should().Be("Test Subject");
        retrieved.Body.Should().Be("Test Body");
        retrieved.NotificationType.Should().Be(NotificationType.Email);
        retrieved.Channel.Should().Be("email");
        retrieved.Status.Should().Be(NotificationStatus.Pending);
        retrieved.Priority.Should().Be(NotificationPriority.High);
        retrieved.ReferenceType.Should().Be("Order");
        retrieved.ReferenceId.Should().NotBeNull();
    }

    [Fact]
    public async Task CanQueryNotificationsByTenant()
    {
        var tenantId1 = Guid.NewGuid().ToString("N");
        var tenantId2 = Guid.NewGuid().ToString("N");

        using (var context = CreateDbContext())
        {
            var repo = new NotificationRepository(context);

            var notification1 = Notification.Create(tenantId1, NotificationType.Email, "email", "user1@example.com", "S1", "B1", NotificationPriority.Normal);
            var notification2 = Notification.Create(tenantId1, NotificationType.SMS, "sms", "+111111", "S2", "B2", NotificationPriority.High);
            var notification3 = Notification.Create(tenantId2, NotificationType.Email, "email", "user3@example.com", "S3", "B3", NotificationPriority.Low);

            await repo.AddAsync(notification1);
            await repo.AddAsync(notification2);
            await repo.AddAsync(notification3);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new NotificationRepository(context);
            var tenant1Notifications = await repo.GetFilteredAsync(tenantId1, null, null, null, null, null, 1, 10);

            tenant1Notifications.Should().HaveCount(2);
            tenant1Notifications.Should().Contain(n => n.Recipient == "user1@example.com");
            tenant1Notifications.Should().Contain(n => n.Recipient == "+111111");
        }
    }

    [Fact]
    public async Task CanFilterNotificationsByStatus()
    {
        using var context = CreateDbContext();
        var repo = new NotificationRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var pendingNotification = Notification.Create(tenantId, NotificationType.Email, "email", "pending@example.com", "Pending", "Body", NotificationPriority.Normal);
        await repo.AddAsync(pendingNotification);
        await context.SaveChangesAsync();

        var sentNotification = Notification.Create(tenantId, NotificationType.SMS, "sms", "sent@example.com", "Sent", "Body", NotificationPriority.Normal);
        sentNotification.MarkSent();
        await repo.AddAsync(sentNotification);
        await context.SaveChangesAsync();

        var deliveredNotification = Notification.Create(tenantId, NotificationType.InApp, "inapp", "delivered@example.com", "Delivered", "Body", NotificationPriority.Normal);
        deliveredNotification.MarkSent();
        deliveredNotification.MarkDelivered();
        await repo.AddAsync(deliveredNotification);
        await context.SaveChangesAsync();

        var pendingResults = await repo.GetFilteredAsync(null, null, null, "Pending", null, null, 1, 10);
        pendingResults.Should().Contain(n => n.Recipient == "pending@example.com");
        pendingResults.Should().NotContain(n => n.Recipient == "sent@example.com");
        pendingResults.Should().NotContain(n => n.Recipient == "delivered@example.com");

        var sentResults = await repo.GetFilteredAsync(null, null, null, "Sent", null, null, 1, 10);
        sentResults.Should().Contain(n => n.Recipient == "sent@example.com");
    }

    [Fact]
    public async Task CanGetPendingNotifications()
    {
        using var context = CreateDbContext();
        var repo = new NotificationRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var pending1 = Notification.Create(tenantId, NotificationType.Email, "email", "p1@example.com", "P1", "B1", NotificationPriority.Normal);
        var pending2 = Notification.Create(tenantId, NotificationType.SMS, "sms", "p2@example.com", "P2", "B2", NotificationPriority.Normal);
        var sent = Notification.Create(tenantId, NotificationType.Email, "email", "sent@example.com", "Sent", "B3", NotificationPriority.Normal);
        sent.MarkSent();

        await repo.AddAsync(pending1);
        await repo.AddAsync(pending2);
        await repo.AddAsync(sent);
        await context.SaveChangesAsync();

        var pendingNotifications = await repo.GetPendingAsync(10);

        pendingNotifications.Should().HaveCount(2);
        pendingNotifications.Should().Contain(n => n.Recipient == "p1@example.com");
        pendingNotifications.Should().Contain(n => n.Recipient == "p2@example.com");
        pendingNotifications.Should().NotContain(n => n.Recipient == "sent@example.com");
    }

    [Fact]
    public async Task CanFilterNotificationsByDateRange()
    {
        using var context = CreateDbContext();
        var repo = new NotificationRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var early = Notification.Create(tenantId, NotificationType.Email, "email", "early@example.com", "Early", "Body", NotificationPriority.Normal);
        var late = Notification.Create(tenantId, NotificationType.Email, "email", "late@example.com", "Late", "Body", NotificationPriority.Normal);

        var earlyDate = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc);
        var lateDate = new DateTime(2025, 1, 1, 10, 1, 0, DateTimeKind.Utc);
        typeof(Notification).GetProperty(nameof(Notification.CreatedAt))!
            .GetSetMethod(true)!.Invoke(early, [earlyDate]);
        typeof(Notification).GetProperty(nameof(Notification.CreatedAt))!
            .GetSetMethod(true)!.Invoke(late, [lateDate]);

        await repo.AddAsync(early);
        await repo.AddAsync(late);
        await context.SaveChangesAsync();

        var earlyResults = await repo.GetFilteredAsync(null, null, null, null, null, earlyDate, 1, 10);
        earlyResults.Should().Contain(n => n.Recipient == "early@example.com");

        var lateResults = await repo.GetFilteredAsync(null, null, null, null, lateDate, null, 1, 10);
        lateResults.Should().Contain(n => n.Recipient == "late@example.com");
    }

    [Fact]
    public async Task CanAddAndRetrieveNotificationTemplate()
    {
        using var context = CreateDbContext();
        var repository = new NotificationTemplateRepository(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var variables = new List<string> { "UserName", "OrderId" };
        var template = NotificationTemplate.Create(
            tenantId,
            "OrderConfirmation",
            "Sent when an order is confirmed",
            NotificationType.Email,
            "Order #{{OrderId}} Confirmed",
            "Dear {{UserName}}, your order {{OrderId}} has been confirmed.",
            variables);

        await repository.AddAsync(template);
        await context.SaveChangesAsync();

        var retrieved = await repository.GetByIdAsync(template.Id);

        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("OrderConfirmation");
        retrieved.Description.Should().Be("Sent when an order is confirmed");
        retrieved.NotificationType.Should().Be(NotificationType.Email);
        retrieved.Subject.Should().Be("Order #{{OrderId}} Confirmed");
        retrieved.Body.Should().Be("Dear {{UserName}}, your order {{OrderId}} has been confirmed.");
        retrieved.GetVariableList().Should().BeEquivalentTo(variables);
        retrieved.IsActive.Should().BeTrue();
        retrieved.Version.Should().Be(1);
    }

    [Fact]
    public async Task CanFilterTemplatesByActiveStatus()
    {
        using var context = CreateDbContext();
        var repository = new NotificationTemplateRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var activeTemplate = NotificationTemplate.Create(tenantId, "ActiveTemp", "Active", NotificationType.Email, "Subj", "Body", []);
        var inactiveTemplate = NotificationTemplate.Create(tenantId, "InactiveTemp", "Inactive", NotificationType.SMS, "Subj", "Body", []);
        inactiveTemplate.Deactivate();

        await repository.AddAsync(activeTemplate);
        await repository.AddAsync(inactiveTemplate);
        await context.SaveChangesAsync();

        var activeTemplates = await repository.GetFilteredAsync(null, null, true);

        activeTemplates.Should().Contain(t => t.Name == "ActiveTemp");
        activeTemplates.Should().NotContain(t => t.Name == "InactiveTemp");

        var inactiveTemplates = await repository.GetFilteredAsync(null, null, false);
        inactiveTemplates.Should().Contain(t => t.Name == "InactiveTemp");
        inactiveTemplates.Should().NotContain(t => t.Name == "ActiveTemp");
    }

    [Fact]
    public async Task CanFilterTemplatesByTenant()
    {
        var tenantId1 = Guid.NewGuid().ToString("N");
        var tenantId2 = Guid.NewGuid().ToString("N");

        using (var context = CreateDbContext())
        {
            var repo = new NotificationTemplateRepository(context);

            var t1 = NotificationTemplate.Create(tenantId1, "Tenant1Temp", null, NotificationType.Email, "S", "B", []);
            var t2 = NotificationTemplate.Create(tenantId2, "Tenant2Temp", null, NotificationType.SMS, "S", "B", []);

            await repo.AddAsync(t1);
            await repo.AddAsync(t2);
            await context.SaveChangesAsync();
        }

        using (var context = CreateDbContext())
        {
            var repo = new NotificationTemplateRepository(context);
            var tenant1Templates = await repo.GetFilteredAsync(tenantId1, null, null);

            tenant1Templates.Should().HaveCount(1);
            tenant1Templates.Should().Contain(t => t.Name == "Tenant1Temp");
            tenant1Templates.Should().NotContain(t => t.Name == "Tenant2Temp");
        }
    }

    [Fact]
    public async Task CanFilterTemplatesByNotificationType()
    {
        using var context = CreateDbContext();
        var repo = new NotificationTemplateRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var emailTemplate = NotificationTemplate.Create(tenantId, "EmailTemp", null, NotificationType.Email, "S", "B", []);
        var smsTemplate = NotificationTemplate.Create(tenantId, "SmsTemp", null, NotificationType.SMS, "S", "B", []);

        await repo.AddAsync(emailTemplate);
        await repo.AddAsync(smsTemplate);
        await context.SaveChangesAsync();

        var emailTemplates = await repo.GetFilteredAsync(null, "Email", null);

        emailTemplates.Should().HaveCount(1);
        emailTemplates.Should().Contain(t => t.Name == "EmailTemp");
        emailTemplates.Should().NotContain(t => t.Name == "SmsTemp");
    }

    [Fact]
    public async Task CanCreateNewTemplateVersion()
    {
        using var context = CreateDbContext();
        var repository = new NotificationTemplateRepository(context);
        var tenantId = Guid.NewGuid().ToString("N");

        var template = NotificationTemplate.Create(
            tenantId,
            "VersionedTemplate",
            "Original",
            NotificationType.Email,
            "Subject v1",
            "Body v1",
            ["Var1"]);

        await repository.AddAsync(template);
        await context.SaveChangesAsync();

        var newVersion = template.CreateNewVersion();
        await repository.AddAsync(newVersion);
        await context.SaveChangesAsync();

        template.IsActive.Should().BeFalse();
        template.Version.Should().Be(1);

        newVersion.IsActive.Should().BeTrue();
        newVersion.Version.Should().Be(2);
        newVersion.Name.Should().Be("VersionedTemplate");
        newVersion.Subject.Should().Be("Subject v1");
    }
}
