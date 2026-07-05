import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { OltDto } from '@/api/generated/dto'
import type { CreateOLTCommand } from '@/api/generated'

export function useCreateOlt() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateOLTCommand) => {
      const res = await api.post<OltDto>("/api/v1/network/olts", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.olts.all })
    },
  })
}
