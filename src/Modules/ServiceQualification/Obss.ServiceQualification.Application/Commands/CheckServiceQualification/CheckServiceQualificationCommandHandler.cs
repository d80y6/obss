using MediatR;
using Mapster;
using Obss.ServiceQualification.Application.Abstractions;
using Obss.ServiceQualification.Application.DTOs;
using Obss.ServiceQualification.Domain.Entities;
using Obss.ServiceQualification.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.ServiceQualification.Application.Commands.CheckServiceQualification;

public sealed class CheckServiceQualificationCommandHandler : IRequestHandler<CheckServiceQualificationCommand, Result<ServiceQualificationDto>>
{
    private readonly IServiceQualificationRepository _repository;
    private readonly IServiceQualificationEngine _engine;
    private readonly IUnitOfWork _unitOfWork;

    public CheckServiceQualificationCommandHandler(
        IServiceQualificationRepository repository,
        IServiceQualificationEngine engine,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _engine = engine;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ServiceQualificationDto>> Handle(
        CheckServiceQualificationCommand command,
        CancellationToken cancellationToken)
    {
        var address = GeographicAddress.Create(
            command.Address.Street,
            command.Address.City,
            command.Address.State,
            command.Address.PostalCode,
            command.Address.Country);

        var qualification = new Domain.Entities.ServiceQualification(
            Guid.NewGuid(),
            command.CustomerId,
            address,
            command.Description);

        foreach (var item in command.RequestedServices)
        {
            qualification.AddItem(Guid.NewGuid(), item.ServiceId, item.ServiceName);
        }

        var engineResult = await _engine.QualifyAsync(
            address,
            command.RequestedServices,
            cancellationToken);

        foreach (var itemResult in engineResult.Items)
        {
            var item = qualification.Items.FirstOrDefault(i => i.ServiceId == itemResult.ServiceId);
            if (item is not null)
            {
                item.SetResult(
                    itemResult.ResultType,
                    itemResult.EstimatedInstallDate,
                    itemResult.EstimatedCompletionDate,
                    itemResult.Reason);

                foreach (var alt in itemResult.AlternateProposals)
                {
                    item.AddAlternateProposal(new AlternateServiceProposal(
                        Guid.NewGuid(),
                        alt.ServiceId,
                        alt.ServiceName,
                        alt.ResultType,
                        alt.EstimatedInstallDate,
                        alt.GuaranteedUntil));
                }
            }
        }

        qualification.Complete();

        await _repository.AddAsync(qualification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = qualification.Adapt<ServiceQualificationDto>();
        return Result.Success(dto);
    }
}
