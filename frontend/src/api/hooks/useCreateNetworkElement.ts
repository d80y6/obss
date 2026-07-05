import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { NetworkElementDto } from '@/api/generated/dto'
import type { CreateNetworkElementCommand } from '@/api/generated'

export function useCreateNetworkElement() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateNetworkElementCommand) => {
      const res = await api.post<NetworkElementDto>("/api/v1/network/elements", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.elements.all })
    },
  })
}
