import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { NetworkElementDto } from '@/api/generated/dto'
import type { UpdateNetworkElementCommand } from '@/api/generated'

export function useUpdateNetworkElement(id: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: UpdateNetworkElementCommand) => {
      const res = await api.put<NetworkElementDto>(`/api/v1/network/elements/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.elements.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.elements.all })
    },
  })
}
