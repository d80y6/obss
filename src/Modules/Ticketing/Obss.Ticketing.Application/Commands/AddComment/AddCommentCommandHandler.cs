using Mapster;
using MediatR;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.Abstractions;
using Obss.Ticketing.Application.DTOs;
using Obss.Ticketing.Domain.Entities;

namespace Obss.Ticketing.Application.Commands.AddComment;

public sealed class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Result<TicketCommentDto>>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCommentCommandHandler(ITicketRepository ticketRepository, IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TicketCommentDto>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdWithDetailsAsync(request.TicketId, cancellationToken);

        if (ticket is null)
            return Result.Failure<TicketCommentDto>(Error.NotFound(nameof(Ticket), request.TicketId));

        var comment = ticket.AddComment(request.Content, request.IsInternal, request.AuthorId, request.AuthorName);

        await _ticketRepository.UpdateAsync(ticket, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(comment.Adapt<TicketCommentDto>());
    }
}
