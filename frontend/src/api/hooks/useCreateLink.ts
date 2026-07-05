import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { CreateConnectivityLinkCommand } from '@/api/generated'

export function useCreateLink() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateConnectivityLinkCommand) => {
      const res = await api.post("/api/v1/network/links", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.links.all })
    },
  })
}
