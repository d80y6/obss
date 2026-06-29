using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddNote;

public sealed record AddNoteCommand(
    Guid CustomerId,
    string Content,
    string Category,
    string CreatedById) : IRequest<Result<CustomerNoteDto>>;
