import { useQuery } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { PriceRangeDto } from '@/api/generated/dto'

export function usePriceRanges(offerId: string, pricingId: string) {
  return useQuery({
    queryKey: ["price-ranges", pricingId],
    queryFn: async () => {
      const res = await api.get(`/api/v1/catalog/offers/${offerId}/pricing/${pricingId}/price-ranges`)
      return res.data as PriceRangeDto[]
    },
    enabled: !!offerId && !!pricingId,
  })
}
