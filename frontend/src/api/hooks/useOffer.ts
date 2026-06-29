import { useQuery } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { OfferDto } from '@/api/generated/dto'

export function useOffer(id: string) {
  return useQuery({
    queryKey: queryKeys.offers.detail(id),
    queryFn: async () => {
      const res = await api.get(`/api/v1/catalog/offers/${id}`)
      return res.data as OfferDto
    },
    enabled: !!id,
  })
}
