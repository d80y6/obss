using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.ReleaseNumber;

public sealed record ReleaseNumberCommand(Guid NumberId) : IRequest<Result>;
