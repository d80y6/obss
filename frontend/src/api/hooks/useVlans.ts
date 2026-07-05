import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { VlanDto } from '@/api/generated/dto'

export function useVlans() {
  return useQuery({
    queryKey: queryKeys.networks.vlans.list({}),
    queryFn: async () => {
      const res = await api.get("/api/v1/network/vlans")
      return res.data as VlanDto[]
    },
  })
}
