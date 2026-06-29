using Mapster;
using MediatR;
using Obss.Audit.Application.Abstractions;
using Obss.Audit.Application.DTOs;
using Obss.Audit.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.Audit.Application.Commands.AcknowledgeAlert;

public sealed class AcknowledgeAlertCommandHandler : IRequestHandler<AcknowledgeAlertCommand, Result<AuditAlertDto>>
{
    private readonly IAuditAlertRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public AcknowledgeAlertCommandHandler(
        IAuditAlertRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<AuditAlertDto>> Handle(AcknowledgeAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (alert is null)
        {
            return Result.Failure<AuditAlertDto>(Error.NotFound(nameof(AuditAlert), request.Id));
        }

        var userId = _currentUser.UserId ?? "system";
        alert.Acknowledge(userId);

        await _repository.UpdateAsync(alert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(alert.Adapt<AuditAlertDto>());
    }
}
