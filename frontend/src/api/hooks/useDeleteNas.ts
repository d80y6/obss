import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"

export function useDeleteNas() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      await api.delete(`/api/v1/aaa/nas/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.nas.all() })
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.metrics() })
    },
  })
}
