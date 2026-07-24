import { useMutation, useQueryClient } from "@tanstack/react-query"
import api from "@/services/api"
import { queryKeys } from "@/lib/query-keys"
import type { NasDto } from "@/api/generated/dto"

export function useCreateNas() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: {
      name: string
      nasIpAddress: string
      nasSecret: string
      nasType: string
      location?: string
    }) => {
      const res = await api.post("/api/v1/aaa/nas", data)
      return res.data as NasDto
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.nas.all() })
      queryClient.invalidateQueries({ queryKey: queryKeys.aaa.metrics() })
    },
  })
}
