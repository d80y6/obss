using Mapster;
using Obss.IAM.Application.DTOs;
using Obss.IAM.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.IAM.Application.Mappings;

public static class IamMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<User, UserDto>.NewConfig()
            .Map(dest => dest.TenantId, src => src.TenantId.Value)
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber != null ? src.PhoneNumber.FullNumber : null)
            .Map(dest => dest.Roles, src => src.UserRoles.Select(ur => ur.Role).Adapt<List<RoleDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<Role, RoleDto>.NewConfig()
            .Map(dest => dest.TenantId, src => src.TenantId)
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<Permission, PermissionDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<Tenant, TenantDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);
    }
}
