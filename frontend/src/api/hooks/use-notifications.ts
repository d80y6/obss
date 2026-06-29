import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { NotificationDto, NotificationPreferenceDto } from '@/api/generated/dto'

export function useNotifications(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.notifications.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/notifications/notifications?${params.toString()}`)
      return res.data as NotificationDto[]
    },
  })
}

export function useNotification(id: string) {
  return useQuery({
    queryKey: ["notifications", id],
    queryFn: async () => {
      const res = await api.get("/api/v1/notifications/notifications")
      const items = res.data as NotificationDto[]
      return items.find((n) => n.id === id) ?? null
    },
    enabled: !!id,
  })
}

export function useMarkNotificationRead() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/notifications/notifications/${id}/read`)
      return res.data
    },
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ["notifications", id] })
      queryClient.invalidateQueries({ queryKey: queryKeys.notifications.list({}) })
    },
  })
}

export function useNotificationPreferences() {
  return useQuery({
    queryKey: ["notification-preferences"],
    queryFn: async () => {
      const res = await api.get("/api/v1/notifications/preferences")
      return res.data as NotificationPreferenceDto[]
    },
  })
}
