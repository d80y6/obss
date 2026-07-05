using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.RemoveAgreementRef;

public sealed record RemoveAgreementRefCommand(Guid CustomerId, Guid AgreementId) : IRequest<Result>;
