using Mapster;
using MediatR;
using Obss.NumberInventory.Application.Abstractions;
using Obss.NumberInventory.Application.DTOs;
using Obss.NumberInventory.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.NumberInventory.Application.Commands.AddNumber;

public sealed class AddNumberCommandHandler : IRequestHandler<AddNumberCommand, Result<TelephoneNumberDto>>
{
    private readonly ITelephoneNumberRepository _numberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public AddNumberCommandHandler(
        ITelephoneNumberRepository numberRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _numberRepository = numberRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<TelephoneNumberDto>> Handle(AddNumberCommand request, CancellationToken cancellationToken)
    {
        var telephoneNumber = TelephoneNumber.Create(
            _currentTenant.TenantId ?? string.Empty,
            request.Number,
            request.NumberType,
            request.Cost,
            request.Currency,
            request.Notes);

        await _numberRepository.AddAsync(telephoneNumber, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(telephoneNumber.Adapt<TelephoneNumberDto>());
    }
}
