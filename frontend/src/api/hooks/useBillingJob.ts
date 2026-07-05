import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { BillingJobDto } from '@/api/generated/dto'

export function useBillingJob(id: string) {
  return useQuery({
    queryKey: queryKeys.billing.jobs.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/billing/jobs/${id}`)
      return res.data as BillingJobDto
    },
    enabled: !!id,
  })
}
