import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { AuditEntryDto } from "@/api/generated"

export function useAuditLog(entityType: string, entityId: string) {
  return useQuery({
    queryKey: queryKeys.audit.entity(entityType, entityId),
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entities/${entityType}/${entityId}`)
      return res.data as AuditEntryDto[]
    },
    enabled: !!entityId,
  })
}
