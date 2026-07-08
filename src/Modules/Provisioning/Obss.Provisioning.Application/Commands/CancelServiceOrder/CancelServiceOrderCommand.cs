using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Provisioning.Application.Commands.CancelServiceOrder;

public sealed record CancelServiceOrderCommand(Guid Id, string Reason) : IRequest<Result>;
