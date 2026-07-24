import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { NasDto } from "@/api/generated/dto"

export function useUpdateNas() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ id, ...data }: {
      id: string
      name: string
      nasIpAddress: string
      nasSecret?: string
      nasType: string
      location?: string
    }) => {
      const res = await api.put(`/api/v1/aaa/nas/${id}`, data)
      return res.data as NasDto
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.nas.detail(variables.id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.nas.lists() })
    },
  })
}
