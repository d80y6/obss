using MediatR;
using Obss.NetworkInventory.Domain.Entities;
using Obss.NetworkInventory.Domain.ValueObjects;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;

namespace Obss.NetworkInventory.Application.Commands.UpdateLinkStatus;

public sealed class UpdateLinkStatusCommandHandler : IRequestHandler<UpdateLinkStatusCommand, Result>
{
    private readonly IRepository<ConnectivityLink> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLinkStatusCommandHandler(IRepository<ConnectivityLink> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateLinkStatusCommand request, CancellationToken cancellationToken)
    {
        var link = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (link is null)
            return Result.Failure(Error.NotFound("ConnectivityLink", request.Id));

        var status = Enum.Parse<LinkStatus>(request.Status);

        switch (status)
        {
            case LinkStatus.Active:
                link.BringUp();
                break;
            case LinkStatus.Down:
                link.TakeDown(request.Reason ?? "Administratively down");
                break;
            case LinkStatus.Maintenance:
                link.SetMaintenance();
                break;
            case LinkStatus.Degraded:
                link.TakeDown(request.Reason ?? "Link degraded");
                break;
            default:
                return Result.Failure(Error.Validation("Invalid link status."));
        }

        await _repository.UpdateAsync(link, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
