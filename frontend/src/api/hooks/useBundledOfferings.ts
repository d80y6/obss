import { useQuery } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { BundledProductOfferingDto } from '@/api/generated/dto'

export function useBundledOfferings(offerId: string) {
  return useQuery({
    queryKey: ["offers", offerId, "bundled-offerings"],
    queryFn: async () => {
      const res = await api.get(`/api/v1/catalog/offers/${offerId}/bundled-offerings`)
      return res.data as BundledProductOfferingDto[]
    },
    enabled: !!offerId,
  })
}
