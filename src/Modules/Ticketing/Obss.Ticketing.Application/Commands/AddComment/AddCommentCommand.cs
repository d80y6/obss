using MediatR;
using Obss.SharedKernel.Application.Contracts;
using Obss.Ticketing.Application.DTOs;

namespace Obss.Ticketing.Application.Commands.AddComment;

public sealed record AddCommentCommand(
    Guid TicketId,
    string Content,
    bool IsInternal,
    string AuthorId,
    string AuthorName) : IRequest<Result<TicketCommentDto>>;
