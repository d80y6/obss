using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Obss.Notifications.Application.Commands.CreateNotificationTemplate;
using Obss.Notifications.Application.Commands.SendNotification;
using Obss.Notifications.Infrastructure.Persistence;
using Obss.Notifications.Infrastructure.Persistence.Repositories;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.Notifications.Tests;

public class CommandHandlerTests : NotificationsIntegrationTests
{
    [Fact]
    public async Task SendNotificationCommand_ShouldCreateNotificationInDatabase()
    {
        using var context = CreateDbContext();
        var notificationRepository = new NotificationRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<SendNotificationCommandHandler>>();

        var handler = new SendNotificationCommandHandler(notificationRepository, unitOfWork, logger);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new SendNotificationCommand(
            tenantId,
            "Email",
            "email",
            "user@example.com",
            "Welcome",
            "Welcome to our platform!",
            "Normal",
            "Signup",
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Recipient.Should().Be("user@example.com");
        result.Value.Subject.Should().Be("Welcome");
        result.Value.Body.Should().Be("Welcome to our platform!");
        result.Value.NotificationType.Should().Be("Email");
        result.Value.Channel.Should().Be("email");
        result.Value.Status.Should().Be("Pending");
        result.Value.Priority.Should().Be("Normal");

        var saved = await notificationRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Recipient.Should().Be("user@example.com");
    }

    [Fact]
    public async Task SendNotificationCommand_ShouldDefaultPriorityToNormal()
    {
        using var context = CreateDbContext();
        var notificationRepository = new NotificationRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<SendNotificationCommandHandler>>();

        var handler = new SendNotificationCommandHandler(notificationRepository, unitOfWork, logger);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new SendNotificationCommand(
            tenantId,
            "SMS",
            "sms",
            "+1234567890",
            "Alert",
            "System alert",
            null,
            null,
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Priority.Should().Be("Normal");
    }

    [Fact]
    public async Task SendNotificationCommand_ShouldFailWithInvalidNotificationType()
    {
        using var context = CreateDbContext();
        var notificationRepository = new NotificationRepository(context);
        var unitOfWork = CreateUnitOfWork(context);
        var logger = Substitute.For<ILogger<SendNotificationCommandHandler>>();

        var handler = new SendNotificationCommandHandler(notificationRepository, unitOfWork, logger);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new SendNotificationCommand(
            tenantId,
            "InvalidType",
            "email",
            "user@example.com",
            "Test",
            "Test body",
            "Normal",
            null,
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateNotificationTemplateCommand_ShouldCreateTemplateInDatabase()
    {
        using var context = CreateDbContext();
        var templateRepository = new NotificationTemplateRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateNotificationTemplateCommandHandler(templateRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var variables = new List<string> { "UserName", "Link" };
        var command = new CreateNotificationTemplateCommand(
            tenantId,
            "WelcomeEmail",
            "Welcome email template",
            "Email",
            "Welcome {{UserName}}",
            "Hello {{UserName}}, click {{Link}} to get started.",
            variables);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("WelcomeEmail");
        result.Value.Description.Should().Be("Welcome email template");
        result.Value.NotificationType.Should().Be("Email");
        result.Value.Subject.Should().Be("Welcome {{UserName}}");
        result.Value.Body.Should().Be("Hello {{UserName}}, click {{Link}} to get started.");
        result.Value.Variables.Should().BeEquivalentTo(variables);
        result.Value.IsActive.Should().BeTrue();
        result.Value.Version.Should().Be(1);

        var saved = await templateRepository.GetByIdAsync(result.Value.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("WelcomeEmail");
    }

    [Fact]
    public async Task CreateNotificationTemplateCommand_ShouldDefaultVariablesToEmpty()
    {
        using var context = CreateDbContext();
        var templateRepository = new NotificationTemplateRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateNotificationTemplateCommandHandler(templateRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateNotificationTemplateCommand(
            tenantId,
            "SimpleAlert",
            null,
            "InApp",
            "Alert",
            "Simple alert body",
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Variables.Should().BeEmpty();
        result.Value.Description.Should().BeNull();
    }

    [Fact]
    public async Task CreateNotificationTemplateCommand_ShouldFailWithInvalidNotificationType()
    {
        using var context = CreateDbContext();
        var templateRepository = new NotificationTemplateRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateNotificationTemplateCommandHandler(templateRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateNotificationTemplateCommand(
            tenantId,
            "BadTemplate",
            null,
            "UnknownType",
            "Subject",
            "Body",
            null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
