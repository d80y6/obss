import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { RecordCapacityCommand } from '@/api/generated'

export function useRecordCapacity(elementId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: RecordCapacityCommand) => {
      await api.post(`/api/v1/network/elements/${elementId}/capacity`, data)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.elements.detail(elementId) })
      queryClient.invalidateQueries({ queryKey: queryKeys.networks.capacity.all })
    },
  })
}
