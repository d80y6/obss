using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.UpdateAgreement;

public sealed class UpdateAgreementCommandHandler : IRequestHandler<UpdateAgreementCommand, Result<AgreementDto>>
{
    private readonly IRepository<Agreement> _agreementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAgreementCommandHandler(IRepository<Agreement> agreementRepository, IUnitOfWork unitOfWork)
    {
        _agreementRepository = agreementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AgreementDto>> Handle(UpdateAgreementCommand request, CancellationToken cancellationToken)
    {
        var agreement = await _agreementRepository.GetByIdAsync(request.Id, cancellationToken);
        if (agreement is null)
            return Result.Failure<AgreementDto>(Error.NotFound("Agreement", request.Id));

        agreement.UpdateDetails(request.Name, request.Description);

        await _agreementRepository.UpdateAsync(agreement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(agreement.Adapt<AgreementDto>());
    }
}
