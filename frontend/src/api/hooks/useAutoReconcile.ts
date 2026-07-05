import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useAutoReconcile() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      const res = await api.post("/api/v1/payments/payments/reconciliation/auto")
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.reconciliation.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.unmatched })
    },
  })
}
