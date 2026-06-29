using Mapster;
using Obss.Invoices.Application.Abstractions;
using Obss.Invoices.Application.DTOs;
using Obss.Invoices.Domain.Entities;

namespace Obss.Invoices.Application.Mappings;

public static class InvoiceMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Invoice, InvoiceDto>.NewConfig()
            .Map(dest => dest.TenantId, src => src.TenantId.Value)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Lines, src => src.Lines.Adapt<List<InvoiceLineDto>>())
            .Map(dest => dest.Payments, src => src.Payments.Adapt<List<InvoicePaymentDto>>())
            .Map(dest => dest.NotesList, src => src.NotesCollection.Adapt<List<InvoiceNoteDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<InvoiceLine, InvoiceLineDto>.NewConfig()
            .Map(dest => dest.LineType, src => src.LineType.ToString())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<InvoicePayment, InvoicePaymentDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<InvoiceNote, InvoiceNoteDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<CreditNote, CreditNoteDto>.NewConfig()
            .Map(dest => dest.TenantId, src => src.TenantId.Value)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Lines, src => src.Lines.Adapt<List<CreditNoteLineDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<CreditNoteLine, CreditNoteLineDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<InvoiceSummary, InvoiceSummaryDto>.NewConfig();

        TypeAdapterConfig<InvoiceDispute, InvoiceDisputeDto>.NewConfig()
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Attachments, src => src.Attachments.Adapt<List<DisputeAttachmentDto>>())
            .Map(dest => dest.Id, src => src.Id);

        TypeAdapterConfig<DisputeAttachment, DisputeAttachmentDto>.NewConfig()
            .Map(dest => dest.Id, src => src.Id);
    }
}
