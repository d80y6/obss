import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { NotificationTemplateDto } from '@/api/generated/dto'
import type { CreateNotificationTemplateCommand, UpdateTemplateCommand } from '@/api/generated'

export function useNotificationTemplates(search = "") {
  return useQuery({
    queryKey: ["notification-templates", search],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set("search", search)
      const res = await api.get(`/api/v1/notifications/templates?${params.toString()}`)
      return res.data as NotificationTemplateDto[]
    },
  })
}

export function useNotificationTemplate(id: string) {
  return useQuery({
    queryKey: ["notification-templates", id],
    queryFn: async () => {
      const res = await api.get("/api/v1/notifications/templates")
      const items = res.data as NotificationTemplateDto[]
      return items.find((t) => t.id === id) ?? null
    },
    enabled: !!id,
  })
}

export function useCreateNotificationTemplate() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateNotificationTemplateCommand) => {
      const res = await api.post("/api/v1/notifications/templates", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notification-templates"] })
    },
  })
}

export function useUpdateNotificationTemplate() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, data }: { id: string; data: UpdateTemplateCommand }) => {
      const res = await api.put(`/api/v1/notifications/templates/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notification-templates"] })
    },
  })
}
