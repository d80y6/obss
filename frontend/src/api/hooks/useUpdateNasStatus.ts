import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { NasDto } from "@/api/generated/dto"

export function useUpdateNasStatus() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, status }: { id: string; status: string }) => {
      const res = await api.patch(`/api/v1/aaa/nas/${id}/status`, { status })
      return res.data as NasDto
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.nas.detail(variables.id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.nas.lists() })
    },
  })
}
