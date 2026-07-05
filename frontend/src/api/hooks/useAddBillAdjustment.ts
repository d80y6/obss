import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useAddBillAdjustment(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: Record<string, unknown>) => {
      const res = await api.post(`/api/v1/billing/bills/${id}/adjustments`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.bills.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.bills.lists() })
    },
  })
}
