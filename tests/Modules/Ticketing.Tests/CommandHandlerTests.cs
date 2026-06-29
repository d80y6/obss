using Xunit;
using FluentAssertions;
using NSubstitute;
using Obss.SharedKernel.Application.Abstractions;
using Obss.Ticketing.Application.Commands.AssignTicket;
using Obss.Ticketing.Application.Commands.CreateTicket;
using Obss.Ticketing.Application.Commands.ResolveTicket;
using Obss.Ticketing.Domain.Entities;
using Obss.Ticketing.Domain.ValueObjects;
using Obss.Ticketing.Infrastructure.Persistence;
using Obss.Ticketing.Infrastructure.Persistence.Repositories;

namespace Obss.Ticketing.Tests;

public class CommandHandlerTests : TicketingIntegrationTests
{
    [Fact]
    public async Task CreateTicketCommand_ShouldCreateTicketInDatabase()
    {
        using var context = CreateDbContext();
        var ticketRepository = new TicketRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateTicketCommandHandler(ticketRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");
        var command = new CreateTicketCommand(
            tenantId,
            Guid.NewGuid(),
            "John Doe",
            "Unable to access portal",
            "User reports 401 error when logging in",
            "High",
            "Technical",
            "Portal");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Subject.Should().Be("Unable to access portal");
        result.Value.Description.Should().Be("User reports 401 error when logging in");
        result.Value.Priority.Should().Be("High");
        result.Value.Category.Should().Be("Technical");
        result.Value.Source.Should().Be("Portal");
        result.Value.CustomerName.Should().Be("John Doe");
        result.Value.Status.Should().Be("Open");
        result.Value.TicketNumber.Should().NotBeNullOrEmpty();

        var saved = await ticketRepository.GetByTicketNumberAsync(result.Value.TicketNumber);
        saved.Should().NotBeNull();
        saved!.Subject.Should().Be("Unable to access portal");
        saved.CustomerName.Should().Be("John Doe");
    }

    [Fact]
    public async Task CreateTicketCommand_ShouldFailWithInvalidPriority()
    {
        using var context = CreateDbContext();
        var ticketRepository = new TicketRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateTicketCommandHandler(ticketRepository, unitOfWork);

        var command = new CreateTicketCommand(
            Guid.NewGuid().ToString("N"),
            Guid.NewGuid(),
            "Jane Doe",
            "Test subject",
            "Test description",
            "Urgent",
            "Technical",
            "Portal");

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task AssignTicketCommand_ShouldAssignTicketToUser()
    {
        using var context = CreateDbContext();
        var ticketRepository = new TicketRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var ticket = Ticket.Create(
            tenantId,
            "TKT-TEST-001",
            Guid.NewGuid(),
            "Alice Smith",
            "Network issue",
            "Intermittent connectivity loss",
            TicketPriority.High,
            TicketCategory.Technical,
            TicketSource.Phone);

        await ticketRepository.AddAsync(ticket);
        await context.SaveChangesAsync();

        var handler = new AssignTicketCommandHandler(ticketRepository, unitOfWork);
        var userId = Guid.NewGuid().ToString("N");

        var result = await handler.Handle(
            new AssignTicketCommand(ticket.Id, userId, "Support Team"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AssignedTo.Should().Be(userId);
        result.Value.AssignedGroup.Should().Be("Support Team");
        result.Value.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task AssignTicketCommand_ShouldFailForNonExistentTicket()
    {
        using var context = CreateDbContext();
        var ticketRepository = new TicketRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new AssignTicketCommandHandler(ticketRepository, unitOfWork);

        var result = await handler.Handle(
            new AssignTicketCommand(Guid.NewGuid(), Guid.NewGuid().ToString("N"), null),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ResolveTicketCommand_ShouldResolveTicket()
    {
        using var context = CreateDbContext();
        var ticketRepository = new TicketRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var tenantId = Guid.NewGuid().ToString("N");
        var ticket = Ticket.Create(
            tenantId,
            "TKT-TEST-002",
            Guid.NewGuid(),
            "Bob Jones",
            "Password reset",
            "User cannot reset password via self-service",
            TicketPriority.Medium,
            TicketCategory.Account,
            TicketSource.Email);

        await ticketRepository.AddAsync(ticket);
        await context.SaveChangesAsync();

        var handler = new ResolveTicketCommandHandler(ticketRepository, unitOfWork);

        var result = await handler.Handle(
            new ResolveTicketCommand(ticket.Id, "Password was reset manually. Issue resolved."),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Resolution.Should().Be("Password was reset manually. Issue resolved.");
        result.Value.Status.Should().Be("Resolved");
    }

    [Fact]
    public async Task ResolveTicketCommand_ShouldFailForNonExistentTicket()
    {
        using var context = CreateDbContext();
        var ticketRepository = new TicketRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new ResolveTicketCommandHandler(ticketRepository, unitOfWork);

        var result = await handler.Handle(
            new ResolveTicketCommand(Guid.NewGuid(), "Resolved"),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateTicketCommand_ShouldGenerateSequentialTicketNumbers()
    {
        using var context = CreateDbContext();
        var ticketRepository = new TicketRepository(context);
        var unitOfWork = CreateUnitOfWork(context);

        var handler = new CreateTicketCommandHandler(ticketRepository, unitOfWork);

        var tenantId = Guid.NewGuid().ToString("N");

        var command1 = new CreateTicketCommand(
            tenantId, Guid.NewGuid(), "User A", "Issue 1", "Desc 1",
            "Low", "Billing", "Portal");

        var command2 = new CreateTicketCommand(
            tenantId, Guid.NewGuid(), "User B", "Issue 2", "Desc 2",
            "Low", "Billing", "Portal");

        var result1 = await handler.Handle(command1, CancellationToken.None);
        var result2 = await handler.Handle(command2, CancellationToken.None);

        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        result2.Value.TicketNumber.Should().NotBe(result1.Value.TicketNumber);
    }
}
