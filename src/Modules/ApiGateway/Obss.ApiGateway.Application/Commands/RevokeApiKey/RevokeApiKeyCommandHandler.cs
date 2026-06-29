using MediatR;
using Obss.ApiGateway.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ApiGateway.Application.Commands.RevokeApiKey;

public sealed class RevokeApiKeyCommandHandler : IRequestHandler<RevokeApiKeyCommand, Result>
{
    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RevokeApiKeyCommandHandler(
        IApiKeyRepository apiKeyRepository,
        IUnitOfWork unitOfWork)
    {
        _apiKeyRepository = apiKeyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RevokeApiKeyCommand request, CancellationToken cancellationToken)
    {
        var apiKey = await _apiKeyRepository.GetByIdAsync(request.Id, cancellationToken);
        if (apiKey is null)
            return Result.Failure(Error.NotFound("ApiKey", request.Id));

        apiKey.Revoke();
        await _apiKeyRepository.UpdateAsync(apiKey, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
