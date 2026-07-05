import { useMutation, useQueryClient } from "@tanstack/react-query"
import { queryKeys } from "@/lib/query-keys"
import { api } from "@/api/client"
import type { OfferDto } from '@/api/generated/dto'

export interface CreateOfferPayload {
  name: string;
  description: string | null;
  productId: string;
  offerType: string;
  isContract: boolean;
  contractDurationMonths: number | null;
  billingPeriod: string | null;
  taxInclusive: boolean;
  sortOrder: number;
  validFrom: string | null;
  validTo: string | null;
  pricings: {
    pricingType: string;
    currency: string;
    recurringPrice: number;
    oneTimePrice: number;
    usagePrice: number;
    unitOfMeasure: string | null;
    minQuantity: number | null;
    maxQuantity: number | null;
    isActive: boolean;
  }[];
}

export function useCreateOffer() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateOfferPayload) => {
      const res = await api.post<OfferDto>("/api/v1/catalog/offers", data)
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: queryKeys.offers.lists() })
      queryClient.invalidateQueries({ queryKey: queryKeys.products.all })
    },
  })
}
