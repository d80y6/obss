using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.PurgeOldEntries;

public sealed record PurgeOldEntriesCommand : IRequest<Result<int>>;
