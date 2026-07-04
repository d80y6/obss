import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { PriceRangeDto } from '@/api/generated/dto'

export function useUpdatePriceRange() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({ offerId, pricingId, rangeId, data }: { offerId: string; pricingId: string; rangeId: string; data: Record<string, unknown> }) => {
      const res = await api.put<PriceRangeDto>(`/api/v1/catalog/offers/${offerId}/pricing/${pricingId}/price-ranges/${rangeId}`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["price-ranges", variables.pricingId] })
    },
  })
}
