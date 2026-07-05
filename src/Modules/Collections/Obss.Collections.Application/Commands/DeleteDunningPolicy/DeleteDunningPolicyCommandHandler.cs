using MediatR;
using Microsoft.Extensions.Logging;
using Obss.Collections.Application.Abstractions;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Collections.Application.Commands.DeleteDunningPolicy;

public sealed class DeleteDunningPolicyCommandHandler : IRequestHandler<DeleteDunningPolicyCommand, Result>
{
    private readonly IDunningPolicyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteDunningPolicyCommandHandler> _logger;

    public DeleteDunningPolicyCommandHandler(
        IDunningPolicyRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteDunningPolicyCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteDunningPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (policy is null)
            return Result.Failure(Error.NotFound("DunningPolicy", request.Id));

        if (policy.IsActive)
            return Result.Failure(Error.Conflict("Cannot delete an active dunning policy. Deactivate it first."));

        await _repository.DeleteAsync(policy, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Dunning policy {PolicyId} deleted.",
            request.Id);

        return Result.Success();
    }
}
