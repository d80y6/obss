import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export interface EventSubscriptionDto {
  id: string
  name: string
  eventType: string
  endpoint: string
  secret: string | null
  filter: Record<string, string> | null
  status: string
  createdAt: string
  lastTriggeredAt: string | null
}

export interface WebhookEventDto {
  id: string
  subscriptionId: string
  eventType: string
  payload: string
  status: string
  retryCount: number
  lastAttemptAt: string | null
  createdAt: string
  errorMessage: string | null
}

export function useEventSubscriptions(filters: Record<string, string> = {}) {
  return useQuery<EventSubscriptionDto[]>({
    queryKey: queryKeys.eventManagement.subscriptions.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams(filters)
      const res = await api.get(`/api/v1/event-management/subscriptions?${params.toString()}`)
      return res.data
    },
  })
}

export function useEventSubscription(id: string) {
  return useQuery<EventSubscriptionDto>({
    queryKey: queryKeys.eventManagement.subscriptions.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/event-management/subscriptions/${id}`)
      return res.data
    },
    enabled: !!id,
  })
}

export function useCreateEventSubscription() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Partial<EventSubscriptionDto>) => {
      const res = await api.post("/api/v1/event-management/subscriptions", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.eventManagement.subscriptions.all() })
    },
  })
}

export function useWebhookEvents(filters: Record<string, string> = {}) {
  return useQuery<WebhookEventDto[]>({
    queryKey: queryKeys.eventManagement.events.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams(filters)
      const res = await api.get(`/api/v1/event-management/events?${params.toString()}`)
      return res.data
    },
  })
}

export function usePublishEvent() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: { eventType: string; payload: Record<string, unknown> }) => {
      const res = await api.post("/api/v1/event-management/events/publish", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.eventManagement.events.all() })
    },
  })
}
