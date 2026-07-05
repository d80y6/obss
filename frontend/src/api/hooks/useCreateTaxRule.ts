import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { CreateTaxRuleCommand } from '@/api/generated'

export function useCreateTaxRule() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: CreateTaxRuleCommand) => {
      const res = await api.post("/api/v1/billing/tax-rules", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.taxRules.all })
    },
  })
}
