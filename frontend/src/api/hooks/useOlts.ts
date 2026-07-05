import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { OltDto } from '@/api/generated/dto'

export function useOlts() {
  return useQuery({
    queryKey: queryKeys.networks.olts.list({}),
    queryFn: async () => {
      const res = await api.get("/api/v1/network/olts")
      return res.data as OltDto[]
    },
  })
}
