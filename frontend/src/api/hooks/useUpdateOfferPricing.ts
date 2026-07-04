import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { OfferPricingDto } from '@/api/generated/dto'
import { queryKeys } from "@/lib/query-keys"

export function useUpdateOfferPricing() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ offerId, pricingId, data }: { offerId: string; pricingId: string; data: Record<string, unknown> }) => {
      const res = await api.put<OfferPricingDto>(`/api/v1/catalog/offers/${offerId}/pricing/${pricingId}`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: queryKeys.offers.detail(variables.offerId) })
    },
  })
}
