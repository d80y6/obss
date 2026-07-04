import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { PriceRangeDto } from '@/api/generated/dto'
import { queryKeys } from "@/lib/query-keys"

export interface CreatePriceRangePayload {
  offerId: string;
  pricingId: string;
  minQuantity: number;
  maxQuantity: number | null;
  price: number;
}

export function useCreatePriceRange() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreatePriceRangePayload) => {
      const res = await api.post<PriceRangeDto>(`/api/v1/catalog/offers/${data.offerId}/pricing/${data.pricingId}/price-ranges`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["price-ranges", variables.pricingId] })
      queryClient.invalidateQueries({ queryKey: queryKeys.offers.detail(variables.offerId) })
    },
  })
}
