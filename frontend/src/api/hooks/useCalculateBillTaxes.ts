import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useCalculateBillTaxes() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (id: string) => {
      const res = await api.post(`/api/v1/billing/bills/${id}/calculate-taxes`)
      return res.data
    },
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.bills.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.bills.lists() })
    },
  })
}
