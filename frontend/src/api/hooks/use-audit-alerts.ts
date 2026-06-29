import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { AuditAlertDto } from '@/api/generated/dto'

export function useAuditAlerts() {
  return useQuery({
    queryKey: ["audit-alerts-list"],
    queryFn: async () => {
      const res = await api.get("/api/v1/audit/alerts")
      return res.data as AuditAlertDto[]
    },
  })
}

export function useAcknowledgeAlert() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/audit/alerts/${id}/acknowledge`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["audit-alerts-list"] })
    },
  })
}
