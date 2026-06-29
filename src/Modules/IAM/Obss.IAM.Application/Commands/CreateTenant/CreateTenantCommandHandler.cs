using Mapster;
using MediatR;
using Obss.IAM.Application.DTOs;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Application.Contracts;
using Obss.SharedKernel.Application.Abstractions;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Application.Commands.CreateTenant;

public sealed class CreateTenantCommandHandler : IRequestHandler<CreateTenantCommand, Result<TenantDto>>
{
    private readonly IRepository<Tenant> _tenantRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenantCommandHandler(
        IRepository<Tenant> tenantRepository,
        IRepository<User> userRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<TenantDto>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = Tenant.Create(request.Name, request.Slug, request.ConnectionString, request.Settings);
        await _tenantRepository.AddAsync(tenant, cancellationToken);

        var tenantId = TenantId.Create(tenant.Id.ToString("N"));
        var email = Email.Create(request.AdminEmail);

        var adminUser = User.Create(
            tenantId,
            request.AdminUsername,
            email,
            request.AdminFirstName,
            request.AdminLastName);

        await _userRepository.AddAsync(adminUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tenant.Adapt<TenantDto>());
    }
}
