import { useQuery } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { ProductOfferingTermDto } from '@/api/generated/dto'

export function useTerms(offerId: string) {
  return useQuery({
    queryKey: ["offers", offerId, "terms"],
    queryFn: async () => {
      const res = await api.get(`/api/v1/catalog/offers/${offerId}/terms`)
      return res.data as ProductOfferingTermDto[]
    },
    enabled: !!offerId,
  })
}
