import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"

export function useCompletePayment(id: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      const res = await api.post(`/api/v1/payments/payments/${id}/complete`)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.all })
      queryClient.invalidateQueries({ queryKey: queryKeys.payments.summary })
    },
  })
}
