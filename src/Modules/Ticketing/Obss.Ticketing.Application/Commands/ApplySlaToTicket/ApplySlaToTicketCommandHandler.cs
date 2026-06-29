using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.DTOs;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Commands.ApplySlaToTicket;

public sealed class ApplySlaToTicketCommandHandler : IRequestHandler<ApplySlaToTicketCommand, Result<TicketDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ISlaDefinitionRepository _slaDefinitionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApplySlaToTicketCommandHandler(
        ITicketRepository ticketRepository,
        ISlaDefinitionRepository slaDefinitionRepository,
        IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _slaDefinitionRepository = slaDefinitionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TicketDto>> Handle(ApplySlaToTicketCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdWithDetailsAsync(request.TicketId, cancellationToken);

        if (ticket is null)
            return Result.Failure<TicketDto>(Error.NotFound(nameof(Ticket), request.TicketId));

        var slaDefinition = await _slaDefinitionRepository.GetByIdAsync(request.SlaDefinitionId, cancellationToken);

        if (slaDefinition is null)
            return Result.Failure<TicketDto>(Error.NotFound(nameof(SlaDefinition), request.SlaDefinitionId));

        if (!slaDefinition.IsActive)
            return Result.Failure<TicketDto>(Error.Validation("Cannot apply an inactive SLA definition."));

        try
        {
            ticket.ApplySlaDefinition(slaDefinition);
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
