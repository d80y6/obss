using Mapster;
using Obss.Ticketing.Application.DTOs;
using Obss.Ticketing.Domain.Entities;
using Obss.Ticketing.Domain.ValueObjects;

namespace Obss.Ticketing.Application.Mappings;

public static class TicketMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Ticket, TicketDto>.NewConfig()
            .Map(dest => dest.Priority, src => src.Priority.ToString())
            .Map(dest => dest.Category, src => src.Category.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Source, src => src.Source.ToString())
            .Map(dest => dest.SlaStatus, src => GetSlaStatusString(src))
            .Map(dest => dest.Comments, src => src.Comments.Adapt<List<TicketCommentDto>>())
            .Map(dest => dest.Attachments, src => src.Attachments.Adapt<List<TicketAttachmentDto>>());

        TypeAdapterConfig<Ticket, TicketSummaryDto>.NewConfig()
            .Map(dest => dest.Priority, src => src.Priority.ToString())
            .Map(dest => dest.Category, src => src.Category.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Source, src => src.Source.ToString())
            .Map(dest => dest.SlaStatus, src => GetSlaStatusString(src))
            .Map(dest => dest.CommentCount, src => src.Comments.Count);

        TypeAdapterConfig<TicketComment, TicketCommentDto>.NewConfig();

        TypeAdapterConfig<TicketAttachment, TicketAttachmentDto>.NewConfig();

        TypeAdapterConfig<SlaDefinition, SlaDefinitionDto>.NewConfig()
            .Map(dest => dest.Priority, src => src.Priority.ToString());
    }

    private static string? GetSlaStatusString(Ticket ticket)
    {
        if (!ticket.SlaDeadline.HasValue)
            return null;

        if (ticket.SlaBreachedAt.HasValue)
            return nameof(SlaStatus.Breached);

        if (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed)
            return nameof(SlaStatus.Met);

        var hoursUntilDeadline = (ticket.SlaDeadline.Value - DateTime.UtcNow).TotalHours;

        if (hoursUntilDeadline <= 0)
            return nameof(SlaStatus.Breached);

        if (hoursUntilDeadline <= 1)
            return nameof(SlaStatus.Warning);

        return nameof(SlaStatus.InProgress);
    }
}
