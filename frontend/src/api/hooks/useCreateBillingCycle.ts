import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { GenerateBillingCycleCommand } from '@/api/generated'

export function useCreateBillingCycle() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: GenerateBillingCycleCommand) => {
      const res = await api.post("/api/v1/billing/cycles", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.cycles.all })
    },
  })
}
