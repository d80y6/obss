using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.DTOs;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Commands.ResolveTicket;

public sealed class ResolveTicketCommandHandler : IRequestHandler<ResolveTicketCommand, Result<TicketDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ResolveTicketCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TicketDto>> Handle(ResolveTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdWithDetailsAsync(request.TicketId, cancellationToken);

        if (ticket is null)
            return Result.Failure<TicketDto>(Error.NotFound(nameof(Ticket), request.TicketId));

        try
        {
            ticket.Resolve(request.Resolution);
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
