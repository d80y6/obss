import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { VlanDto } from '@/api/generated/dto'
import type { CreateVLANCommand } from '@/api/generated'

export function useCreateVlan() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateVLANCommand) => {
      const res = await api.post<VlanDto>("/api/v1/network/vlans", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.vlans.all })
    },
  })
}
