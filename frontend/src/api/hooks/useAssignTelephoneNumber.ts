import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useAssignTelephoneNumber() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ id, ...data }: { id: string } & Record<string, unknown>) => {
      const res = await api.post(`/api/v1/number-inventory/numbers/${id}/assign`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.numberInventory.numbers.lists() })
      queryClient.invalidateQueries({ queryKey: queryKeys.numberInventory.numbers.details() })
    },
  })
}
