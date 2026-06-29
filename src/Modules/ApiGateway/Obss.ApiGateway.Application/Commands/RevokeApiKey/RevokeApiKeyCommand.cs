using MediatR;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Commands.RevokeApiKey;

public sealed record RevokeApiKeyCommand(Guid Id) : IRequest<Result>;
