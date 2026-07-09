using System.Text.Json;
using Mapster;
using Obss.CRM.Application.DTOs;
using Obss.CRM.Domain.Entities;
using Obss.CRM.Domain.ValueObjects;
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
            .Map(dest => dest.ValidFrom, src => src.ValidFor != null ? src.ValidFor.StartDateTime : null)
            .Map(dest => dest.ValidUntil, src => src.ValidFor != null ? src.ValidFor.EndDateTime : null)
            .Map(dest => dest.Individual, src => src.Individual != null ? src.Individual.Adapt<IndividualDto>() : null)
            .Map(dest => dest.Organization, src => src.Organization != null ? src.Organization.Adapt<OrganizationDto>() : null)
            .Map(dest => dest.Characteristics, src => src.Characteristics.Adapt<List<CharValueDto>>())
            .Map(dest => dest.CreditProfiles, src => src.CreditProfiles.Adapt<List<CreditProfileDto>>())
            .Map(dest => dest.RelatedParties, src => src.RelatedParties.Adapt<List<RelatedPartyDto>>())
            .Map(dest => dest.Contacts, src => src.Contacts.Adapt<List<ContactDto>>())
            .Map(dest => dest.Notes, src => src.Notes.Adapt<List<CustomerNoteDto>>())
            .Map(dest => dest.NotificationHubs, src => src.NotificationHubs.Adapt<List<NotificationHubDto>>())
            .Map(dest => dest.ContactMedia, src => src.ContactMedia.Adapt<List<ContactMediumDto>>())
            .Map(dest => dest.AccountRefs, src => src.AccountRefs.Adapt<List<AccountRefDto>>())
            .Map(dest => dest.AgreementRefs, src => src.AgreementRefs.Adapt<List<AgreementRefDto>>())
            .Map(dest => dest.PaymentMethodRefs, src => src.PaymentMethodRefs.Adapt<List<PaymentMethodRefDto>>())
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

        TypeAdapterConfig<Individual, IndividualDto>.NewConfig()
            .Map(dest => dest.KycStatus, src => src.KycStatus.ToString())
            .Map(dest => dest.RiskRating, src => src.RiskRating.ToString())
            .Map(dest => dest.Documents, src => src.Documents.Adapt<List<IdentityDocumentDto>>());

        TypeAdapterConfig<IdentityDocument, IdentityDocumentDto>.NewConfig()
            .Map(dest => dest.DocumentType, src => src.DocumentType.ToString());

        TypeAdapterConfig<Organization, OrganizationDto>.NewConfig()
            .Map(dest => dest.CompanyType, src => src.CompanyType.ToString())
            .Map(dest => dest.KycStatus, src => src.KycStatus.ToString());

        TypeAdapterConfig<CreditProfile, CreditProfileDto>.NewConfig()
            .Map(dest => dest.ValidFrom, src => src.ValidFor.StartDateTime)
            .Map(dest => dest.ValidUntil, src => src.ValidFor.EndDateTime);

        TypeAdapterConfig<NotificationHub, NotificationHubDto>.NewConfig();
        TypeAdapterConfig<NotificationHubDto, NotificationHub>.NewConfig()
            .ConstructUsing(src => new NotificationHub(src.HubType, src.Identifier, src.IsOptIn, src.ValidFrom, src.ValidUntil));

        TypeAdapterConfig<Agreement, AgreementDto>.NewConfig();

        TypeAdapterConfig<ContactMedium, ContactMediumDto>.NewConfig()
            .Map(dest => dest.Characteristics, src => src.Characteristics.ToList());
        TypeAdapterConfig<ContactMediumDto, ContactMedium>.NewConfig()
            .ConstructUsing(src => new ContactMedium(src.MediumType, src.IsPreferred, src.ValidFrom, src.ValidUntil));

        TypeAdapterConfig<AccountRef, AccountRefDto>.NewConfig();
        TypeAdapterConfig<AgreementRef, AgreementRefDto>.NewConfig();
        TypeAdapterConfig<PaymentMethodRef, PaymentMethodRefDto>.NewConfig();

        TypeAdapterConfig<Quote, QuoteDto>.NewConfig()
            .Map(dest => dest.State, src => src.State.ToString())
            .Map(dest => dest.Items, src => src.Items.Adapt<List<QuoteItemDto>>())
            .Map(dest => dest.RelatedParties, src => src.RelatedParties.Adapt<List<RelatedPartyDto>>())
            .Map(dest => dest.QuotePrices, src => src.QuotePrices.Adapt<List<QuotePriceDto>>())
            .Map(dest => dest.Authorizations, src => src.Authorizations.Adapt<List<QuoteAuthorizationDto>>())
            .Map(dest => dest.BillingAccountRefs, src => src.BillingAccountRefs.Adapt<List<AccountRefDto>>())
            .Map(dest => dest.AgreementRefs, src => src.AgreementRefs.Adapt<List<AgreementRefDto>>())
            .Map(dest => dest.Notes, src => src.Notes.Adapt<List<NoteDto>>());

        TypeAdapterConfig<QuoteItem, QuoteItemDto>.NewConfig()
            .Map(dest => dest.Action, src => src.Action.ToString())
            .Map(dest => dest.State, src => src.State.ToString())
            .Map(dest => dest.Prices, src => src.Prices.Adapt<List<QuotePriceDto>>())
            .Map(dest => dest.ItemRelationships, src => src.ItemRelationships.Adapt<List<QuoteItemRelationshipDto>>())
            .Map(dest => dest.Notes, src => src.Notes.Adapt<List<NoteDto>>());

        TypeAdapterConfig<QuotePrice, QuotePriceDto>.NewConfig()
            .Map(dest => dest.PriceType, src => src.PriceType.ToString());

        TypeAdapterConfig<PriceAlteration, PriceAlterationDto>.NewConfig()
            .Map(dest => dest.PriceType, src => src.PriceType.ToString());

        TypeAdapterConfig<QuoteAuthorization, QuoteAuthorizationDto>.NewConfig()
            .Map(dest => dest.State, src => src.State.ToString());

        TypeAdapterConfig<QuoteItemRelationship, QuoteItemRelationshipDto>.NewConfig();

        TypeAdapterConfig<Note, NoteDto>.NewConfig();
    }
}
