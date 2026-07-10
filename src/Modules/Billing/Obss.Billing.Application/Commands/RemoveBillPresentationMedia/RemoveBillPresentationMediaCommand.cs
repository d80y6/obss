using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Billing.Application.Commands.RemoveBillPresentationMedia;

public sealed record RemoveBillPresentationMediaCommand(
    Guid BillingAccountId,
    Guid MediaId) : IRequest<Result>;
