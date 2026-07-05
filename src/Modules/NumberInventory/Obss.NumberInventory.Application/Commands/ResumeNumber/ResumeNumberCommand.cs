using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.ResumeNumber;

public sealed record ResumeNumberCommand(Guid NumberId) : IRequest<Result>;
