using Mapster;
using MediatR;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.CRM.Application.Commands.CreateAgreement;

public sealed class CreateAgreementCommandHandler : IRequestHandler<CreateAgreementCommand, Result<AgreementDto>>
{
    private readonly IRepository<Agreement> _agreementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAgreementCommandHandler(IRepository<Agreement> agreementRepository, IUnitOfWork unitOfWork)
    {
        _agreementRepository = agreementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AgreementDto>> Handle(CreateAgreementCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AgreementType>(request.AgreementType, true, out var agreementType))
            return Result.Failure<AgreementDto>(Error.Validation($"Invalid agreement type: {request.AgreementType}"));

        var agreement = new Agreement(request.CustomerId, request.Name, agreementType);

        await _agreementRepository.AddAsync(agreement, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(agreement.Adapt<AgreementDto>());
    }
}
