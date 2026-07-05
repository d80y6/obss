using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.AddIdentityDocument;

public sealed record AddIdentityDocumentCommand(
    Guid IndividualId,
    string DocumentType,
    string DocumentNumber,
    string? IssuingAuthority,
    string? IssuingCountry,
    DateTime? IssuedDate,
    DateTime? ExpiryDate) : IRequest<Result>;
