import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useUpdateLinkStatus(id: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.patch(`/api/v1/network/links/${id}/status`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.links.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.links.lists() })
    },
  })
}
