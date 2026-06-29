using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.DTOs;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Commands.CloseTicket;

public sealed class CloseTicketCommandHandler : IRequestHandler<CloseTicketCommand, Result<TicketDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CloseTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TicketDto>> Handle(CloseTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdWithDetailsAsync(request.TicketId, cancellationToken);

        if (ticket is null)
            return Result.Failure<TicketDto>(Error.NotFound(nameof(Ticket), request.TicketId));

        try
        {
            ticket.Close();
        }
        catch (Exception ex)
        {
            return Result.Failure<TicketDto>(Error.Validation(ex.Message));
        }

        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(ticket.Adapt<TicketDto>());
    }
}
