import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { OltDto } from '@/api/generated/dto'

export function useOlt(id: string) {
  return useQuery({
    queryKey: queryKeys.networks.olts.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/network/olts/${id}`)
      return res.data as OltDto
    },
    enabled: !!id,
  })
}
