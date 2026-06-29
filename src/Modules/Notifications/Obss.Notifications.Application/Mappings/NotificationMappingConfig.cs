using Mapster;
using Obss.Notifications.Application.DTOs;
using Obss.Notifications.Domain.Entities;

namespace Obss.Notifications.Application.Mappings;

public static class NotificationMappingConfig
{
    public static void Configure()
    {
        TypeAdapterConfig<Notification, NotificationDto>.NewConfig()
            .Map(dest => dest.NotificationType, src => src.NotificationType.ToString())
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.Priority, src => src.Priority.ToString());

        TypeAdapterConfig<NotificationTemplate, NotificationTemplateDto>.NewConfig()
            .Map(dest => dest.NotificationType, src => src.NotificationType.ToString())
            .Map(dest => dest.Variables, src => src.GetVariableList());

        TypeAdapterConfig<NotificationPreference, NotificationPreferenceDto>.NewConfig()
            .Map(dest => dest.NotificationType, src => src.NotificationType.ToString());
    }
}
