import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { CreateConnectivityLinkCommand } from '@/api/generated'
import type { NetworkLinkDto } from '@/api/generated/dto'

export function useSaveTopologyMap() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateConnectivityLinkCommand) => {
      const res = await api.post<NetworkLinkDto>("/api/v1/network/topology/maps", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.topology.all })
    },
  })
}
