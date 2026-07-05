import { useMutation, useQueryClient } from "@tanstack/react-query"
import { api } from "@/api/client"
import type { ProductOfferingTermDto } from '@/api/generated/dto'
import { queryKeys } from "@/lib/query-keys"

export interface CreateTermPayload {
  offerId: string;
  name: string;
  description: string | null;
  duration: number;
  durationUnit: string;
  termType: string;
  validFrom: string | null;
  validTo: string | null;
}

export function useCreateTerm() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (data: CreateTermPayload) => {
      const res = await api.post<ProductOfferingTermDto>(`/api/v1/catalog/offers/${data.offerId}/terms`, data)
      return res.data
    },
    onSuccess: (_data, variables) => {
      queryClient.invalidateQueries({ queryKey: ["offers", variables.offerId, "terms"] })
      queryClient.invalidateQueries({ queryKey: queryKeys.offers.detail(variables.offerId) })
    },
  })
}
