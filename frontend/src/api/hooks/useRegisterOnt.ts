import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useRegisterOnt(oltId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post(`/api/v1/network/olts/${oltId}/register-ont`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.olts.registerOnt(oltId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.olts.detail(oltId) })
    },
  })
}
