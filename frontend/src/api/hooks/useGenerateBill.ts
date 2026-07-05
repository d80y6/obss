import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { GenerateBillCommand } from '@/api/generated'

export function useGenerateBill() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (data: GenerateBillCommand) => {
      const res = await api.post("/api/v1/billing/bills/generate", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.billing.bills.all })
    },
  })
}
