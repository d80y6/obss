using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.DTOs;
using Obss.Ticketing.Domain.Entities;
using Obss.Ticketing.Domain.ValueObjects;

namespace Obss.Ticketing.Application.Commands.CreateTicket;

public sealed class CreateTicketCommandHandler : IRequestHandler<CreateTicketCommand, Result<TicketDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TicketDto>> Handle(CreateTicketCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<TicketPriority>(request.Priority, true, out var priority))
            return Result.Failure<TicketDto>(Error.Validation($"Invalid priority: {request.Priority}"));

        if (!Enum.TryParse<TicketCategory>(request.Category, true, out var category))
            return Result.Failure<TicketDto>(Error.Validation($"Invalid category: {request.Category}"));

        if (!Enum.TryParse<TicketSource>(request.Source, true, out var source))
            return Result.Failure<TicketDto>(Error.Validation($"Invalid source: {request.Source}"));

        var ticketNumber = await _ticketRepository.GetNextTicketNumberAsync(cancellationToken);

        var ticket = Ticket.Create(
            request.TenantId,
            ticketNumber,
            request.CustomerId,
            request.CustomerName,
            request.Subject,
            request.Description,
            priority,
            category,
            source);

        await _ticketRepository.AddAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ticket.Adapt<TicketDto>());
    }
}
