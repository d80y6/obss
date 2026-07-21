using Mapster;
using MediatR;
using Obss.AAA.Application.Abstractions;
using Obss.AAA.Application.DTOs;
using Obss.AAA.Domain.Entities;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Application.Contracts;

namespace Obss.AAA.Application.Commands.RegisterNas;

public sealed class RegisterNasCommandHandler : IRequestHandler<RegisterNasCommand, Result<NasDto>>
{
    private readonly INasRepository _nasRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentTenant _currentTenant;

    public RegisterNasCommandHandler(
        INasRepository nasRepository,
        IUnitOfWork unitOfWork,
        ICurrentTenant currentTenant)
    {
        _nasRepository = nasRepository;
        _unitOfWork = unitOfWork;
        _currentTenant = currentTenant;
    }

    public async Task<Result<NasDto>> Handle(RegisterNasCommand request, CancellationToken cancellationToken)
    {
        var nas = NetworkAccessServer.Create(
            _currentTenant.TenantId ?? string.Empty,
            request.Name,
            request.NasIpAddress,
            request.NasSecret,
            request.NasType,
            request.Location);

        await _nasRepository.AddAsync(nas, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(nas.Adapt<NasDto>());
    }
}
