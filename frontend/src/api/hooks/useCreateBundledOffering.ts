import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { BundledProductOfferingDto } from '@/api/generated/dto'

export interface CreateBundledOfferingPayload {
  offerId: string;
  bundledOfferId: string;
  name: string | null;
  quantity: number;
  referralType: string | null;
}

export function useCreateBundledOffering() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateBundledOfferingPayload) => {
      const res = await api.post<BundledProductOfferingDto>(`/api/v1/catalog/offers/${data.offerId}/bundled-offerings`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["offers", variables.offerId, "bundled-offerings"] })
    },
  })
}
