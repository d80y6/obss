using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.RemoveIdentityDocument;

public sealed record RemoveIdentityDocumentCommand(Guid IndividualId, Guid DocumentId) : IRequest<Result>;
