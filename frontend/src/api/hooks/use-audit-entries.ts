import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { AuditEntryDto } from '@/api/generated/dto'

export function useAuditEntries(filters: Record<string, string> = {}) {
  return useQuery({
    queryKey: queryKeys.audit.entries.list(filters),
    queryFn: async () => {
      const params = new URLSearchParams()
      Object.entries(filters).forEach(([k, v]) => { if (v) params.set(k, v) })
      const res = await api.get(`/api/v1/audit/entries?${params.toString()}`)
      return res.data as AuditEntryDto[]
    },
  })
}

export function useAuditEntry(id: string) {
  return useQuery({
    queryKey: ["audit", "entries", id],
    queryFn: async () => {
      const res = await api.get(`/api/v1/audit/entries/${id}`)
      return res.data as AuditEntryDto
    },
    enabled: !!id,
  })
}
