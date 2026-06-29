using System.Text.Json;
using Mapster;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.SharedKernel.Domain.ValueObjects;

namespace Obss.CRM.Application.Mappings;

public static class CrmMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Customer, CustomerDto>.NewConfig()
            .Map(dest => dest.TenantId, src => src.TenantId)
            .Map(dest => dest.CustomerType, src => src.CustomerType.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber != null ? src.PhoneNumber.FullNumber : null)
            .Map(dest => dest.Contacts, src => src.Contacts.Adapt<List<ContactDto>>())
            .Map(dest => dest.Notes, src => src.Notes.Adapt<List<CustomerNoteDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<Contact, ContactDto>.NewConfig()
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber != null ? src.PhoneNumber.FullNumber : null)
            .Map(dest => dest.MobileNumber, src => src.MobileNumber != null ? src.MobileNumber.FullNumber : null)
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<CustomerNote, CustomerNoteDto>.NewConfig()
            .Map(dest => dest.Category, src => src.Category.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<CustomerSegment, CustomerSegmentDto>.NewConfig()
            .Map(dest => dest.Criteria, src => src.Criteria)
            .Map(dest => dest.Id, src => src.Id)
            .MapWith(src => new CustomerSegmentDto(
                src.Id,
                src.TenantId,
                src.Name,
                src.Description,
                JsonSerializer.Serialize(src.Criteria, (JsonSerializerOptions?)null),
                src.Priority,
                src.IsActive,
                0,
                src.CreatedAt,
                src.UpdatedAt));

        TypeAdapterConfig<CustomerSegmentAssignment, CustomerSegmentAssignmentDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);
    }
}
