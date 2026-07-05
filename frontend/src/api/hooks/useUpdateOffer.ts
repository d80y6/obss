import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { OfferDto } from '@/api/generated/dto'

export interface UpdateOfferPayload {
  offerId: string;
  name: string;
  description: string | null;
  offerType: string;
  isContract: boolean;
  contractDurationMonths: number | null;
  billingPeriod: string | null;
  taxInclusive: boolean;
  sortOrder: number;
  validFrom: string | null;
  validTo: string | null;
}

export function useUpdateOffer(id: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: UpdateOfferPayload) => {
      const res = await api.put<OfferDto>(`/api/v1/catalog/offers/${id}`, data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.offers.detail(id) })
      queryClient.invalidateQueries({ queryKey: queryKeys.offers.lists() })
    },
  })
}
