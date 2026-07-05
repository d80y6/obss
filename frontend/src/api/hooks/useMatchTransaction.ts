import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useMatchTransaction() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ reconciliationId, itemId, matchedPaymentId }: {
      reconciliationId: string
      itemId: string
      matchedPaymentId: string
    }) => {
      const res = await api.post(
        `/api/v1/payments/payments/reconciliation/${reconciliationId}/match`,
        { itemId, matchedPaymentId }
      )
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.reconciliation.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.unmatched })
    },
  })
}
