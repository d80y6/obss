using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceInventory.Application.Commands.RemoveTopologyLink;

public sealed record RemoveTopologyLinkCommand(Guid ServiceTopologyId, Guid LinkId) : IRequest<Result>;
